using System.Collections.Concurrent;

using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public class PaymentsRepository
{
    private readonly ConcurrentDictionary<Guid, PostPaymentResponse> _payments = new();

    public void Add(PostPaymentResponse payment)
    {
        _payments[payment.Id] = payment;
    }

    public PostPaymentResponse? Get(Guid id)
    {
        _payments.TryGetValue(id, out var payment);
        return payment;
    }
}