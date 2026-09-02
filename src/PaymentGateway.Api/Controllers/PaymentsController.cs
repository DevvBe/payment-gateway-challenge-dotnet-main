using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Clients.Models;
using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Validations;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : ControllerBase
{
    private readonly PaymentsRepository _paymentsRepository;
       private readonly PaymentRequestValidator _validator;
    private readonly IAcquiringBankClient _bankClient;
    public PaymentsController(PaymentsRepository paymentsRepository, PaymentRequestValidator validator, IAcquiringBankClient bankClient)
    {
        _paymentsRepository = paymentsRepository;
        _validator = validator;
        _bankClient = bankClient;
    }
 

    [HttpGet("{id:guid}" , Name = "GetPayment")]
    public ActionResult<GetPaymentResponse?> GetPayment(Guid id)
    {
        var payment = _paymentsRepository.Get(id);

            if (payment is null)
        {
            return NotFound();
        }

        return Ok(ToGetPaymentResponse(payment));
    }
     [HttpPost]
    public async Task<ActionResult<PostPaymentResponse>> CreatePaymentAsync(
        PostPaymentRequest request, CancellationToken cancellationToken)
    {
        var validationErrors = _validator.Validate(request);

        if (validationErrors.Count > 0)
        {
            return BadRequest(new RejectedPaymentResponse { Errors = validationErrors });
        }

        var bankResult = await AuthorizeWithBankAsync(request, cancellationToken);

        if (bankResult is null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                "The acquiring bank is currently unavailable. Please try again later.");
        }

        var payment = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Status = bankResult.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined,
            CardNumberLastFour = request.CardNumber[^4..],
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency,
            Amount = request.Amount!.Value
        };

        _paymentsRepository.Add(payment);

        return CreatedAtRoute(nameof(GetPayment), new { id = payment.Id }, payment);
    }

    private async Task<BankPaymentResult?> AuthorizeWithBankAsync(
        PostPaymentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return await _bankClient.AuthorizeAsync(ToBankPaymentRequest(request), cancellationToken);
        }
        catch (BankUnavailableException)
        {
            return null;
        }
    }

    private static BankPaymentRequest ToBankPaymentRequest(PostPaymentRequest request) => new()
    {
        CardNumber = request.CardNumber,
        ExpiryDate = $"{request.ExpiryMonth:D2}/{request.ExpiryYear}",
        Currency = request.Currency,
        Amount = request.Amount!.Value,
        Cvv = request.Cvv
    };

    
      private static GetPaymentResponse ToGetPaymentResponse(PostPaymentResponse payment) => new()
    {
        Id = payment.Id,
        Status = payment.Status,
        CardNumberLastFour = payment.CardNumberLastFour,
        ExpiryMonth = payment.ExpiryMonth,
        ExpiryYear = payment.ExpiryYear,
        Currency = payment.Currency,
        Amount = payment.Amount
    };
}