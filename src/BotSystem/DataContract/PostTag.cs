namespace DataContract
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class PostTag { 
        public int Id { get; set; }

        [StringLength(100)]
        public string Name { get; set; }
        
        public virtual ICollection<Post> Posts { get; set; }
    }
}
