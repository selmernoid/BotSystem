using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BotSystem.Models;
using DataContract;
using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;

using log4net;
using Newtonsoft.Json;
using Quartz;
using RestSharp;
using RestSharp.Extensions;

namespace BotSystem.Jobs {
    public class PostCommentsGrabber {
        private static readonly ILog log = LogManager.GetLogger("Grabber");
        private static readonly ILog AltLog = LogManager.GetLogger("AltLog");
        private HtmlDocument html;
        private RestClient client;
        public PostCommentsGrabber() {
            html = new HtmlDocument();
            client = new RestClient("http://pikabu.ru");

            var sessionToken = "89bc439l4l47n8m6a54kbvdl132isfh2";// "cghv6e71q63j2nklf6mapucp3k4fpnq9";
            client.AddDefaultHeader("X-Csrf-Token", sessionToken);
            client.AddDefaultParameter("PHPSESS", sessionToken, ParameterType.Cookie);
            client.AddDefaultParameter("is_scrollmode", 1, ParameterType.Cookie);
        }

        public void GragPost(int postId) {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            //            html = new HtmlWeb().Load(UrlConstants.SpecifiedPost + postId);
            //            using (var client = new WebClient()) {
            //                html.LoadHtml(client.DownloadString(UrlConstants.SpecifiedPost + postId));
            //            }

            //http://pikabu.ru/story/afrika_obedinyonnaya_respublika_tanzaniya_kakoyto_znakomyiy_vid_4820359#comment_81152709

            var restRequest = new RestRequest(UrlConstants.SpecifiedPost + postId, Method.GET);
            
            var restResponse = client.Execute(restRequest);
            var en = Encoding.GetEncoding("windows-1251");
            var ar = Encoding.Convert(Encoding.Default, en, Encoding.Default.GetBytes(restResponse.Content)).ToString();
            html.LoadHtml(ar);


            var processingDateTime = DateTime.Now;

            var post = html.DocumentNode.QuerySelector(".story");
            if (post == null)
                return;
            var authorName = post.QuerySelector(".story__author").InnerText;
            if (authorName == "ads")
                return;


            // empty link video url

            // removed comment


            using (var db = new DataContext()) {

//                var user = PostProcessingManager.GetUser(db, authorName);

                var postInfoNode = post.QuerySelector(".story__toggle-button");
                Post dbPostEntry;


                var postRate = post.QuerySelector(".story__rating-count");
                var isDeleted = postRate.QuerySelectorAll("i.i-sprite--feed__rating-trash").Any(); //4801869 deleted
                var rateNotAvailable = postRate.QuerySelectorAll("i").Any();
                var postTitle = post.QuerySelector(".story__header-title a.story__title-link").InnerText;
                var postContent = post.QuerySelector(".b-story__content")?.InnerHtml;
                var postCreateDate = Helper.UnixTimeStampToDateTime(
                    double.Parse(
                        post.QuerySelector(".story__date").Attributes.AttributesWithName("title").FirstOrDefault().Value));
                var commentsCount = Int32.Parse(post.QuerySelector(".story__comments-count").InnerText.Split(' ')[0]);
                var postType = postInfoNode.Attributes.AttributesWithName("data-story-type").FirstOrDefault().Value;
                var isLong = postInfoNode.Attributes.AttributesWithName("data-story-long").FirstOrDefault().Value.Equals("true", StringComparison.CurrentCultureIgnoreCase);
                db.Posts.Add(dbPostEntry = new Post {
                    Id = postId,
                    Title = postTitle,
                    Content = postContent,
                    Created = postCreateDate,
                    LastCheck = processingDateTime,

                    CommentsCount = commentsCount,
                    IsDeleted = isDeleted,
                    Rating = rateNotAvailable ? (int?) null : int.Parse(postRate.InnerText),
                    AuthorName = authorName,
//                    Author = user,
                    Type = postType,
                    IsLong = isLong,
                    IsMine = post.QuerySelectorAll(".story__header-title a.story__authors").Any(),
                    IsStraw = post.QuerySelectorAll(".story__header-additional a.story__straw").Any(),

                    Tags = new List<PostTag>(),
                    PostLinks = new List<PostLink>(),
                    UserComments = new List<UserComment>()
                });


                foreach (var comment in html.DocumentNode.QuerySelectorAll(".b-comments_type_main .b-comment")) {
                    CommentProcessing(comment, dbPostEntry);
                }

                if (dbPostEntry.CommentsCount > dbPostEntry.UserComments.Count)
                    AjaxUploadRestComments(dbPostEntry);

                var postCommunity = post.QuerySelector(".story__author + a");
                if (postCommunity != null) {
                    var community = db.Communities.FirstOrDefault(x => x.Name == postCommunity.InnerText);
                    if (community == null)
                        db.Communities.Add(community = new Community {
                            Name = postCommunity.InnerText,
                            Link = postCommunity.GetAttributeValue("href", (string) null)
                        });
                    dbPostEntry.Community = community;
                }
//                    var a = db.ChangeTracker.Entries().Where(e => e.State != System.Data.Entity.EntityState.Unchanged).ToList();
//                    var a2 = db.ChangeTracker.Entries().Where(e => e.State == System.Data.Entity.EntityState.Added).ToList();
//                    db.SaveChanges();
                PostProcessingManager.ProcessingTags(db, dbPostEntry, post.QuerySelectorAll(".story__tag").Select(x => x.InnerText).ToList());
                post.QuerySelectorAll("[data-large-image]")
                    .Select(x => x.ParentNode.GetAttributeValue("href", null)).ToList().ForEach((link) => {
                        dbPostEntry.PostLinks.Add(
                            new PostLink {
                                Url = link,
                                Type = LinkType.Image
                            });
                    });
                post.QuerySelectorAll(".b-video")
                    .Select(x => {
                        var url = x.GetAttributeValue("data-url", null);
                        return new PostLink {
                            Url = url,
                            DataId = //postType == "video" ?
                                url.Substring(url.LastIndexOf('/') + 1),
                            //VideoCodeRegex.Match(url).Value : x.GetAttributeValue("data-id", null),
                            Type = LinkType.Video
                        };
                    }).ToList().ForEach((link) => {
                        dbPostEntry.PostLinks.Add(link);
                    });
                post.QuerySelectorAll(".b-gifx__player")
                    .Select(x =>
                        new PostLink {
                            Url = x.GetAttributeValue("data-src", null),
                            Type = LinkType.Gif
                        }
                    ).ToList().ForEach((link) => {
                        dbPostEntry.PostLinks.Add(link);
                    });
                post.QuerySelectorAll("noindex > a[rel=\"nofollow\"]")
                    .Select(x => x.GetAttributeValue("href", null)).ToList().ForEach(
                        (address) => {
                            var link = GetRefLinkByUrl<PostLink>(address);
                            dbPostEntry.PostLinks.Add(link);
                        });
//                    db.SaveChanges();
                timer.Stop();
                dbPostEntry.ProcessedTime = timer.Elapsed;
                db.SaveChanges();
            }
            //            for (int i = 0; i < 10; i++)
            //                Console.WriteLine(JsonConvert.SerializeObject(commentInfo[i]));

            //            IList<HtmlNode> nodes = html.QuerySelectorAll("div .my-class[data-attr=123] > ul li");
            //            HtmlNode node = nodes[0].QuerySelector("p.with-this-class span[data-myattr]");
            //            html.DocumentElement.SelectNodes("//a[@href")

            //            throw new NotImplementedException();
            //            });

            //            task.Wait();

//            AltLog.Error($"{postId} #1 {altTimer.Elapsed}");
        }

        private void AjaxUploadRestComments(Post post) {
            var ajaxCalls = 0;
            var lastCommentId = post.UserComments.Last().Id;
//            var lastCommentId = post.UserComments.First().Id;

            var currentTotal = 0;//post.CommentsCount;
            while (currentTotal < post.UserComments.Count && post.CommentsCount > post.UserComments.Count && ajaxCalls++ < 25) {
                currentTotal = post.UserComments.Count;
                var ajaxModel = GetAjaxComments(post.Id, lastCommentId);

                foreach (var comment in ajaxModel.data.comments) {
                    if (post.UserComments.Any(x=>x.Id == comment.id))
                        continue;
                    var commentHtml = new HtmlDocument();
                    commentHtml.LoadHtml(comment.html);
                    foreach (var commentNode in commentHtml.DocumentNode.QuerySelectorAll(".b-comment")) {
                        CommentProcessing(commentNode, post);
                    }
                }

                if (ajaxModel.data.last_id.HasValue) {
                    lastCommentId = ajaxModel.data.last_id.Value;
                } else
                    break;
            }
            if (ajaxCalls == 25) {
                log.Error($"T3. Ajax infinite loop. PostId#{post.Id}. Loaded: {post.UserComments.Count}. Required: {post.CommentsCount}.");
            } else if (currentTotal == post.UserComments.Count) {
                log.Error($"T4. Ajax dont read all required. PostId#{post.Id}. Loaded: {post.UserComments.Count}. Required: {post.CommentsCount}.");
            }
        }

        private AjaxComments GetAjaxComments(int id, int lastCommentId) {

            var url = "ajax/comments_actions.php";
//            var client = new RestClient(url);

//            var sessionToken = "89bc439l4l47n8m6a54kbvdl132isfh2";// "cghv6e71q63j2nklf6mapucp3k4fpnq9";
//            client.AddDefaultHeader("X-Csrf-Token", sessionToken);
            //            client.AddDefaultHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");

            var request = new RestRequest(url, Method.POST);
            request.AddParameter("action", "get_story_comments");
            request.AddParameter("story_id", id);
            request.AddParameter("start_comment_id", lastCommentId);

//            request.AddParameter("PHPSESS", sessionToken, ParameterType.Cookie);

            // execute the request
            var result=  client.Execute(request);
            var responseData = JsonConvert.DeserializeObject<AjaxComments>(result.Content);
            return responseData;
//            client.DownloadData(request).SaveAs($"f:\\t1\\{startCommentId}.json");
//            throw new NotImplementedException();
        }

        private static void CommentProcessing(HtmlNode comment, Post dbPostEntry) {
            var parentId = int.Parse(comment.Attributes.AttributesWithName("data-parent-id").FirstOrDefault().Value);
            var commentId = int.Parse(comment.Attributes.AttributesWithName("data-id").FirstOrDefault().Value);


            var tempHeader = comment.QuerySelector(".b-comment__header");
            var tempUser = tempHeader.QuerySelector(".b-comment__user");
            var tempRate = tempHeader.QuerySelector(".b-comment__rating-count");

            var notYetCommentRate = tempRate.QuerySelector("i") != null;
            var userName = tempUser.QuerySelector("a span").InnerText;
            var commentDateTime = Helper.UnixTimeStampToDateTime(
                double.Parse(
                    tempUser.QuerySelector("time.b-comment__time").Attributes.AttributesWithName("datetime").FirstOrDefault().Value));
            //                    var content = tempHeader.QuerySelector(".b-comment__content");
            HtmlNode content = tempHeader.NextSibling;
            while (content.GetAttributeValue("class", null) != "b-comment__content") {
                content = content.NextSibling;
            }

            var level = int.Parse(comment.Attributes.AttributesWithName("data-level").FirstOrDefault().Value);
            var commentInfo = new UserComment {
                Id = commentId,
                UserName = userName,
                IsPostAuthor = userName.Equals(dbPostEntry.AuthorName),
                Content = content.InnerHtml,
                Rating = notYetCommentRate ? (int?) null : int.Parse(tempRate.InnerText),
                ParentCommentId = parentId == 0 ? (int?) null : parentId,
                Level = level,
                Created = commentDateTime,
//                Post = dbPostEntry,
                Links = new List<CommentLink>()
            };

            content.QuerySelectorAll(".b-p_type_image > div.b-gifx > a")
                .Select(x => x.GetAttributeValue("href", null)).ToList().ForEach(
                    (address) => {
                        commentInfo.Links.Add(new CommentLink {
                            Url = address,
                            Type = LinkType.Gif
                        });
                    });
            content.QuerySelectorAll("noindex > a[rel=\"nofollow\"]")
                .Select(x => x.GetAttributeValue("href", null)).ToList().ForEach(
                    (address) => {
                        var link = GetRefLinkByUrl<CommentLink>(address);
                        commentInfo.Links.Add(link);
                    });
            dbPostEntry.UserComments.Add(commentInfo);
        }

        private static readonly Regex PostIdRegex = new Regex(@"\d+$");
        private static readonly Regex CommentRegex = new Regex(@"comment_\d+$");
        private static readonly Regex ProfileRegex = new Regex(@"/profile/\w+$");

        private static T GetRefLinkByUrl<T>(string address) where T : LinkBase, new() {
            if (address.StartsWith("/away.php?"))
                return new T {
                    Url = address,
                    Type = LinkType.ExternalAwayUrl,
                };
            var url = new Uri(address);

            T link = new T {Url = address};
            if (url.DnsSafeHost.Equals("pikabu.ru", StringComparison.CurrentCultureIgnoreCase)) {
                if (url.AbsolutePath.StartsWith("/story/", true, CultureInfo.InvariantCulture)) {
                    link.OtherPostId = int.Parse(PostIdRegex.Match(url.AbsolutePath).Value); //int.Parse(url.AbsolutePath.Substring(url.AbsolutePath.LastIndexOf('_') + 1));
                    if (CommentRegex.IsMatch(url.Fragment)) {
                        link.OtherCommentId = int.Parse(PostIdRegex.Match(url.Fragment).Value);
                        link.Type = LinkType.Comment;
                    }
                    else
                        link.Type = LinkType.Post;
                }
                else if (ProfileRegex.IsMatch(url.AbsolutePath)) {
                    link.Type = LinkType.User;
                    var userName = url.Segments[2];
                    link.UserName = userName;
//                    link.User = PostProcessingManager.GetUser(db, userName);
                }
                else
                    link.Type = LinkType.NotRecognized;
            }
            else {
                link.Type = LinkType.ExternalUrl;
            }
            return link;
        }
    }
}
