using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atoship;
using Atoship.Models;

namespace Atoship.Examples
{
    public class BasicExample
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("=== atoship C# SDK Basic Example ===");
            Console.WriteLine();

            // Initialize the SDK
            var client = new AtoshipClient(options =>
            {
                options.ApiKey = Environment.GetEnvironmentVariable("ATOSHIP_API_KEY") ?? "your-api-key";
                options.BaseUrl = "https://api.atoship.com";
                options.EnableLogging = true;
            });

            try
            {
                // Example 1: Create an order
                Console.WriteLine("1. Creating an order...");
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
                    RecipientEmail = "john.doe@example.com",
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
                        },
                        new OrderItem
                        {
                            Name = ".NET Framework Guide",
                            SKU = "BOOK-NET-001",
                            Quantity = 1,
                            UnitPrice = 34.99m,
                            Weight = 1.8m,
                            WeightUnit = "lb"
                        }
                    }
                });
                Console.WriteLine($"✅ Order created successfully: {order.Id}");
                Console.WriteLine();

                // Example 2: Get shipping rates
                Console.WriteLine("2. Getting shipping rates...");
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
                        Weight = 3.3m,
                        WeightUnit = "lb"
                    }
                });

                Console.WriteLine($"Found {rates.Count} shipping rates:");
                foreach (var rate in rates)
                {
                    Console.WriteLine($"  - {rate.Carrier} {rate.Service}: ${rate.Rate:F2} ({rate.DeliveryDays} days)");
                    if (rate.IsDiscounted)
                    {
                        Console.WriteLine($"    💰 Save {rate.DiscountPercentage:F1}% off retail price!");
                    }
                }
                Console.WriteLine();

                // Example 3: Compare rates to find best option
                Console.WriteLine("3. Comparing rates...");
                var comparison = await client.Shipping.CompareRatesAsync(new RateRequest
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
                        Weight = 3.3m,
                        WeightUnit = "lb"
                    }
                });

                if (comparison.HasRates)
                {
                    Console.WriteLine($"💵 Cheapest: {comparison.Cheapest.Carrier} - ${comparison.Cheapest.Rate:F2}");
                    Console.WriteLine($"⚡ Fastest: {comparison.Fastest.Carrier} - {comparison.Fastest.DeliveryDays} days");
                    Console.WriteLine($"⭐ Best Value: {comparison.BestValue.Carrier} - ${comparison.BestValue.Rate:F2} in {comparison.BestValue.DeliveryDays} days");
                }
                Console.WriteLine();

                // Example 4: Validate an address
                Console.WriteLine("4. Validating an address...");
                var validation = await client.Addresses.ValidateAsync(new ValidateAddressRequest
                {
                    Name = "Jane Smith",
                    Street1 = "1600 Amphitheatre Parkway",
                    City = "Mountain View",
                    State = "CA",
                    PostalCode = "94043",
                    Country = "US"
                });

                if (validation.IsValid)
                {
                    Console.WriteLine("✅ Address is valid");
                    if (validation.NormalizedAddress != null)
                    {
                        Console.WriteLine($"Normalized: {validation.NormalizedAddress.Street1}, {validation.NormalizedAddress.City}, {validation.NormalizedAddress.State} {validation.NormalizedAddress.PostalCode}");
                    }
                }
                else
                {
                    Console.WriteLine("❌ Address validation failed");
                    if (validation.Errors.Any())
                    {
                        Console.WriteLine("Errors:");
                        foreach (var error in validation.Errors)
                        {
                            Console.WriteLine($"  - {error}");
                        }
                    }
                    if (validation.Suggestions.Any())
                    {
                        Console.WriteLine("Suggested addresses:");
                        foreach (var addr in validation.Suggestions)
                        {
                            Console.WriteLine($"  - {addr.Street1}, {addr.City}, {addr.State} {addr.PostalCode}");
                        }
                    }
                }
                Console.WriteLine();

                // Example 5: Track a package
                Console.WriteLine("5. Tracking a package...");
                try
                {
                    var tracking = await client.Tracking.TrackAsync("1Z999AA10123456784");
                    
                    Console.WriteLine($"Package Status: {tracking.Status}");
                    Console.WriteLine($"Current Location: {tracking.CurrentLocation}");
                    
                    if (tracking.IsDelivered)
                    {
                        Console.WriteLine("✅ Package delivered!");
                        Console.WriteLine($"Delivered at: {tracking.ActualDelivery}");
                        if (!string.IsNullOrEmpty(tracking.Signature))
                        {
                            Console.WriteLine($"Signature: {tracking.Signature}");
                        }
                    }
                    else if (tracking.EstimatedDelivery.HasValue)
                    {
                        Console.WriteLine($"📦 Estimated delivery: {tracking.EstimatedDelivery}");
                    }

                    if (tracking.IsInTransit)
                    {
                        Console.WriteLine($"Package has been in transit for {tracking.DaysInTransit} days");
                    }

                    if (tracking.Events.Any())
                    {
                        Console.WriteLine("\nRecent tracking events:");
                        foreach (var trackingEvent in tracking.Events.Take(3))
                        {
                            Console.WriteLine($"  {trackingEvent.Timestamp}: {trackingEvent.Description}");
                            if (!string.IsNullOrEmpty(trackingEvent.Location))
                            {
                                Console.WriteLine($"    Location: {trackingEvent.Location}");
                            }
                        }
                    }
                }
                catch (AtoshipNotFoundException ex)
                {
                    Console.WriteLine($"⚠️  Could not track package: {ex.Message}");
                }
                Console.WriteLine();

                // Example 6: Create a webhook
                Console.WriteLine("6. Creating a webhook...");
                var webhook = await client.Webhooks.CreateAsync(new CreateWebhookRequest
                {
                    Url = "https://your-app.com/webhooks/atoship",
                    Events = new List<string> { "order.shipped", "label.created", "tracking.updated" },
                    Active = true
                });
                Console.WriteLine($"✅ Webhook created: {webhook.Id}");
                Console.WriteLine($"   URL: {webhook.Url}");
                Console.WriteLine($"   Events: {string.Join(", ", webhook.Events)}");
                Console.WriteLine();

                // Example 7: List recent orders
                Console.WriteLine("7. Listing recent orders...");
                var orders = await client.Orders.ListAsync(new ListOrdersRequest
                {
                    Page = 1,
                    Limit = 5
                });

                Console.WriteLine($"Found {orders.Total} total orders");
                Console.WriteLine($"Showing page {orders.Page} of {Math.Ceiling((double)orders.Total / orders.Limit)}");

                foreach (var orderItem in orders.Items)
                {
                    Console.WriteLine($"  - {orderItem.OrderNumber}: {orderItem.RecipientName} ({orderItem.Status})");
                }

                if (orders.HasMore)
                {
                    Console.WriteLine($"  ... more orders available on page {orders.Page + 1}");
                }
                Console.WriteLine();

                // Example 8: Get user profile and usage stats
                Console.WriteLine("8. Getting user profile and usage...");
                var profile = await client.Users.GetProfileAsync();
                Console.WriteLine($"User: {profile.Name} ({profile.Email})");
                Console.WriteLine($"Account Type: {profile.AccountType}");

                var usage = await client.Users.GetUsageStatsAsync();
                Console.WriteLine("API Usage:");
                Console.WriteLine($"  - Orders Created: {usage.OrdersCreated}");
                Console.WriteLine($"  - Labels Purchased: {usage.LabelsPurchased}");
                Console.WriteLine($"  - Total Shipping Cost: ${usage.TotalShippingCost:F2}");
                Console.WriteLine();

                Console.WriteLine("=== Example completed successfully! ===");
            }
            catch (AtoshipAuthenticationException ex)
            {
                Console.WriteLine($"❌ Authentication failed: {ex.Message}");
                Console.WriteLine("Please check your API key");
            }
            catch (AtoshipValidationException ex)
            {
                Console.WriteLine($"❌ Validation error: {ex.Message}");
                if (ex.ValidationErrors != null)
                {
                    Console.WriteLine("Details:");
                    foreach (var error in ex.ValidationErrors)
                    {
                        Console.WriteLine($"  - {error.Field}: {error.Message}");
                    }
                }
            }
            catch (AtoshipRateLimitException ex)
            {
                Console.WriteLine($"❌ Rate limit exceeded: {ex.Message}");
                if (ex.RetryAfter.HasValue)
                {
                    Console.WriteLine($"Retry after: {ex.RetryAfter} seconds");
                }
            }
            catch (AtoshipNetworkException ex)
            {
                Console.WriteLine($"❌ Network error: {ex.Message}");
                Console.WriteLine("Please check your internet connection");
            }
            catch (AtoshipApiException ex)
            {
                Console.WriteLine($"❌ API error: {ex.Message}");
                if (!string.IsNullOrEmpty(ex.ErrorCode))
                {
                    Console.WriteLine($"Error code: {ex.ErrorCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Unexpected error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                client.Dispose();
            }
        }
    }
}