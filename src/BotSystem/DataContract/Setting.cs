using System.ComponentModel.DataAnnotations;

namespace DataContract {
    public class Setting {
        [Key, StringLength(50)]
        public string Name { get; set; }
        [StringLength(100)]
        public string ValueString { get; set; }
        public int? ValueInt { get; set; }
    }
}