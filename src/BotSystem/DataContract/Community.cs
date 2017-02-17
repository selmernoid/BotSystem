using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataContract {
    public class Community {
        public int Id { get; set; }
        [StringLength(32),Index]
        public string Name { get; set; }
        public string Link { get; set; }

        public virtual ICollection<Post> Posts { get; set; }
    }
}