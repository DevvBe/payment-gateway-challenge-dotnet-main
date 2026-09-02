using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;

using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Tests.Controllers;

[TestFixture]
public class PaymentsControllerPostTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private static PostPaymentRequest ValidRequest(string cardNumber) => new()
    {
        CardNumber = cardNumber,
        ExpiryMonth = 4,
        ExpiryYear = DateTime.UtcNow.Year + 2,
        Currency = "GBP",
        Amount = 100,
        Cvv = "123"
    };

    [Test]
    public async Task CreatePayment_WithInvalidRequest_ReturnsRejected()
    {
        var request = ValidRequest("123");

        var response = await _client.PostAsJsonAsync("/api/Payments", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var rejected = await response.Content.ReadFromJsonAsync<RejectedPaymentResponse>();
        Assert.That(rejected!.Status, Is.EqualTo(PaymentStatus.Rejected));
        Assert.That(rejected.Errors, Has.Some.Contains("CardNumber"));


    }

    [Test]
    public async Task CreatePayment_WithCardEndingOdd_IsAuthorized()
    {
        // card number ending in an odd digit -> authorized
        var request = ValidRequest("2222405343248871");

        var response = await _client.PostAsJsonAsync("/api/Payments", request);
        var payment = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(payment!.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(payment.Status, Is.EqualTo(PaymentStatus.Authorized));
        Assert.That(payment.CardNumberLastFour, Is.EqualTo("8871"));
        Assert.That(payment.ExpiryMonth, Is.EqualTo(request.ExpiryMonth));
        Assert.That(payment.ExpiryYear, Is.EqualTo(request.ExpiryYear));
        Assert.That(payment.Currency, Is.EqualTo(request.Currency));
        Assert.That(payment.Amount, Is.EqualTo(request.Amount));
    }

    [Test]
    public async Task CreatePayment_WithCardEndingEven_IsDeclined()
    {
        // card number ending in an even digit -> declined
        var request = ValidRequest("2222405343248872");

        var response = await _client.PostAsJsonAsync("/api/Payments", request);
        var payment = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(payment!.Status, Is.EqualTo(PaymentStatus.Declined));
        Assert.That(payment.CardNumberLastFour, Is.EqualTo("8872"));
    }

    [Test]
    public async Task CreatePayment_WithCardEndingZero_ReturnsServiceUnavailable()
    {
        // card number ending in 0 -> 503
        var response = await _client.PostAsJsonAsync("/api/Payments", ValidRequest("2222405343248870"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
    }

    [Test]
    public async Task CreatePayment_ThenRetrieveIt_ReturnsMatchingDetails()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/Payments", ValidRequest("2222405343248871"));
        var created = await createResponse.Content.ReadFromJsonAsync<PostPaymentResponse>();

        var getResponse = await _client.GetAsync($"/api/Payments/{created!.Id}");
        var retrieved = await getResponse.Content.ReadFromJsonAsync<GetPaymentResponse>();

        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(retrieved!.Id, Is.EqualTo(created.Id));
        Assert.That(retrieved.Status, Is.EqualTo(created.Status));
        Assert.That(retrieved.CardNumberLastFour, Is.EqualTo(created.CardNumberLastFour));
        Assert.That(retrieved.ExpiryMonth, Is.EqualTo(created.ExpiryMonth));
        Assert.That(retrieved.ExpiryYear, Is.EqualTo(created.ExpiryYear));
        Assert.That(retrieved.Currency, Is.EqualTo(created.Currency));
        Assert.That(retrieved.Amount, Is.EqualTo(created.Amount));
    }
}