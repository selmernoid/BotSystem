using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataContract;

namespace BotSystem {

    public class CommentInfo {
        public int Id { get; set; }
        public string UserName { get; set; }
        public DateTime DateTime { get; set; }
        public string Content { get; set; }
        public int? Rating { get; set; }
        public int? ParentCommentId { get; set; }
        public int Level { get; set; }
        public List<CommentLink> Links { get; set; }
    }
}
