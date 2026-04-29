using BackEndAPI.Models.Document;
using BackEndAPI.Service.Document;
using BackEndAPI.Service.Sync;
using BackEndAPI.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BackEndAPI.Tests.Unit.Services;

/// <summary>
/// Test cho DocumentPushSyncService — kiểm tra queue drain pattern + stuck-doc detection.
/// Verify rằng bug "1 doc/cycle" trước đây đã được fix bằng drain loop.
/// </summary>
public class DocumentPushSyncServiceTests
{
    private static ODOC CreatePendingDoc(int id) => new()
    {
        Id = id,
        Status = "DXN",
        ObjType = 22,
        IsSync = false,
        IsCheck = false,
        InvoiceCode = $"INV{id:D6}"
    };

    [Fact]
    public async Task PushPendingBatchAsync_WhenNoPending_ReportsZero()
    {
        using var db = TestDbContextFactory.Create();
        var docService = new Mock<IDocumentService>();
        var sut = new DocumentPushSyncService(docService.Object, db, NullLogger<DocumentPushSyncService>.Instance);

        var result = await sut.PushPendingBatchAsync(CancellationToken.None);

        result.DocsPushed.Should().Be(0);
        result.StuckDocs.Should().Be(0);
        // Không gọi push khi queue rỗng
        docService.Verify(s => s.SyncToSapAsync(), Times.Never);
    }

    [Fact]
    public async Task PushPendingBatchAsync_DrainsAllPendingDocs()
    {
        using var db = TestDbContextFactory.Create();
        // 3 docs pending Status=DXN, ObjType=22, IsSync=false (queue "docs")
        db.ODOC.AddRange(CreatePendingDoc(1), CreatePendingDoc(2), CreatePendingDoc(3));
        await db.SaveChangesAsync();

        var docService = new Mock<IDocumentService>();
        // Mock: mỗi lần SyncToSapAsync() được gọi → mark 1 doc IsSync=true (giả lập legacy behavior)
        var nextDocId = 1;
        docService.Setup(s => s.SyncToSapAsync()).ReturnsAsync(() =>
        {
            var id = nextDocId++;
            var doc = db.ODOC.FirstOrDefault(d => d.Id == id);
            if (doc != null) { doc.IsSync = true; db.SaveChanges(); }
            return true;
        });

        var sut = new DocumentPushSyncService(docService.Object, db, NullLogger<DocumentPushSyncService>.Instance);

        var result = await sut.PushPendingBatchAsync(CancellationToken.None);

        result.DocsPushed.Should().Be(3, "drain phải process hết 3 doc trong 1 cycle");
        result.StuckDocs.Should().Be(0);
        result.RemainingDocs.Should().Be(0);
        docService.Verify(s => s.SyncToSapAsync(), Times.Exactly(3));
    }

    [Fact]
    public async Task PushPendingBatchAsync_WhenPushDoesNotAdvance_DetectsStuckAndAborts()
    {
        using var db = TestDbContextFactory.Create();
        db.ODOC.AddRange(CreatePendingDoc(1), CreatePendingDoc(2));
        await db.SaveChangesAsync();

        var docService = new Mock<IDocumentService>();
        // Mock: SyncToSapAsync return true nhưng KHÔNG mark IsSync (giả lập bug "catch nuốt lỗi return true")
        docService.Setup(s => s.SyncToSapAsync()).ReturnsAsync(true);

        var sut = new DocumentPushSyncService(docService.Object, db, NullLogger<DocumentPushSyncService>.Instance);

        var result = await sut.PushPendingBatchAsync(CancellationToken.None);

        result.DocsPushed.Should().Be(0, "không doc nào thực sự sync");
        result.StuckDocs.Should().BeGreaterThan(0, "phải detect stuck-doc thay vì lặp vô tận");
        result.RemainingDocs.Should().Be(2, "không doc nào ra khỏi queue");
        // Quan trọng: không lặp vô tận — chỉ gọi 1 lần rồi abort
        docService.Verify(s => s.SyncToSapAsync(), Times.Once);
    }

    [Fact]
    public async Task PushPendingBatchAsync_HonorsCancellationToken()
    {
        using var db = TestDbContextFactory.Create();
        db.ODOC.Add(CreatePendingDoc(1));
        await db.SaveChangesAsync();

        var docService = new Mock<IDocumentService>();
        var sut = new DocumentPushSyncService(docService.Object, db, NullLogger<DocumentPushSyncService>.Instance);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => sut.PushPendingBatchAsync(cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
