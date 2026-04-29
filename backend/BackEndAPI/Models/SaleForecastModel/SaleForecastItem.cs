using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using BackEndAPI.Models.ItemMasterData;
using BackEndAPI.Models.Unit;

namespace BackEndAPI.Models.SaleForecastModel;

public class SaleForecastItem
{
    [Key] public int Id { get; set; }
    public int SaleForecastId { get; init; }
    public int ItemId { get; init; }
    public int UomId { get; init; }
    [NotMapped] public string? UomCode => Uom?.UomCode;
    [NotMapped] public string? UomName => Uom?.UomName;
    [NotMapped] public string? ItemName => Item?.ItemName;
    [NotMapped] public string? ItemCode => Item?.ItemCode;
    [NotMapped] public string? UgpName => Item?.OUGP?.UgpName;
    [NotMapped] public string? UgpCode => Item?.OUGP?.UgpCode;
    [NotMapped] [JsonIgnore] public OUOM? Uom { get; init; }
    [JsonIgnore] public Item? Item { get; init; }
    [JsonIgnore] public SaleForecast? SaleForecast { get; init; }
    public List<SaleForecastItemPeriod> Periods { get; init; } = [];
    [NotMapped] public string? Status { get; set; }
}

public class SaleForecastItemPeriod
{
    public int Id { get; init; }
    [MaxLength(255)] public string? PeriodName { get; init; }
    public int SaleForecastItemId { get; init; }

    public int PeriodIndex { get; init; }
    public int Quantity { get; init; }
    public int ActualQuantity { get; set; }

    [NotMapped] [JsonIgnore] public SaleForecastItem? SaleForecastItem { get; init; }
}

