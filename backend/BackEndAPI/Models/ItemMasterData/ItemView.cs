using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace BackEndAPI.Models.ItemMasterData
{
    public class ItemView
    {
        [JsonProperty("item")]
        [Required(ErrorMessage = "Thông tin trống")]
        public string? item { get; set; }
        [JsonProperty("images")]
        public List<IFormFile>? images { get; set; }
    }
    public class ItemOnhand
    {
        public string ItemCode {  get; set; }
        public double Onhand { get; set; }
        public double OnOrder { get; set; }
    }

    public class ItemImportView
    {
        public string ItemCode { get; set; }
        public double Price { get; set; }
        public string Currency { get; set; }
    }

    public class ItemImport
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }    
        public int PackageId { get; set; }
        public string PackingName { get; set; }
        public double Price { get; set; }
        public string Currency { get; set; }
        public string? Images { get; set; }
    }
}
