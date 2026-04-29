using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BackEndAPI.Models.Other
{
    public class ItemSpec
    {
        public int Id { get; set; }
        public int BrandId { get; set; }
        [MaxLength(255)]
        public string Brand { get; set; }
        public int IndustryId { get; set; }
        [MaxLength(255)]
        public string Industry { get; set; }
        public int ItemTypeId { get; set; }
        [MaxLength(255)]
        public string ItemType {  get; set; }
        public int PackingId { get; set; }
        [MaxLength(255)]
        public string Packing { get; set; }
    }
    public class ItemSpecHierarchy
    {
        public int BrandId { get; set; }
        [MaxLength(255)]
        [JsonPropertyName("brandName")]
        public string BandName { get; set; }
        public ICollection<Nganhhang>? Industry { get; set; }
    }
    public class Nganhhang
    {
        public int IndustryId { get; set; }
        [MaxLength(255)]
        public string IndustryName { get; set; }
        public ICollection<LoaiSP>? ItemType { get; set; }
    }
    public class LoaiSP
    {
        public int ItemTypeId { get; set; }
        [MaxLength(255)]
        public string ItemTypeName { get; set; }
        public ICollection<QCBaobi>? Packing { get; set; }
    }
    public class QCBaobi
    {
        public int PackingId { get; set; }
        [MaxLength(255)]
        public string PackingName { get; set; }
    }
}
