using System.ComponentModel.DataAnnotations;

namespace BackEndAPI.Models.Other
{
    public class StoneType
    {
        public int Id { get; set; }
        [MaxLength(25)]
        public string Code { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
    }
}
