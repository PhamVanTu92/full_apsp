# BackEndAPI.Tests

Unit & integration tests cho BackEndAPI.

## Chạy test

```bash
# Chạy toàn bộ
dotnet test

# Chạy theo filter
dotnet test --filter "FullyQualifiedName~SmokeTests"

# Chạy với coverage (cần cài coverlet)
dotnet test --collect:"XPlat Code Coverage"
```

## Cấu trúc

```
BackEndAPI.Tests/
├── Unit/
│   ├── SmokeTests.cs              # Smoke test xác nhận setup
│   └── Services/
│       └── SampleServiceTests.cs  # Mẫu pattern Moq
├── Integration/                    # (sẽ thêm) test qua HTTP
└── Helpers/                        # (sẽ thêm) builder, fixture
```

## Hướng dẫn viết test mới

Xem `.claude/rules/testing.md` để biết quy ước đặt tên, pattern Arrange/Act/Assert,
và cách mock các dependency thường gặp (DbContext, IConfiguration, IHttpClientFactory).

## Stack

- **xUnit** — test framework
- **Moq 4.20** — mocking
- **FluentAssertions 6.12** — assertion DSL
- **EF Core InMemory** — in-memory database cho integration test
