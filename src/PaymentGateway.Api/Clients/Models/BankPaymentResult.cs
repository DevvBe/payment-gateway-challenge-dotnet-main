using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Clients.Models;

public class BankPaymentResult
{
    [JsonPropertyName("authorized")]
    public bool Authorized { get; set; }

    [JsonPropertyName("authorization_code")]
    public string? AuthorizationCode { get; set; }
}
