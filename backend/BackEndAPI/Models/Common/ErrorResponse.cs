using System.ComponentModel;
using System.Text.Json.Serialization;

namespace BackEndAPI.Models.Common;

public class ErrorResponse
{
    [JsonPropertyName("code")] public string Code { get; set; } = "<unknown>";
    [JsonPropertyName("httpStatusCode")] public int HttpStatusCode { get; set; }
    [JsonPropertyName("error")] public string Message { get; set; } = string.Empty;
}