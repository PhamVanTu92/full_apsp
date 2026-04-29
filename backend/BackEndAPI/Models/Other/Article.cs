using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace BackEndAPI.Models.Other
{
    public class Article
    {
        public int Id { get; set; }
        [MaxLength(255)]
        public string Name { get; set; }

        public DateTime CreateDate { get; set; }

        [MaxLength(100)]
        public string Creator { get; set; }

        [MaxLength(50)]
        public string Status { get; set; }
        public string? Note { get; set; }
        public string? FilePath { get; set; }
    }
    public class ArticleView
    {
        public string Name { get; set; }
        public string? Note { get; set; }
        [JsonProperty("File")]
        public IFormFile File { get; set; }
    }
}
