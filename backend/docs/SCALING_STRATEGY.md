# RFC — Scaling Strategy cho APSP NEW_BACKEND

| Field | Value |
|---|---|
| **Status** | Draft — chưa quyết |
| **Author** | (đề xuất bởi review session 2026-04-28) |
| **Reviewers** | (cần điền) |
| **Decision deadline** | (cần điền) |
| **Liên quan** | [docs/REFACTOR_PLAN.md](REFACTOR_PLAN.md), [SECURITY.md](../SECURITY.md) |

## 1. Tóm tắt

App APSP hiện chỉ deploy được **1 instance** vì Quartz scheduler dùng `RAMJobStore` (job state lưu trong RAM của process). Khi scale ra nhiều instance, mọi instance đều fire cùng 1 job → duplicate processing trên SAP, race condition khi update DB, tải SAP gấp N lần. RFC này phân tích 3 phương án và đề xuất **Quartz cluster mode (AdoJobStore)** làm giải pháp chính.

## 2. Vấn đề chi tiết

### 2.1 Hiện trạng

```csharp
// Program.cs
builder.Services.AddQuartz(q => {
    q.AddJob<SyncBPCRD4Job>(...);
    q.AddTrigger(... .WithCronSchedule("0/30 * * * * ?"));
    // ... 6 jobs khác
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
```

- `RAMJobStore` (mặc định) — schedule + trigger state trong RAM
- `[DisallowConcurrentExecution]` chỉ chặn **trong 1 process**, không chặn giữa các process

### 2.2 Behavior khi scale ra N instance

```
T=0 (cron 0/30 * * * * ? trigger toàn bộ instance cùng lúc)

Instance A: SyncBPCRD4Job.Execute → BPSyncService.SyncCRD4DeltaAsync
Instance B: SyncBPCRD4Job.Execute → BPSyncService.SyncCRD4DeltaAsync
Instance C: SyncBPCRD4Job.Execute → BPSyncService.SyncCRD4DeltaAsync
                  ↓
         Tất cả query SAP "Invoices?$filter=UpdateDate ge {checkpoint}"
         → 3× HTTP call SAP
         → 3× cùng records về local DB
         → 3× UPDATE SyncCheckpoints (last write wins)
         → có thể conflict EF tracking nếu 3 instance cùng SaveChangesAsync
```

### 2.3 Hậu quả cụ thể trên các job hiện có

| Job | Tần suất | Hậu quả với N=3 instance |
|---|---|---|
| `SyncBPCRD4Job` | 30s | 3× fetch invoices SAP, race trên CRD4 upsert |
| `SyncDebJob` | 2h | 3× fetch BP balance, 3× UPDATE checkpoint |
| `SyncBPJob`, `SyncDOCJob` | 10m / 2h | 3× call proxy API → tải proxy gấp 3 |
| `SyncJob` (push docs) | 2h | **Nguy hiểm nhất**: 3 instance cùng pick up doc IsSync=false → mỗi doc bị push 3 lần lên SAP, tạo 3 SAP DocEntry khác nhau cho cùng 1 đơn |
| `SyncVPKMJob` | 58s | Tương tự SyncJob |

### 2.4 Tại sao đến giờ chưa thấy bug?

App hiện chỉ deploy 1 instance. Trước khi scale (PRD đặt ra growth target?), refactor này **bắt buộc**.

## 3. Yêu cầu

### Functional
- Mỗi job chỉ chạy trên 1 instance tại 1 thời điểm
- Khi instance crash giữa job, instance khác pick up trong < 1 phút
- Job state (last fired, misfire) bền vững qua restart toàn cluster

### Non-functional
- Không tăng latency p99 của API endpoints (job và API tách biệt resource)
- Không thêm dependency mới (Redis/Kafka/...) nếu không cần thiết
- Ops effort cài đặt < 1 ngày
- Rollback được trong < 30 phút nếu phát sinh issue

### Constraints hiện tại
- SQL Server đã có sẵn (160.30.252.14,14388)
- Deploy hiện tại: chưa rõ K8s / Docker Swarm / VM (cần xác nhận)
- Team không có Redis/Kafka cluster vận hành sẵn

## 4. Phương án

### 4.1 Phương án A — Quartz AdoJobStore + Cluster mode ⭐ Khuyến nghị

#### Cách hoạt động
- 11 bảng `QRTZ_*` lưu trigger state, job data, lock, instance check-in
- Mỗi instance acquire row lock `QRTZ_LOCKS` trước khi fire trigger
- Heartbeat `QRTZ_SCHEDULER_STATE` mỗi N giây — instance nào timeout sẽ bị "fail-over": instance khác pick up jobs đang chạy

#### Ưu
- Tích hợp built-in của Quartz, code thay đổi ~30 dòng
- Tự fail-over: instance crash giữa job → instance khác recover
- Không cần dịch vụ ngoài (Redis, Consul, ...)
- Cộng đồng lớn — đã được Stack Overflow, Microsoft, etc. dùng từ 2010
- Job declaration C# giữ nguyên (chỉ đổi store backend)

#### Nhược
- Tăng query DB: ~1 query/instance/check-in interval (mặc định 7.5s)
  - Với 5 instance × 1 query / 7.5s = ~40 query/phút — không đáng kể với SQL Server
- 11 bảng QRTZ_* phải migrate (script chuẩn từ Quartz repo)
- Cluster cần đồng bộ thời gian (NTP) trên các instance
- Có overhead khi nhận cluster ban đầu (~5-10s)

#### Triển khai chi tiết (~2-4 giờ work)

**Bước 1**: Cài package serialization
```bash
cd BackEndAPI
dotnet add package Quartz.Serialization.Json --version 3.14.*
```

**Bước 2**: Apply schema cho SQL Server từ [Quartz official script](https://github.com/quartznet/quartznet/blob/main/database/tables/tables_sqlServer.sql):
```bash
sqlcmd -S "<server>" -d APSP -U <user> -P <pass> -i tables_sqlServer.sql
```

11 bảng tạo: `QRTZ_BLOB_TRIGGERS`, `QRTZ_CALENDARS`, `QRTZ_CRON_TRIGGERS`, `QRTZ_FIRED_TRIGGERS`, `QRTZ_JOB_DETAILS`, `QRTZ_LOCKS`, `QRTZ_PAUSED_TRIGGER_GRPS`, `QRTZ_SCHEDULER_STATE`, `QRTZ_SIMPLE_TRIGGERS`, `QRTZ_SIMPROP_TRIGGERS`, `QRTZ_TRIGGERS`.

**Bước 3**: Update `Program.cs` AddQuartz section:
```csharp
builder.Services.AddQuartz(q =>
{
    // Cluster setup
    q.SchedulerName = "APSP-Scheduler";
    q.SchedulerId = Environment.GetEnvironmentVariable("HOSTNAME")
        ?? Environment.MachineName;

    q.UsePersistentStore(s =>
    {
        s.UseProperties = true;
        s.RetryInterval = TimeSpan.FromSeconds(15);

        s.UseSqlServer(sqlServer =>
        {
            sqlServer.ConnectionString = connectionString; // hoặc connection string riêng cho Quartz
            sqlServer.TablePrefix = "QRTZ_";
        });

        s.UseClustering(c =>
        {
            c.CheckinInterval = TimeSpan.FromSeconds(10);
            c.CheckinMisfireThreshold = TimeSpan.FromSeconds(20);
        });

        s.UseSystemTextJsonSerializer();
    });

    // Job + trigger declaration giữ nguyên 100%
    var jobBPCRD4Key = new JobKey("SyncBPCRD4Job");
    q.AddJob<SyncBPCRD4Job>(opts => opts.WithIdentity(jobBPCRD4Key).StoreDurably());
    q.AddTrigger(opts => opts
        .ForJob(jobBPCRD4Key)
        .WithIdentity("SyncBPCRD4Job-trigger")
        .WithCronSchedule("0/30 * * * * ?"));
    // ... 6 jobs khác
});
```

Lưu ý: với cluster mode, mọi job phải đánh dấu `.StoreDurably()` để store xuống DB.

**Bước 4**: Connection string riêng cho Quartz (best practice, để tách load)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "<app-conn>",
    "QuartzConnection": "<quartz-conn — có thể trỏ DB riêng>"
  }
}
```

**Bước 5**: Test cluster mode trên dev — chạy 2 instance cùng lúc:
```bash
# Terminal 1
ASPNETCORE_URLS=https://localhost:7238 dotnet run --project BackEndAPI

# Terminal 2
ASPNETCORE_URLS=https://localhost:7239 HOSTNAME=instance-2 dotnet run --project BackEndAPI
```

Verify trong log:
- Mỗi job log "fired" chỉ ở **1 instance** mỗi cron tick
- Bảng `QRTZ_SCHEDULER_STATE` có 2 row, mỗi row 1 instance, `LAST_CHECKIN_TIME` cập nhật mỗi 10s

**Bước 6**: Rollback plan
- Nếu phát sinh issue, revert PR → Quartz quay lại RAMJobStore
- Schema QRTZ_* để lại không sao, app cũ ignore
- Rollback < 5 phút (chỉ là code change)

### 4.2 Phương án B — Distributed lock thủ công (Redis hoặc DB-based)

#### Cách hoạt động
- Quartz vẫn dùng RAMJobStore (mọi instance đều fire trigger)
- Mỗi job acquire 1 named lock trước khi chạy
- Job nào không acquire được lock → return ngay, đợi cron tiếp

```csharp
public class SyncBPCRD4Job : SyncJobBase
{
    private readonly IDistributedLock _lock;

    protected override async Task RunAsync(CancellationToken ct)
    {
        await using var handle = await _lock.AcquireAsync(
            "sync-bpcrd4", TimeSpan.FromMinutes(2), ct);
        if (handle == null) return;

        await _syncService.SyncCRD4DeltaAsync(ct);
    }
}
```

#### Ưu
- Không cần migrate Quartz schema
- Có thể dùng SQL Server làm lock provider qua [DistributedLock.SqlServer](https://github.com/madelson/DistributedLock)
- Code nhỏ — wrap mỗi job

#### Nhược
- **Mọi instance vẫn fire trigger và try-lock** → wakeup tốn CPU vô ích
- Lock release phụ thuộc vào dispose đúng — nếu instance kill -9 giữa job, lock có thể kẹt đến TTL
- Không có fail-over: nếu instance ôm lock crash, job phải đợi TTL hết mới retry
- Phải code 7 wrap method (1 cho mỗi job)

→ Phù hợp khi không thể migrate Quartz schema. Không khuyến nghị cho APSP.

### 4.3 Phương án C — Worker Service riêng

#### Cách hoạt động
Tách deployment thành 2 service:
- **API**: N replicas, không cấu hình Quartz
- **Worker**: 1 replica, chỉ chạy Quartz job (không expose HTTP)

```csharp
// Program.cs với env var WORKER_ROLE=true
if (builder.Configuration.GetValue<bool>("WORKER_ROLE"))
{
    builder.Services.AddQuartz(...);
    builder.Services.AddQuartzHostedService(...);
}
```

K8s deployment YAML tách 2 deployment trỏ vào cùng image, env var khác nhau.

#### Ưu
- Code thay đổi tối thiểu (~10 dòng)
- API scale tự do, không lo job race
- Resource tách biệt: peak API request không ảnh hưởng job, ngược lại

#### Nhược
- **Worker = single point of failure** — worker chết, mọi sync dừng đến khi K8s restart
- Vẫn không scale được khi backlog tăng (chỉ 1 worker xử lý)
- Cần manage 2 deployment thay vì 1
- Database connection pool phân tán giữa API + Worker, tổng connection có thể tăng

→ Phù hợp khi muốn deploy nhanh, chấp nhận downtime sync ngắn khi worker restart.

### 4.4 So sánh nhanh

| Tiêu chí | A (AdoJobStore) | B (Distributed lock) | C (Worker riêng) |
|---|---|---|---|
| **Code change** | ~30 dòng | ~50 dòng × 7 jobs | ~10 dòng |
| **Schema migration** | 11 bảng QRTZ_* | Không (hoặc 1 bảng lock) | Không |
| **Scale jobs** | ✅ Active-active failover | ⚠ Active-passive (1 winner) | ❌ 1 instance only |
| **Single point of failure** | ❌ Không | ❌ Không | ⚠ Worker chết = sync stop |
| **CPU lãng phí** | Thấp | Cao (mọi instance wakeup) | Thấp |
| **Ops effort** | Trung bình | Thấp | Trung bình |
| **Rollback dễ** | ✅ Revert PR | ✅ Revert PR | ⚠ Cần redeploy 2 service |

## 5. Khuyến nghị

**Chọn Phương án A — Quartz AdoJobStore + Cluster mode** vì:

1. **Đáp ứng đủ requirement**: scale, fail-over, không SPOF
2. **Tận dụng infra có sẵn**: SQL Server đã có, không cần Redis/Kafka
3. **Built-in pattern**: tested rộng, document đầy đủ, low risk
4. **Rollback nhanh**: revert code commit là xong
5. **Không phá kiến trúc hiện tại**: job declaration giữ nguyên, chỉ đổi store

## 6. Câu hỏi cần xác nhận trước khi triển khai

| # | Câu hỏi | Tại sao quan trọng |
|---|---|---|
| 1 | Hiện đang deploy bao nhiêu instance? Có plan scale lên N=3+ không? | Nếu chỉ luôn 1 instance, có thể delay refactor này — backlog low priority |
| 2 | Deploy trên nền nào: K8s, Docker Swarm, VM? | Quyết định cách inject `SchedulerId` (HOSTNAME env var với K8s, hostname với VM) |
| 3 | SQL Server có chịu thêm load 40-100 query/phút từ cluster check-in? | Nếu DB đã căng, cân nhắc tách `QuartzConnection` sang DB nhỏ riêng |
| 4 | NTP có sync trên các instance? | Cluster Quartz cần clock skew < 1 phút giữa các node |
| 5 | Có acceptable nếu sync trễ 1-2 phút khi failover? | Cluster phát hiện instance chết qua heartbeat — mặc định 7.5s × 2 = 15s, có thể tunable |
| 6 | DB SQL ở 160.30.252.14 có thuộc cluster Always On không? | Nếu có, đảm bảo Quartz connection trỏ tới primary node hoặc dùng listener |

## 7. Implementation plan (sau khi approve)

### Phase 1 — Setup (1 ngày)
- [ ] Apply schema 11 bảng QRTZ_* lên DB staging
- [ ] Update Program.cs với UsePersistentStore + UseClustering
- [ ] Test trên dev với 2 instance localhost
- [ ] Verify log: trigger fire chỉ trên 1 instance / lần

### Phase 2 — Staging validation (3 ngày)
- [ ] Deploy 2 replica trên staging
- [ ] Monitor 24h: log + metric:
  - Mỗi cron tick có exactly 1 instance fire không?
  - Bảng `QRTZ_SCHEDULER_STATE` heartbeat đều chứ?
  - Latency API có degraded không?
- [ ] Test failover: kill 1 pod giữa job → pod kia có pick up không?

### Phase 3 — Production rollout (1 ngày)
- [ ] Deploy 1 instance + Quartz cluster mode (cluster với 1 node = vẫn hoạt động)
- [ ] Monitor 1 ngày
- [ ] Scale lên 2 instance
- [ ] Monitor 1 tuần
- [ ] Scale lên N instance theo nhu cầu

### Phase 4 — Cleanup (sau 1 tháng ổn định)
- [ ] Document runbook trong `docs/RUNBOOK.md`: cách thêm/xoá node, cách clear stuck job từ QRTZ_*
- [ ] Setup alert: Prometheus metric `quartz_jobs_failed_total`, `quartz_cluster_size`

## 8. Open questions cho team

1. **Schedule riêng cho job đặc biệt?** — VD `SyncBPCRD4Job` có cần ưu tiên fire trên instance gần SAP nhất (network locality) không? Quartz support node tag.
2. **Migration data từ RAMJobStore**: hiện không có data persistent (RAM), vậy first deploy cluster mode bằng cách nào? → Quartz tự seed schedule từ code khi start, không cần migrate
3. **Disaster recovery**: nếu DB SQL down, scheduler có graceful degrade không? Quartz sẽ retry kết nối nhưng job không fire → cần health check riêng cho Quartz cluster
4. **Quartz version**: hiện đang `Quartz 3.14.0`. AdoJobStore SQL Server provider là `Quartz` core (có sẵn). `Quartz.Serialization.Json` chỉ cần thêm cho serialize JobDataMap

## 9. References

- [Quartz.NET Clustering documentation](https://www.quartz-scheduler.net/documentation/quartz-3.x/tutorial/advanced-enterprise-features.html)
- [SQL Server schema script](https://github.com/quartznet/quartznet/blob/main/database/tables/tables_sqlServer.sql)
- [DistributedLock library (cho Phương án B)](https://github.com/madelson/DistributedLock)
- Quartz performance benchmark trên SQL Server: ~100-500 jobs/s acquired qua row lock — đủ cho APSP (7 job × 1 trigger/30s = 14 acquires/min)

---

## Phụ lục — POC code mẫu Phương án A

```csharp
// Program.cs (sau refactor)
builder.Services.AddQuartz(q =>
{
    q.SchedulerName = "APSP-Scheduler";
    q.SchedulerId = builder.Configuration["Quartz:SchedulerId"]
        ?? Environment.GetEnvironmentVariable("HOSTNAME")
        ?? Environment.MachineName;

    q.UsePersistentStore(s =>
    {
        s.UseProperties = true;
        s.RetryInterval = TimeSpan.FromSeconds(15);

        s.UseSqlServer(sqlServer =>
        {
            sqlServer.ConnectionString = builder.Configuration.GetConnectionString("QuartzConnection")
                ?? builder.Configuration.GetConnectionString("DefaultConnection")!;
            sqlServer.TablePrefix = "QRTZ_";
        });

        s.UseClustering(c =>
        {
            c.CheckinInterval = TimeSpan.FromSeconds(10);
            c.CheckinMisfireThreshold = TimeSpan.FromSeconds(20);
        });

        s.UseSystemTextJsonSerializer();
    });

    // Job declaration: thêm StoreDurably() — bắt buộc với persistent store
    var key1 = new JobKey("SyncBPCRD4Job");
    q.AddJob<SyncBPCRD4Job>(o => o.WithIdentity(key1).StoreDurably());
    q.AddTrigger(o => o
        .ForJob(key1)
        .WithIdentity("SyncBPCRD4Job-trigger")
        .WithCronSchedule("0/30 * * * * ?"));

    // ... 6 jobs khác làm tương tự
});

builder.Services.AddQuartzHostedService(q =>
{
    q.WaitForJobsToComplete = true;
    q.AwaitApplicationStarted = true;
});
```
