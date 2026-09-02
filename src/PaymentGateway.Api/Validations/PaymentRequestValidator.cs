using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Validations;

public class PaymentRequestValidator
{
    private static readonly HashSet<string> AllowedCurrencies = new(StringComparer.Ordinal)
    {
        "USD", "GBP", "EUR"
    };

    public IReadOnlyList<string> Validate(PostPaymentRequest request)
    {
        var errors = new List<string>();

        ValidateCardNumber(request, errors);
        ValidateExpiry(request, errors);
        ValidateCurrency(request, errors);
        ValidateAmount(request, errors);
        ValidateCvv(request, errors);

        return errors;
    }

    private static void ValidateCardNumber(PostPaymentRequest request, List<string> errors)
    {
        var cardNumber = request.CardNumber ?? string.Empty;

        if (cardNumber.Length is < 14 or > 19 || !cardNumber.All(char.IsDigit))
        {
            errors.Add("CardNumber must be between 14 and 19 numeric characters.");
        }
    }

    private static void ValidateExpiry(PostPaymentRequest request, List<string> errors)
    {
        if (request.ExpiryMonth is < 1 or > 12)
        {
            errors.Add("ExpiryMonth must be between 1 and 12.");
            return;
        }

        var now = DateTime.UtcNow;

        var isPastOrCurrentMonth =
            request.ExpiryYear < now.Year ||
            (request.ExpiryYear == now.Year && request.ExpiryMonth <= now.Month);

        if (isPastOrCurrentMonth)
        {
            errors.Add("Expiry date (ExpiryMonth/ExpiryYear) must be in the future.");
        }
    }

    private static void ValidateCurrency(PostPaymentRequest request, List<string> errors)
    {
        var currency = request.Currency ?? string.Empty;

        if (currency.Length != 3 || !AllowedCurrencies.Contains(currency))
        {
            errors.Add($"Currency must be one of: {string.Join(", ", AllowedCurrencies)}.");
        }
    }

    private static void ValidateAmount(PostPaymentRequest request, List<string> errors)
    {

        if (request.Amount is null)
        {
            errors.Add("Amount is required.");
            return;
        }


        if (request.Amount < 0)
        {
            errors.Add("Amount must be zero or a positive integer.");
        }
    }

    private static void ValidateCvv(PostPaymentRequest request, List<string> errors)
    {
        var cvv = request.Cvv ?? string.Empty;

        if (cvv.Length is < 3 or > 4 || !cvv.All(char.IsDigit))
        {
            errors.Add("Cvv must be 3-4 numeric characters.");
        }
    }
}