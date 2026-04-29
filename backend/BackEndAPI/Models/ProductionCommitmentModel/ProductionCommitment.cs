using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using BackEndAPI.Models.ItemGroups;

namespace BackEndAPI.Models.ProductionCommitmentModel;

public class ProductionCommitment
{
    [Key] public int Id { get; set; }

    [Required] [StringLength(100)] public string CommitmentCode { get; set; }
    public string CommitmentName { get; set; }

    [Required] public int CommitmentDate { get; set; }
    public int BPId { get; set; }

    [StringLength(500)] public string Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    [NotMapped]
    public List<CommitmentLine>? CommitmentLines { get; set; }
}

public class CommitmentLine
{
    [Key] public int Id { get; set; }
    public int CommitId { get; set; }
    public string Type { get; set; }
    [JsonIgnore]
    [NotMapped]
    public ProductionCommitment? ProductionCommitment { get; set; }
    [NotMapped]
    public List<CommitmentLineItem>? Items { get; set; }
}

public class CommitmentLineItem
{
    [Key] public int Id { get; set; }
    public int LineId { get; set; }
    public int BrandId { get; set; }
    public int IndustryId { get; set; }
    
    [NotMapped]
    public Industry? Industry { get; set; }
    [NotMapped]
    public Brand? Brand { get; set; }

    [JsonIgnore] 
    [NotMapped]
    public CommitmentLine? Line { get; set; }

    [NotMapped]
    public List<double>? ListValueCommitments { get; set; }
    [NotMapped]
    public List<CommitmentItemAttribute>? CommitmentItemAttribute { get; set; }
    [NotMapped]
    public List<DiscountCommitment>? DiscountCommitments { get; set; }
}

public class DiscountCommitment
{
    [Key] public int Id { get; set; }
    public int LineItemId { get; set; }
    public double ValueCommitment { get; set; }
    public double DiscountPercentage { get; set; }
    public bool IsConvertGoods { get; set; }
    public string Type { get; set; }
    [JsonIgnore]
    [NotMapped]
    public CommitmentLineItem? LineItem { get; set; }
}

public class CommitmentItemAttribute
{
    [Key] public int Id { get; set; }
    public int LineItemId { get; set; }
    public string Name { get; set; }
    public double ValueCommitment { get; set; }
    [JsonIgnore]
    [NotMapped]
    public CommitmentLineItem? LineItem { get; set; }
}