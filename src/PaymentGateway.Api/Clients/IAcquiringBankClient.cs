using PaymentGateway.Api.Clients.Models;


namespace PaymentGateway.Api.Clients;

public interface IAcquiringBankClient
{
    Task<BankPaymentResult> AuthorizeAsync(BankPaymentRequest request, CancellationToken cancellationToken = default);
}