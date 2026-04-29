using Newtonsoft.Json;

namespace BackEndAPI.Models.BusinessPartners
{
    public class BusinessPartnerView
    {
        [JsonProperty("item")]
        public string? item { get; set; }
        [JsonProperty("images")]
        public IFormFile? images { get; set; }
    }
}
