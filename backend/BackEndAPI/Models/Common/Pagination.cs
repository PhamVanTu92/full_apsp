using System.Text.Json.Serialization;

namespace ePortal.Models.Common;

public class Pagination <T> where T : class
{
    public List<T> Result { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
}