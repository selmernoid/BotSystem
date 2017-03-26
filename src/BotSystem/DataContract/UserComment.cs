namespace DataContract
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public class UserComment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string UserName { get; set; }
        public int? UserId { get; set; }
        public virtual User User { get; set; }

        public bool IsPostAuthor { get; set; }

        public int? ParentCommentId { get; set; }
        public virtual UserComment ParentComment { get; set; }

        public int PostId { get; set; }
        public virtual Post Post { get; set; }

        public int? Rating { get; set; }
        public string Content { get; set; }

        public ModificationType ModificationType { get; set; }
        public string ModificationComment { get; set; }
        
        public DateTime Created { get; set; }
        public virtual ICollection<CommentLink> Links { get; set; }
        public virtual ICollection<UserComment> ChildComments { get; set; }
        public int Level { get; set; }
    }

    public enum ModificationType {
        None = 0,

        Deleted = 1
    }
}
