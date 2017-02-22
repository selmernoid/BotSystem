using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataContract;

namespace BotSystem {
    public class PostProcessingManager {

//        public static User GetUser(DataContext db, string username) {
//            var user =
//                db.Users.Local.FirstOrDefault(x => x.Name == username) ??
//                db.Users.FirstOrDefault(x => x.Name == username);
//            if (user == null)
//                db.Users.Add(user = new User {Name = username});
//            return user;
//        }

        public static void ProcessingComment(DataContext db, int postId, IEnumerable<CommentInfo> comments, string author) {
            
//            Dictionary<string, User> users = new Dictionary<string, User>();

//            foreach (var userName in comments.Select(x => x.UserName).Distinct()) {
//                users[userName] = GetUser(db, userName);
//            }

            db.Configuration.AutoDetectChangesEnabled = false;
            try {
                foreach (var comment in comments.OrderBy(x => x.Id)) {
                    UserComment uc;
                    db.UserComments.Add(uc = new UserComment {
                        Id = comment.Id,
                        UserName = comment.UserName,
//                        User = users[comment.UserName],
                        IsPostAuthor = comment.UserName.Equals(author),
                        Rating = comment.Rating,
                        PostId = postId,
                        ParentCommentId = comment.ParentCommentId,
                        Level = comment.Level,
                        Content = comment.Content,
                        Created = comment.DateTime,
                        Links = comment.Links
                    });
                }
            }
            finally {
                db.Configuration.AutoDetectChangesEnabled = true;
            }
        }
        
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
