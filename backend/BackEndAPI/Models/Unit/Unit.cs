using BackEndAPI.Models.ItemMasterData;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BackEndAPI.Models.Unit
{
    /// <summary>Đơn vị tính</summary>
    public class OUOM
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string UomCode { get; set; }
        [MaxLength(254)]
        public string UomName { get; set; }
        public int? SapId { get; set; }
        public bool Status { get; set; }
    }
    public class OUGP
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string UgpCode { get; set; }
        [MaxLength(254)]
        public string UgpName { get; set; }
        public int? BaseUom { get; set; }
        public int? SapId { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<UGP1>? UGP1 { get; set; }
        public OUOM? OUOM { get; set; }
        public bool Status { get; set; }

    }
    public class UGP1
    {
        public int Id { get; set; }
        public int UgpId { get; set; }
        public int UomId { get; set; }
        [JsonIgnore]
        public OUOM? OUOM { get; set; }
        public double BaseQty { get; set; }
        public double AltQty { get; set; }
        [NotMapped]
        public string? Status { get; set; }
        [JsonIgnore]
        public OUGP? OUGP { get; set; }
    }
}
