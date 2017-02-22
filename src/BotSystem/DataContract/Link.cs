namespace DataContract
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public abstract class LinkBase {
        public int Id { get; set; }

        public LinkType Type { get; set; }

        public string UserName { get; set; }
        public int? UserId { get; set; }
        public virtual User User { get; set; }

        [Index]
        public int? OtherPostId { get; set; }
        [Index]
        public int? OtherCommentId { get; set; }
        [Index, StringLength(890)]
        public string Url { get; set; }

        [StringLength(50)]
        public string DataId { get; set; }
    }

    public class CommentLink : LinkBase {
        public int CommentId { get; set; }
        public virtual UserComment Comment { get; set; }
    }
    public class PostLink : LinkBase {
        public int PostId { get; set; }
        public virtual Post Post { get; set; }
    }

    public enum LinkType {
        None = 0,

        User          = 1,
        Post          = 2,
        Comment       = 3,
        Image         = 4,
        Gif           = 5,
        Video         = 6,
        ExternalUrl   = 7,
        ExternalAwayUrl = 8,

        NotRecognized = 100,


    }
}
