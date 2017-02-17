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
using DataContract;
using HtmlAgilityPack;
using log4net;
using Newtonsoft.Json;
using Quartz;

namespace BotSystem.Jobs {
    public class PostCommentsGrabber : IJob {
        private static readonly ILog log = LogManager.GetLogger("Grabber");
        public static void GragPost(int postId) {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            //            throw new NotSupportedException("log4 testing");


                HtmlDocument html = new HtmlDocument();
            using (WebClient client = new WebClient()) {
                html.LoadHtml(client.DownloadString(UrlConstants.SpecifiedPost+ postId));
            }
            var processingDateTime = DateTime.Now;

            var post = html.QuerySelector(".story");
            if (post == null)
                return;
            var authorName = post.QuerySelector(".story__author").InnerText;
            if (authorName == "ads")
                return;

            var comments = html.QuerySelectorAll(".b-comments_type_main .b-comment");
            var commentInfos = new List<CommentInfo>();

            using (var db = new DataContext()) {
                foreach (var comment in comments) {
                var parentId = int.Parse(comment.Attributes.AttributesWithName("data-parent-id").FirstOrDefault().Value);
                var commentId = int.Parse(comment.Attributes.AttributesWithName("data-id").FirstOrDefault().Value);
                var userName = comment.QuerySelector(".b-comment__user a span").InnerText;
                var content = comment.QuerySelector(".b-comment__content").InnerHtml;
                var notYetCommentRate = comment.QuerySelectorAll(".b-comment__rating-count i").Any();
                var level = int.Parse(comment.Attributes.AttributesWithName("data-level").FirstOrDefault().Value);
                var commentDateTime = Helper.UnixTimeStampToDateTime(
                    double.Parse(
                        comment.QuerySelector(".b-comment__user time.b-comment__time").Attributes.AttributesWithName("datetime").FirstOrDefault().Value));
                var commentInfo = new CommentInfo {
                    Id = commentId,
                    UserName = userName,
                    Content = content,
                    Rating = notYetCommentRate ? (int?)null : int.Parse(comment.QuerySelector(".b-comment__rating-count").InnerText),
                    ParentCommentId = parentId == 0 ? (int?) null : parentId,
                    Level = level,
                    DateTime = commentDateTime,
                    Links = new List<CommentLink>()
                };

                comment.QuerySelector(".b-comment__content").QuerySelectorAll(".b-p_type_image > div.b-gifx > a")
                    .Select(x => x.GetAttributeValue("href", null)).ToList().ForEach(
                    (address) => {
                        commentInfo.Links.Add(new CommentLink {
                            Url = address,
                            Type = LinkType.Gif
                        });
                    });
                comment.QuerySelector(".b-comment__content").QuerySelectorAll("noindex > a[rel=\"nofollow\"]")
                    .Select(x => x.GetAttributeValue("href", null)).ToList().ForEach(
                    (address) => {
                        var link = GetRefLinkByUrl<CommentLink>(address, db);
                        commentInfo.Links.Add(link);
                    });
                commentInfos.Add(commentInfo);
            }

                var user = db.Users.FirstOrDefault(x => x.Name.ToLower() == authorName) ;
                if (user == null)
                    db.Users.Add(user = new User {Name = authorName});

                var postInfoNode = post.QuerySelector(".story__toggle-button");
                Post dbPostEntry;


                var postRate = post.QuerySelector(".story__rating-count");
                var isDeleted = postRate.QuerySelectorAll("i.i-sprite--feed__rating-trash").Any();//4801869 deleted
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
                    Rating = rateNotAvailable ? (int?)null : int.Parse(postRate.InnerText),
                    Author = user,
                    Type = postType,
                    IsLong = isLong,
                    IsMine = post.QuerySelectorAll(".story__header-title a.story__authors").Any(),
                    IsStraw = post.QuerySelectorAll(".story__header-additional a.story__straw").Any(),

                    Tags = new List<PostTag>(),
                    PostLinks = new List<PostLink>()
                });
                PostProcessingManager.ProcessingComment(db, postId, commentInfos, authorName);

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
                db.SaveChanges();
                PostProcessingManager.ProcessingTags(db, dbPostEntry, post.QuerySelectorAll(".story__tag").Select(x=>x.InnerText).ToList());
                post.QuerySelectorAll("[data-large-image]")
                    .Select(x => x.GetAttributeValue("data-large-image", null)).ToList().ForEach((link) => {
                        dbPostEntry.PostLinks.Add(
                        new PostLink {
                            Url = link,
                            Type = LinkType.Image
                        });
                    });
                post.QuerySelectorAll(".b-video")
                    .Select(x =>
                        new PostLink {
                            Url = x.GetAttributeValue("data-url", null),
                            DataId = x.GetAttributeValue("data-id", null),
                            Type = LinkType.Video
                        }
                    ).ToList().ForEach((link) => {
                        dbPostEntry.PostLinks.Add(link);
                    });
                post.QuerySelectorAll(".b-story-block.b-story-block_type_text a")
                    .Select(x =>
                        new PostLink {
                            Url = x.GetAttributeValue("href", null),
                            Type = LinkType.ExternalUrl
                        }
                    ).ToList().ForEach((link) => {
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
                        var link = GetRefLinkByUrl<PostLink>(address, db);
                        dbPostEntry.PostLinks.Add(link);
                    });
                db.SaveChanges();
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
        }

        private static readonly Regex PostIdRegex = new Regex(@"\d+$");
        private static readonly Regex CommentRegex = new Regex(@"comment_\d+$");
        private static readonly Regex ProfileRegex = new Regex(@"/profile/\w+$");

        private static T GetRefLinkByUrl<T>(string address, DataContext db) where T : LinkBase, new() {
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
                    var userRef = db.Users.FirstOrDefault(x => x.Name == userName);
                    if (userRef == null)
                        db.Users.Add(userRef = new User {Name = userName});
                    link.User = userRef;
                }
                else
                    link.Type = LinkType.NotRecognized;
            }
            else {
                link.Type = LinkType.ExternalUrl;
            }
            return link;
        }

        public void Execute(IJobExecutionContext context) {
            var postId = 4801861;
            try {
//                GragPost(postId);
            }
            catch (Exception e) {
                log.Error("Post #"+postId+" not processed.", e);
            }
        }
    }
}
