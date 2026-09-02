
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Validations;


namespace PaymentGateway.Api.Tests.Validations;

[TestFixture]
public class PaymentRequestValidatorTests
{
    private PaymentRequestValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new PaymentRequestValidator();
    }

    private static PostPaymentRequest ValidRequest() => new()
    {
        CardNumber = "2222405343248877",
        ExpiryMonth = 12,
        ExpiryYear = DateTime.UtcNow.Year + 1,
        Currency = "GBP",
        Amount = 100,
        Cvv = "123"
    };

    // --- Card number ---

    [Test]
    public void CardNumber_Missing_IsInvalid()
    {
        var request = ValidRequest();
        request.CardNumber = "";
        Assert.That(_validator.Validate(request), Has.Some.Contains("CardNumber"));
    }

    [Test]
    public void CardNumber_TooShort_IsInvalid()
    {
        var request = ValidRequest();
        request.CardNumber = "1234567890123"; // 13 digits
        Assert.That(_validator.Validate(request), Has.Some.Contains("CardNumber"));
    }

    [Test]
    public void CardNumber_TooLong_IsInvalid()
    {
        var request = ValidRequest();
        request.CardNumber = "12345678901234567890"; // 20 digits
        Assert.That(_validator.Validate(request), Has.Some.Contains("CardNumber"));
    }

    [Test]
    public void CardNumber_NonNumeric_IsInvalid()
    {
        var request = ValidRequest();
        request.CardNumber = "22224053432488AA";
        Assert.That(_validator.Validate(request), Has.Some.Contains("CardNumber"));
    }

    [Test]
    public void CardNumber_FourteenDigits_IsValid()
    {
        var request = ValidRequest();
        request.CardNumber = "12345678901234"; // exactly 14 digits, lower bound
        Assert.That(_validator.Validate(request), Has.None.Contains("CardNumber"));
    }

    [Test]
    public void CardNumber_NineteenDigits_IsValid()
    {
        var request = ValidRequest();
        request.CardNumber = "1234567890123456789"; // exactly 19 digits, upper bound
        Assert.That(_validator.Validate(request), Has.None.Contains("CardNumber"));
    }

    // --- Expiry month / year ---

    [Test]
    public void ExpiryMonth_OutOfRange_IsInvalid()
    {
        var request = ValidRequest();
        request.ExpiryMonth = 13;
        Assert.That(_validator.Validate(request), Has.Some.Contains("Expiry"));
    }

    [Test]
    public void ExpiryMonth_Zero_IsInvalid()
    {
        var request = ValidRequest();
        request.ExpiryMonth = 0;
        Assert.That(_validator.Validate(request), Has.Some.Contains("Expiry"));
    }

    [Test]
    public void ExpiryDate_InThePast_IsInvalid()
    {
        var request = ValidRequest();
        var past = DateTime.UtcNow.AddYears(-1);
        request.ExpiryMonth = past.Month;
        request.ExpiryYear = past.Year;
        Assert.That(_validator.Validate(request), Has.Some.Contains("Expiry"));
    }

    [Test]
    public void ExpiryDate_CurrentMonthAndYear_IsInvalid()
    {
        var request = ValidRequest();
        var now = DateTime.UtcNow;
        request.ExpiryMonth = now.Month;
        request.ExpiryYear = now.Year;
        Assert.That(_validator.Validate(request), Has.Some.Contains("Expiry"));
    }

    [Test]
    public void ExpiryDate_NextMonth_IsValid()
    {
        var request = ValidRequest();
        var future = DateTime.UtcNow.AddMonths(1);
        request.ExpiryMonth = future.Month;
        request.ExpiryYear = future.Year;
        Assert.That(_validator.Validate(request), Has.None.Contains("Expiry"));
    }

    [Test]
    public void ExpiryDate_NextYearSameMonth_IsValid()
    {


        var request = ValidRequest();
        var now = DateTime.UtcNow;
        request.ExpiryMonth = now.Month;
        request.ExpiryYear = now.Year + 1;
        Assert.That(_validator.Validate(request), Has.None.Contains("Expiry"));
    }

    // --- Currency ---

    [Test]
    public void Currency_Missing_IsInvalid()
    {
        var request = ValidRequest();
        request.Currency = "";
        Assert.That(_validator.Validate(request), Has.Some.Contains("Currency"));
    }

    [Test]
    public void Currency_WrongLength_IsInvalid()
    {
        var request = ValidRequest();
        request.Currency = "GB";
        Assert.That(_validator.Validate(request), Has.Some.Contains("Currency"));
    }

    [Test]
    public void Currency_NotInAllowedList_IsInvalid()
    {
        var request = ValidRequest();
        request.Currency = "JPY";
        Assert.That(_validator.Validate(request), Has.Some.Contains("Currency"));
    }

    [Test]
    [TestCase("USD")]
    [TestCase("GBP")]
    [TestCase("EUR")]
    public void Currency_InAllowedList_IsValid(string currency)
    {
        var request = ValidRequest();
        request.Currency = currency;
        Assert.That(_validator.Validate(request), Has.None.Contains("Currency"));
    }

    // --- Amount ---

    [Test]
    public void Amount_Missing_IsInvalid()
    {
        var request = ValidRequest();
        request.Amount = null;
        Assert.That(_validator.Validate(request), Has.Some.Contains("Amount"));
    }

    [Test]
    public void Amount_Negative_IsInvalid()
    {
        var request = ValidRequest();
        request.Amount = -100;
        Assert.That(_validator.Validate(request), Has.Some.Contains("Amount"));
    }

    [Test]
    public void Amount_Zero_IsValid()
    {

        var request = ValidRequest();
        request.Amount = 0;
        Assert.That(_validator.Validate(request), Has.None.Contains("Amount"));
    }

    [Test]
    public void Amount_Positive_IsValid()
    {
        var request = ValidRequest();
        request.Amount = 1050;
        Assert.That(_validator.Validate(request), Has.None.Contains("Amount"));
    }

    // --- CVV ---

    [Test]
    public void Cvv_Missing_IsInvalid()
    {
        var request = ValidRequest();
        request.Cvv = "";
        Assert.That(_validator.Validate(request), Has.Some.Contains("Cvv"));
    }

    [Test]
    public void Cvv_TooShort_IsInvalid()
    {
        var request = ValidRequest();
        request.Cvv = "12";
        Assert.That(_validator.Validate(request), Has.Some.Contains("Cvv"));
    }

    [Test]
    public void Cvv_TooLong_IsInvalid()
    {
        var request = ValidRequest();
        request.Cvv = "12345";
        Assert.That(_validator.Validate(request), Has.Some.Contains("Cvv"));
    }

    [Test]
    public void Cvv_NonNumeric_IsInvalid()
    {
        var request = ValidRequest();
        request.Cvv = "12A";
        Assert.That(_validator.Validate(request), Has.Some.Contains("Cvv"));
    }

    [Test]
    public void Cvv_LeadingZero_IsValid()
    {

        // "012" must remain a valid 3-digit CVV, not become 12.
        var request = ValidRequest();
        request.Cvv = "012";
        Assert.That(_validator.Validate(request), Has.None.Contains("Cvv"));
    }

    [Test]
    [TestCase("123")]
    [TestCase("1234")]
    public void Cvv_ThreeOrFourDigits_IsValid(string cvv)
    {
        var request = ValidRequest();
        request.Cvv = cvv;
        Assert.That(_validator.Validate(request), Has.None.Contains("Cvv"));
    }

    // --- Fully valid request ---

    [Test]
    public void FullyValidRequest_ProducesNoErrors()
    {
        Assert.That(_validator.Validate(ValidRequest()), Is.Empty);
    }
}