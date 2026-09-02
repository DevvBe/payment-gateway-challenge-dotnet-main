namespace PaymentGateway.Api.Clients;

public class BankUnavailableException : Exception
{
    public BankUnavailableException(string message) : base(message)
    {
    }
}