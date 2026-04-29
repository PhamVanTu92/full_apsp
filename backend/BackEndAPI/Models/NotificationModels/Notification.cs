using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BackEndAPI.Data;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BackEndAPI.Models.NotificationModels;

public class Notification
{
    [Key]
    public int Id { get; set; } // Mã thông báo, dùng để định danh
    public string Title { get; set; } // Tiêu đề của thông báo
    public string Message { get; set; } // Nội dung thông báo
    public string Type { get; set; } // 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Ngày tạo thông báo
    public NotificationObject Object { get; set; }
    [NotMapped]
    public List<int> SendUsers { get; set; }
    [JsonIgnore]
    public ICollection< NotificationUser> Users { get; set; }
}

public class NotificationUser 
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime ReadAt { get; set; }
    public bool? IsHide { get; set; }
    
    public DateTime HideAt { get; set; }
    public bool? IsRead { get; set; } 
    public int NotificationId { get; set; }
    public Notification Notification { get; set; } 
}


public  class NotificationObject
{
    [Key]
    public int Id { get; set; }
    public int ObjId { get; set; } 
    public int NotificationId { get; set; }
    public string ObjType { get; set; }
    public string ObjName { get; set; }
    [JsonIgnore]
    public Notification Notification { get; set; }
}