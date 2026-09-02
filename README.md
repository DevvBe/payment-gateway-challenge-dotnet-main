# Instructions for candidates

This is the .NET version of the Payment Gateway challenge. If you haven't already read this [README.md](https://github.com/cko-recruitment/) on the details of this exercise, please do so now. 

## Template structure
```
src/
    PaymentGateway.Api - a skeleton ASP.NET Core Web API
test/
    PaymentGateway.Api.Tests - an empty xUnit test project
imposters/ - contains the bank simulator configuration. Don't change this

.editorconfig - don't change this. It ensures a consistent set of rules for submissions when reformatting code
docker-compose.yml - configures the bank simulator
PaymentGateway.sln
```

Feel free to change the structure of the solution, use a different test library etc.
---

## Solution notes

**POST /api/Payments** — validate → `400 Rejected` on failure; else call the bank → `201 Created` (`Authorized`/`Declined`); bank down → `503`, nothing persisted.

**GET /api/Payments/{id}** — `404` or `200`.

### Design notes
- Three separate response contracts (`PostPaymentResponse`, `GetPaymentResponse`, `RejectedPaymentResponse`) — Rejected has no `Id`.
- `PaymentStatus` serializes as a string .
- `CardNumber`/`Cvv`/`CardNumberLastFour` are `string` .
- Expiry check compares year/month .
- `Amount` is `int?` (nullable) . Negative amounts are rejected 
-  tests run against the real simulator (`docker-compose up`) .
- Currencies selected at 3: `USD`, `GBP`, `EUR`.
- Assumption: a card is treated as expired as soon as the current month reaches its expiry month/year — "in the future" is interpreted strictly (a card expiring this month is rejected, not valid through the end of the month).
- `PaymentsRepository` is an in-memory, thread-safe store (`ConcurrentDictionary`).

### Running the tests
```bash
docker-compose up -d
dotnet test
```
