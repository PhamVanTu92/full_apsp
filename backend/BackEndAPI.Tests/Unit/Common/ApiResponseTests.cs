using BackEndAPI.Models.Common;
using FluentAssertions;
using System.Text.Json;
using Xunit;

namespace BackEndAPI.Tests.Unit.Common;

/// <summary>
/// Test cấu trúc + serialization của ApiResponse — đảm bảo format JSON đầu ra đồng nhất.
/// </summary>
public class ApiResponseTests
{
    [Fact]
    public void Ok_Default_Returns200WithDefaultMessage()
    {
        var resp = ApiResponse<string>.Ok("hello");

        resp.Success.Should().BeTrue();
        resp.StatusCode.Should().Be(200);
        resp.Code.Should().Be("OK");
        resp.Message.Should().Be("Success");
        resp.Data.Should().Be("hello");
        resp.Errors.Should().BeNull();
    }

    [Fact]
    public void Ok_CustomMessage_PreservesIt()
    {
        var resp = ApiResponse<int>.Ok(42, "Tạo thành công");
        resp.Message.Should().Be("Tạo thành công");
        resp.Data.Should().Be(42);
    }

    [Fact]
    public void Created_Returns201()
    {
        var resp = ApiResponse<object>.Created(new { id = 1 });
        resp.StatusCode.Should().Be(201);
        resp.Code.Should().Be("CREATED");
    }

    [Fact]
    public void Fail_BadRequest_HasErrorsList()
    {
        var resp = ApiResponse.Fail(400, "VALIDATION_ERROR", "Dữ liệu không hợp lệ",
            new List<string> { "Name required", "Age must be positive" });

        resp.Success.Should().BeFalse();
        resp.StatusCode.Should().Be(400);
        resp.Code.Should().Be("VALIDATION_ERROR");
        resp.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void Serialize_UsesCamelCase_OutputMatchesContract()
    {
        var resp = ApiResponse<string>.Ok("data");
        resp.TraceId = "00-abc";

        var json = JsonSerializer.Serialize(resp);

        json.Should().Contain("\"success\":true");
        json.Should().Contain("\"statusCode\":200");
        json.Should().Contain("\"code\":\"OK\"");
        json.Should().Contain("\"message\":\"Success\"");
        json.Should().Contain("\"data\":\"data\"");
        json.Should().Contain("\"traceId\":\"00-abc\"");
        json.Should().Contain("\"timestamp\":");
    }

    [Fact]
    public void Serialize_NullTraceId_OmittedFromOutput()
    {
        var resp = ApiResponse<int>.Ok(1);

        var json = JsonSerializer.Serialize(resp);

        json.Should().NotContain("\"traceId\"", "TraceId null phải bị omit (JsonIgnoreCondition.WhenWritingNull)");
    }

    [Fact]
    public void Serialize_NullErrors_OmittedFromOutput()
    {
        var resp = ApiResponse<int>.Ok(1);
        var json = JsonSerializer.Serialize(resp);
        json.Should().NotContain("\"errors\"");
    }
}
