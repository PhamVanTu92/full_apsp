using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace BackEndAPI.Models.LocationModels;

public class Area
{
    [Key]
    public int Id { get; set; }
    public required string Name { get; set; }
    [JsonIgnore] public List<Region>? Regions { get; set; }
}