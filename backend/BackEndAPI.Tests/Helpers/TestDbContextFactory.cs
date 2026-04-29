using BackEndAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace BackEndAPI.Tests.Helpers;

/// <summary>
/// Tạo AppDbContext in-memory cho unit test. Mỗi test case có DB riêng (qua databaseName)
/// để không leak state giữa các test.
/// </summary>
public static class TestDbContextFactory
{
    public static AppDbContext Create(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .ConfigureWarnings(w =>
            {
                w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning);
                // Ignore nullability check — InMemory enforces non-nullable strings strictly hơn SQL Server thật.
                // Trong test seed data ta KHÔNG cần fill mọi nullable string field.
                w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.ChangesSaved);
            })
            .Options;

        return new AppDbContext(options);
    }
}
