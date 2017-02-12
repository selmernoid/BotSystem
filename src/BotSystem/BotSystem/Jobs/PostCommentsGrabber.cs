using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
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
            var authorName = post.QuerySelector(".story__author").InnerText;

            var comments = html.QuerySelectorAll(".b-comments_type_main .b-comment");
            var commentInfo = new List<CommentInfo>();

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
                commentInfo.Add(new CommentInfo {
                    Id = commentId,
                    UserName = userName,
                    Content = content,
                    Rating = notYetCommentRate ? (int?)null : int.Parse(comment.QuerySelector(".b-comment__rating-count").InnerText),
                    ParentCommentId = parentId == 0 ? (int?) null : parentId,
                    Level = level,
                    DateTime = commentDateTime,
                });
            }
            using (var db = new DataContext()) {
                var user = db.Users.FirstOrDefault(x => x.Name.ToLower() == authorName) ;
                if (user == null)
                    db.Users.Add(user = new User {Name = authorName});

                var postInfoNode = post.QuerySelector(".story__toggle-button");
                Post dbPostEntry;
                db.Posts.Add(dbPostEntry = new Post {
//                    Id = postId,
                    Title = post.QuerySelector(".story__header-title a.story__title-link").InnerText,
                    Content = post.QuerySelector(".b-story__content").InnerHtml,
                    Created =
                        Helper.UnixTimeStampToDateTime(
                            double.Parse(
                                post.QuerySelector(".story__date").Attributes.AttributesWithName("title").FirstOrDefault().Value)),
                    LastCheck = processingDateTime,

                    CommentsCount = Int32.Parse(post.QuerySelector(".story__comments-count").InnerText.Split(' ')[0]),
                    Rating = int.Parse(post.QuerySelector(".story__rating-count").InnerText),
                    Author = user,
                    Type = postInfoNode.Attributes.AttributesWithName("data-story-type").FirstOrDefault().Value,
                    IsLong = postInfoNode.Attributes.AttributesWithName("data-story-long").FirstOrDefault().Value.Equals("true", StringComparison.CurrentCultureIgnoreCase),
                    IsMine = post.QuerySelectorAll(".story__header-title a.story__authors").Any(),
                    
                    Tags = new List<PostTag>()
                });
                PostProcessingManager.ProcessingComment(db, postId, commentInfo, authorName);
                db.SaveChanges();
                PostProcessingManager.ProcessingTags(db, dbPostEntry, post.QuerySelectorAll(".story__tag").Select(x=>x.InnerText).ToList());
                db.SaveChanges();
                timer.Stop();
                dbPostEntry.ProcessedTime = timer.Elapsed;
                db.SaveChanges();
            }
            for (int i = 0; i < 10; i++)
                Console.WriteLine(JsonConvert.SerializeObject(commentInfo[i]));
            //            IList<HtmlNode> nodes = html.QuerySelectorAll("div .my-class[data-attr=123] > ul li");
            //            HtmlNode node = nodes[0].QuerySelector("p.with-this-class span[data-myattr]");
            //            html.DocumentElement.SelectNodes("//a[@href")

//            throw new NotImplementedException();
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
