using System.Net;
using System.Net.Http.Json;

using PaymentGateway.Api.Clients.Models;

namespace PaymentGateway.Api.Clients;

public class AcquiringBankClient : IAcquiringBankClient
{
    private readonly HttpClient _httpClient;

    public AcquiringBankClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BankPaymentResult> AuthorizeAsync(
        BankPaymentRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("/payments", request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
        {
            throw new BankUnavailableException("The acquiring bank is currently unavailable.");
        }

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<BankPaymentResult>(cancellationToken);
        return result ?? throw new InvalidOperationException("The acquiring bank returned an empty response body.");
    }
}