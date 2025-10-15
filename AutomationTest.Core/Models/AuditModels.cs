using System.Text.Json.Serialization;

namespace AutomationTest.Core.Models
{
    public class EffectiveWindow
    {
        public string Start { get; set; } = string.Empty;
        public string End { get; set; } = string.Empty;
    }

    public class AuditRequest
    {
        public string BillingId { get; set; } = string.Empty;
        public string UnitId { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string CodeId { get; set; } = string.Empty;
        public EffectiveWindow EffectiveWindow { get; set; } = new();
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; } = string.Empty;
    }

    public class AuditHistory
    {
        [JsonPropertyName("AuditId")]
        public string AuditId { get; set; } = string.Empty;

        [JsonPropertyName("createdAt")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; } = string.Empty;

        [JsonPropertyName("PDescription")]
        public string PDescription { get; set; } = string.Empty;

        [JsonPropertyName("TDescription")]
        public string TDescription { get; set; } = string.Empty;

        [JsonPropertyName("details")]
        public string Details { get; set; } = string.Empty;

        [JsonPropertyName("ImpKey")]
        public string ImpKey { get; set; } = string.Empty;
    }

    public class AuditHistoryResponse
    {
        [JsonPropertyName("items")]
        public List<AuditHistory> Items { get; set; } = new();

        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }
}
