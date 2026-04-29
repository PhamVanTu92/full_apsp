using System.ComponentModel.DataAnnotations;

namespace BackEndAPI.Models.Sync;

/// <summary>
/// Lưu mốc thời gian sync gần nhất cho từng job để hỗ trợ delta sync.
/// PK là JobName để tránh duplicate row mỗi job.
/// </summary>
public class SyncCheckpoint
{
    [Key]
    [MaxLength(100)]
    public string JobName { get; set; } = string.Empty;

    /// <summary>Mốc UpdateDate cuối cùng đã sync (UTC).</summary>
    public DateTime LastSyncedAt { get; set; }

    /// <summary>Thời gian chạy lần cuối (ms) — để monitor.</summary>
    public int LastDurationMs { get; set; }

    /// <summary>Số bản ghi delta lần cuối.</summary>
    public int LastRecordsProcessed { get; set; }

    /// <summary>"Success" / "Failed" / "Cancelled".</summary>
    [MaxLength(20)]
    public string LastStatus { get; set; } = "Success";

    /// <summary>Stack trace ngắn khi fail — null khi success.</summary>
    [MaxLength(1000)]
    public string? LastError { get; set; }

    public DateTime UpdatedAt { get; set; }
}
