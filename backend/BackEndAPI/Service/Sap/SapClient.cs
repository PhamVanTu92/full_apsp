using B1SLayer;
using BackEndAPI.Service.Sap.Models;

namespace BackEndAPI.Service.Sap;

public class SapClient : ISapClient
{
    private readonly SLConnection _sl;
    private readonly ILogger<SapClient> _logger;

    public SapClient(SLConnection sl, ILogger<SapClient> logger)
    {
        _sl = sl;
        _logger = logger;
    }

    public async Task<List<SapDraftSummary>> GetDraftsByInvoiceCodeAsync(string invoiceCode, CancellationToken ct)
    {
        // Escape OData literal — '' = single quote.
        // (HttpWebRequest legacy concat string trực tiếp → injection rủi ro)
        var safe = invoiceCode.Replace("'", "''");
        var filter = $"U_MDHPT eq '{safe}'";

        try
        {
            var response = await _sl.Request("Drafts")
                .Filter(filter)
                .Select("DocEntry,U_MDHPT,DocStatus")
                .GetAsync<DraftCollection>();

            return response?.Value ?? new List<SapDraftSummary>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "GetDraftsByInvoiceCodeAsync failed for {InvoiceCode}", invoiceCode);
            throw;
        }
    }

    private class DraftCollection
    {
        public List<SapDraftSummary>? Value { get; set; }
    }
}
