using BackEndAPI.Models.ItemMasterData;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BackEndAPI.Models.TaxGroup
{
    public class TaxGroups
    {
        public int Id { get; set; }
        [MaxLength(25)]
        public string Code { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        public int Rate { get; set; }   
        public DateOnly EffDate { get; set; }
        [MaxLength(5)]
        public string Type { get; set; }
        public bool Locked { get; set; }
    }
}
