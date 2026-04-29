using BackEndAPI.Models.Approval_V2;

namespace BackEndAPI.Dtos;

public class IdAndTypeDocDto
{
    public int Id { get; set; }
    public DocumentEnum Type { get; set; }
    public string DocType { get; set; }
}