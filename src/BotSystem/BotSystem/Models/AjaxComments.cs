using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSystem.Models {
    public class AjaxComments {
        public bool result { get; set; }
        public string message { get; set; }
        public int message_code { get; set; }
        public AjaxCommentsData data { get; set; }
    }

    public class AjaxCommentsData {
        public int total { get; set; }
        public int? last_id { get; set; }
        public IEnumerable<AjaxCommentData> comments { get; set; }
    }

    public class AjaxCommentData {
        public int id { get; set; }
        public int parent_id { get; set; }
        public string html { get; set; }
        public bool is_hidden { get; set; }
        public bool is_hidden_children { get; set; }
    }
}
