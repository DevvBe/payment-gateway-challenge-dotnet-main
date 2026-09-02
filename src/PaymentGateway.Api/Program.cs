using PaymentGateway.Api.Services;
using PaymentGateway.Api.Validations;
using PaymentGateway.Api.Clients;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<PaymentsRepository>();
builder.Services.AddSingleton<PaymentRequestValidator>();

builder.Services.AddHttpClient<IAcquiringBankClient, AcquiringBankClient>(client =>
{
    var baseUrl = builder.Configuration["BankSimulator:BaseUrl"]
        ?? throw new InvalidOperationException("BankSimulator:BaseUrl is not configured.");
    client.BaseAddress = new Uri(baseUrl);
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
public partial class Program { } 
