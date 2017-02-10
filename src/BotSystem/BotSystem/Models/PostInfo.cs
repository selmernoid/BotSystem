using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSystem {

    public class PostInfo {
        public int Id { get; set; }
        public string Title { get; set; }
        
        public int Rating { get; set; }
        public string Content { get; set; }

        public string UserName { get; set; }

        public DateTime Created { get; set; }

        public int CommentsCount { get; set; }
    }
}
