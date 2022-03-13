using System.ComponentModel.DataAnnotations;

namespace Tele2Task.Models
{
    public class Citizens
    {
        public string id { get; set; } = "";
        [Required]
        public string name { get; set; } = "";
        public string sex { get; set; } = "";
        public int age { get; set; }
    }

    public class Citizen
    {
        public string name { get; set; } = "";
        public string sex { get; set; } = "";
        public int age { get; set; }
    }
}
