using BackEndAPI.Models.Document;
using BackEndAPI.Models.ItemMasterData;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace BackEndAPI.Models.Other
{
    public class GoldPriceList
    {
        public int Id { get; set; }
        [MaxLength(25)]
        public string PriceListCode { get; set; }
        [MaxLength(100)]
        public string PriceListName { get; set; }
        public DateOnly Date {  get; set; }
        public TimeOnly Time { get; set; }
        [MaxLength(50)]
        public string Location { get; set; }
        [MaxLength(50)]
        public string? Status { get; set; } = "O";

        public ICollection<GoldPriceListLine>? GoldPriceListLine { get; set; }
    }
    public class GoldPriceListLine
    {
        public int Id { get; set; }
        public int FatherId { get; set; }
        public int GoldBrandId { get; set; }
        public int GoldTypeId { get; set; }
        public int GoldContentId { get; set; }
        public double SellPrice { get; set; }
        public double BuyPrice { get; set; }
        [JsonIgnore]
        public GoldPriceList? GoldPriceList { get; set; }
        [JsonIgnore]
        public GoldType? GoldType { get; set; }
        [JsonIgnore]
        public GoldBrand? GoldBrand { get; set; }
        [JsonIgnore]
        public GoldContent? GoldContent { get; set; }
    }
    public class GoldContent
    {
        public int Id { get; set; }

        [MaxLength(25)]
        public string Code { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        public double Rate { get; set; }
        [MaxLength(25)]
        public string Symbol { get; set; }

    }
    public class GoldType
    {
        public int Id { get; set; }
        [MaxLength(25)]
        public string Code { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
    }
    public class GoldBrand
    {
        public int Id { get; set; }
        [MaxLength(25)]
        public string Code { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
    }
}
