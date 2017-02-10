namespace DataContract
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Post { 
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [StringLength(100)]
        public string Title { get; set; }

        public string Content { get; set; }
        public int? Rating { get; set; }

        public int AuthorId { get; set; }
        public virtual User Author { get; set; }



        public bool IsLong { get; set; }
        public bool IsMine { get; set; }
        [StringLength(20)]
        public string Type { get; set; }
        public int CommentsCount { get; set; }

        public DateTime Created { get; set; }


        public TimeSpan? ProcessedTime { get; set; }
        public DateTime LastCheck { get; set; }



        public virtual ICollection<PostTag> Tags { get; set; }
        public virtual ICollection<UserComment> UserComments { get; set; }
        public virtual ICollection<PostLink> PostLinks { get; set; }
    }
}
