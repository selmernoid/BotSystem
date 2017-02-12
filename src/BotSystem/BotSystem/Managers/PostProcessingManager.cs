using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataContract;

namespace BotSystem {
    public class PostProcessingManager {

        public static void ProcessingComment(DataContext db, int postId, IEnumerable<CommentInfo> comments, string author) {
            
            Dictionary<string, User> users = new Dictionary<string, User>();

            foreach (var userName in comments.Select(x => x.UserName).Distinct()) {
                var user = db.Users.FirstOrDefault(x => x.Name.ToLower() == userName.ToLower());
                if (user == null)
                    db.Users.Add(user = new User {Name = userName});
                users[userName] = user;
            }


            foreach (var comment in comments.OrderBy(x=>x.Id)) {
                db.UserComments.Add(new UserComment {
                    Id = comment.Id,
                    User = users[comment.UserName],
                    IsPostAuthor = comment.UserName.Equals(author),
                    Rating = comment.Rating,
                    PostId = postId,
                    ParentCommentId = comment.ParentCommentId,
                    Level = comment.Level,
                    Content = comment.Content,
                    Created = comment.DateTime
                });
            }


        }

//        public static void ProcessingPost(DataContext db, PostInfo post) {
//
//                var user = db.Users.FirstOrDefault(x => x.Name.ToLower() == post.UserName.ToLower());
//                if (user == null)
//                    db.Users.Add(user = new User { Name = post.UserName });
//
//            db.Posts.Add(new Post {
//                Id = post.Id,
//                Title = post.Title,
//                Rating = post.Rating,
//                Author = user,
//                Content = post.Content,
//                CommentsCount = post.CommentsCount
//            });
//        }

        public static void ProcessingTags(DataContext db, Post post, List<string> tags) {
            var toAdd = tags.Where(x => post.Tags.All(t => t.Name != x)).ToList();
            var toRemove = post.Tags.Where(x => !tags.Contains(x.Name)).ToList();

            foreach (var postTag in toRemove) {
                post.Tags.Remove(postTag);
            }

            foreach (var newTag in toAdd) {
                var tag = db.Tags.FirstOrDefault(x => x.Name == newTag);
                if (tag == null)
                    db.Tags.Add(new PostTag {Name = newTag});
                post.Tags.Add(tag);
            }
        }
    }
}
