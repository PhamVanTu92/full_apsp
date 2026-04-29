using System.ComponentModel.DataAnnotations;

namespace BackEndAPI.Models.LocationModels;

public class Region

{
    [Key]
    public int Id { get; set; }
    public required string Name { get;set; }
    public List<Area>? Areas { get; set; }
}