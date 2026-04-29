using System.Text.Json;
using System.Text.Json.Serialization;

namespace BackEndAPI.Models.Other
{
    public class ZaloTokens
    {
        public int Id { get; set; }
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public int expires_in { get; set; }
    }
    public class ZaloAccess
    {
        public int Id { get; set; }
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public int expires_in { get; set; }
        public string templateConfirmed { get; set; }
        public string templateCompleted { get; set; }
    }
    public class ZaloTokenConfig
    {
        public string RefreshToken { get; set; }
        public string AppId { get; set; }
        public string SecretKey { get; set; }
        public string TokenUrl { get; set; }
        public string ConnectionString { get; set; }
    }

    public class ZaloTokenResponse
    {
        public string access_token { get; set; }
        [JsonConverter(typeof(ParseStringToIntConverter))]
        public Int32 expires_in { get; set; }
        public string refresh_token { get; set; }
        public string error { get; set; }
        public string error_description { get; set; }
    }
    public class ParseStringToIntConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                return int.TryParse(str, out int val) ? val : 0;
            }
            return reader.GetInt32();
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
    public class ZaloErrorResponse
    {
        public int error { get; set; }
        public string error_name { get; set; }
        public string error_description { get; set; }
    }
}
