# atoship C# SDK

The official C# SDK for the atoship API. This SDK provides a comprehensive, type-safe interface for all atoship shipping and logistics operations.

## Features

- 🚀 **Type-Safe**: Full IntelliSense support with strongly typed models
- 🔒 **Secure**: Built-in API key management and request signing
- 🔄 **Robust**: Automatic retries with exponential backoff
- 📦 **Comprehensive**: Covers all atoship API endpoints
- 🎯 **.NET Standard 2.0+**: Works with .NET Core, .NET 5+, and .NET Framework
- 🧪 **Well-tested**: Comprehensive unit and integration tests
- 📚 **Documented**: XML documentation for IntelliSense

## Installation

### Package Manager Console

```powershell
Install-Package Atoship.SDK
```

### .NET CLI

```bash
dotnet add package Atoship.SDK
```

### PackageReference

```xml
<PackageReference Include="Atoship.SDK" Version="1.0.0" />
```

## Quick Start

```csharp
using Atoship;
using Atoship.Models;

// Initialize the SDK
var client = new AtoshipClient("your-api-key");

// Create an order
var order = await client.Orders.CreateAsync(new CreateOrderRequest
{
    OrderNumber = "CS-ORDER-001",
    RecipientName = "John Doe",
    RecipientStreet1 = "123 Main St",
    RecipientCity = "San Francisco",
    RecipientState = "CA",
    RecipientPostalCode = "94105",
    RecipientCountry = "US",
    RecipientPhone = "415-555-0123",
    Items = new List<OrderItem>
    {
        new OrderItem
        {
            Name = "C# Programming Book",
            SKU = "BOOK-CS-001",
            Quantity = 2,
            UnitPrice = 29.99m,
            Weight = 1.5m,
            WeightUnit = "lb"
        }
    }
});

Console.WriteLine($"Order created: {order.Id}");
```

## Configuration

```csharp
// Configure globally
AtoshipClient.Configure(config =>
{
    config.ApiKey = Environment.GetEnvironmentVariable("ATOSHIP_API_KEY");
    config.BaseUrl = "https://api.atoship.com";
    config.Timeout = TimeSpan.FromSeconds(30);
    config.MaxRetries = 3;
    config.EnableLogging = true;
});

// Or per-client
var client = new AtoshipClient(options =>
{
    options.ApiKey = "your-api-key";
    options.Timeout = TimeSpan.FromSeconds(60);
    options.EnableLogging = Environment.IsDevelopment();
});
```

## Examples

### Get Shipping Rates

```csharp
var rates = await client.Shipping.GetRatesAsync(new RateRequest
{
    FromAddress = new Address
    {
        Street1 = "456 Oak Ave",
        City = "Los Angeles",
        State = "CA",
        PostalCode = "90001",
        Country = "US"
    },
    ToAddress = new Address
    {
        Street1 = "789 Pine St",
        City = "New York",
        State = "NY",
        PostalCode = "10001",
        Country = "US"
    },
    Parcel = new Parcel
    {
        Length = 10,
        Width = 8,
        Height = 6,
        DimUnit = "in",
        Weight = 2.5m,
        WeightUnit = "lb"
    }
});

foreach (var rate in rates)
{
    Console.WriteLine($"{rate.Carrier} {rate.Service}: ${rate.Rate:F2} ({rate.DeliveryDays} days)");
}
```

### Purchase a Label

```csharp
var label = await client.Shipping.PurchaseLabelAsync(new PurchaseLabelRequest
{
    RateId = "rate_123456",
    OrderId = "order_789012"
});

Console.WriteLine($"Label URL: {label.LabelUrl}");
Console.WriteLine($"Tracking: {label.TrackingNumber}");
```

### Track a Package

```csharp
var tracking = await client.Tracking.TrackAsync("1Z999AA10123456784");

Console.WriteLine($"Status: {tracking.Status}");
Console.WriteLine($"Location: {tracking.CurrentLocation}");

foreach (var trackingEvent in tracking.Events)
{
    Console.WriteLine($"{trackingEvent.Timestamp}: {trackingEvent.Description} at {trackingEvent.Location}");
}
```

### Validate an Address

```csharp
var result = await client.Addresses.ValidateAsync(new ValidateAddressRequest
{
    Name = "Jane Smith",
    Street1 = "1600 Amphitheatre Parkway",
    City = "Mountain View",
    State = "CA",
    PostalCode = "94043",
    Country = "US"
});

if (result.IsValid)
{
    Console.WriteLine("✅ Address is valid");
}
else
{
    Console.WriteLine("❌ Address validation failed");
    Console.WriteLine($"Errors: {string.Join(", ", result.Errors)}");
    
    if (result.Suggestions.Any())
    {
        Console.WriteLine("Suggested addresses:");
        foreach (var address in result.Suggestions)
        {
            Console.WriteLine($"  - {address.Street1}, {address.City}, {address.State} {address.PostalCode}");
        }
    }
}
```

## Async/Await Support

All methods support async/await pattern:

```csharp
public async Task ProcessOrdersAsync()
{
    // List orders
    var orders = await client.Orders.ListAsync(new ListOrdersRequest
    {
        Page = 1,
        Limit = 100,
        Status = "pending"
    });

    // Process each order
    var tasks = orders.Items.Select(async order =>
    {
        // Get rates
        var rates = await client.Shipping.GetRatesAsync(BuildRateRequest(order));
        
        // Select best rate
        var bestRate = rates.OrderBy(r => r.Rate).First();
        
        // Purchase label
        var label = await client.Shipping.PurchaseLabelAsync(new PurchaseLabelRequest
        {
            RateId = bestRate.Id,
            OrderId = order.Id
        });
        
        return label;
    });

    var labels = await Task.WhenAll(tasks);
    Console.WriteLine($"Processed {labels.Length} orders");
}
```

## Error Handling

```csharp
try
{
    var order = await client.Orders.CreateAsync(orderData);
}
catch (AtoshipValidationException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
    foreach (var error in ex.ValidationErrors)
    {
        Console.WriteLine($"  - {error.Field}: {error.Message}");
    }
}
catch (AtoshipRateLimitException ex)
{
    Console.WriteLine($"Rate limit exceeded. Retry after: {ex.RetryAfter} seconds");
}
catch (AtoshipAuthenticationException ex)
{
    Console.WriteLine($"Authentication failed: {ex.Message}");
}
catch (AtoshipApiException ex)
{
    Console.WriteLine($"API error: {ex.Message} (Code: {ex.ErrorCode})");
}
catch (AtoshipNetworkException ex)
{
    Console.WriteLine($"Network error: {ex.Message}");
}
```

## Dependency Injection

```csharp
// Startup.cs or Program.cs
services.AddAtoship(Configuration);

// Or with options
services.AddAtoship(options =>
{
    options.ApiKey = Configuration["Atoship:ApiKey"];
    options.Timeout = TimeSpan.FromSeconds(30);
});

// Use in controller
public class ShippingController : ControllerBase
{
    private readonly IAtoshipClient _atoship;

    public ShippingController(IAtoshipClient atoship)
    {
        _atoship = atoship;
    }

    [HttpPost("ship")]
    public async Task<IActionResult> ShipOrder(string orderId)
    {
        var order = await _atoship.Orders.GetAsync(orderId);
        // ... process shipping
        return Ok(order);
    }
}
```

## Webhooks

```csharp
// Create webhook endpoint
[HttpPost("webhooks/atoship")]
public async Task<IActionResult> HandleWebhook()
{
    var signature = Request.Headers["X-Atoship-Signature"];
    var body = await Request.GetRawBodyStringAsync();
    
    if (!AtoshipWebhook.VerifySignature(body, signature, webhookSecret))
    {
        return Unauthorized();
    }

    var webhookEvent = AtoshipWebhook.ParseEvent(body);
    
    switch (webhookEvent.Type)
    {
        case "order.shipped":
            await HandleOrderShipped(webhookEvent.Data);
            break;
        case "tracking.updated":
            await HandleTrackingUpdate(webhookEvent.Data);
            break;
    }

    return Ok();
}
```

## Pagination

```csharp
var allOrders = new List<Order>();
var page = 1;
PagedResult<Order> result;

do
{
    result = await client.Orders.ListAsync(new ListOrdersRequest
    {
        Page = page++,
        Limit = 100
    });
    
    allOrders.AddRange(result.Items);
    
} while (result.HasMore);

Console.WriteLine($"Total orders: {allOrders.Count}");
```

## Batch Operations

```csharp
// Batch create orders
var orders = await client.Orders.BatchCreateAsync(ordersList);

// Batch track packages
var trackingInfos = await client.Tracking.BatchTrackAsync(trackingNumbers);

// Batch validate addresses
var validations = await client.Addresses.BatchValidateAsync(addresses);
```

## Logging

```csharp
// Configure logging
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.AddDebug();
});

// SDK will use ILogger when available
var client = new AtoshipClient(options =>
{
    options.ApiKey = "your-api-key";
    options.LogLevel = LogLevel.Debug;
});
```

## Testing

Run tests:

```bash
dotnet test
```

Run with coverage:

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Migration from Other SDKs

### From REST API

```csharp
// Before (HttpClient)
var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
var response = await httpClient.PostAsJsonAsync("https://api.atoship.com/api/orders", orderData);
var order = await response.Content.ReadFromJsonAsync<Order>();

// After (SDK)
var client = new AtoshipClient(apiKey);
var order = await client.Orders.CreateAsync(orderData);
```

## Contributing

We welcome contributions! Please see our contributing guidelines for details.

## License

MIT License - see LICENSE file for details.

## Support

- Documentation: https://atoship.com/docs
- API Reference: https://atoship.com/docs/api-reference
- Support: support@atoship.com