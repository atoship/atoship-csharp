using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Atoship.Services;
using Atoship.Configuration;

namespace Atoship
{
    /// <summary>
    /// Main client for interacting with the atoship API
    /// </summary>
    public class AtoshipClient : IAtoshipClient
    {
        private readonly HttpClient _httpClient;
        private readonly AtoshipOptions _options;
        private readonly ILogger<AtoshipClient>? _logger;

        /// <summary>
        /// Orders service for order management
        /// </summary>
        public IOrdersService Orders { get; }

        /// <summary>
        /// Addresses service for address validation and management
        /// </summary>
        public IAddressesService Addresses { get; }

        /// <summary>
        /// Shipping service for rate quotes and label generation
        /// </summary>
        public IShippingService Shipping { get; }

        /// <summary>
        /// Tracking service for package tracking
        /// </summary>
        public ITrackingService Tracking { get; }

        /// <summary>
        /// Users service for user management
        /// </summary>
        public IUsersService Users { get; }

        /// <summary>
        /// Carriers service for carrier account management
        /// </summary>
        public ICarriersService Carriers { get; }

        /// <summary>
        /// Webhooks service for webhook management
        /// </summary>
        public IWebhooksService Webhooks { get; }

        /// <summary>
        /// Admin service for administrative operations
        /// </summary>
        public IAdminService Admin { get; }

        /// <summary>
        /// Initialize a new atoship client with API key
        /// </summary>
        /// <param name="apiKey">Your atoship API key</param>
        public AtoshipClient(string apiKey) : this(new AtoshipOptions { ApiKey = apiKey })
        {
        }

        /// <summary>
        /// Initialize a new atoship client with options
        /// </summary>
        /// <param name="options">Client configuration options</param>
        public AtoshipClient(AtoshipOptions options) : this(options, null, null)
        {
        }

        /// <summary>
        /// Initialize a new atoship client with options configuration
        /// </summary>
        /// <param name="configure">Options configuration action</param>
        public AtoshipClient(Action<AtoshipOptions> configure)
        {
            var options = new AtoshipOptions();
            configure(options);
            Initialize(options, null, null);
        }

        /// <summary>
        /// Initialize a new atoship client with dependency injection
        /// </summary>
        internal AtoshipClient(AtoshipOptions options, HttpClient? httpClient, ILogger<AtoshipClient>? logger)
        {
            Initialize(options, httpClient, logger);
        }

        private void Initialize(AtoshipOptions options, HttpClient? httpClient, ILogger<AtoshipClient>? logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _options.Validate();

            _logger = logger;
            _httpClient = httpClient ?? CreateHttpClient();

            // Initialize services
            Orders = new OrdersService(_httpClient, _options, _logger);
            Addresses = new AddressesService(_httpClient, _options, _logger);
            Shipping = new ShippingService(_httpClient, _options, _logger);
            Tracking = new TrackingService(_httpClient, _options, _logger);
            Users = new UsersService(_httpClient, _options, _logger);
            Carriers = new CarriersService(_httpClient, _options, _logger);
            Webhooks = new WebhooksService(_httpClient, _options, _logger);
            Admin = new AdminService(_httpClient, _options, _logger);
        }

        private HttpClient CreateHttpClient()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(_options.BaseUrl),
                Timeout = _options.Timeout
            };

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");
            client.DefaultRequestHeaders.Add("User-Agent", $"atoship-csharp-sdk/{GetType().Assembly.GetName().Version}");
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            return client;
        }

        /// <summary>
        /// Configure global default options
        /// </summary>
        public static void Configure(Action<AtoshipOptions> configure)
        {
            AtoshipOptions.SetDefaults(configure);
        }

        /// <summary>
        /// Test API connectivity
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Dispose of resources
        /// </summary>
        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    /// <summary>
    /// Interface for atoship client
    /// </summary>
    public interface IAtoshipClient : IDisposable
    {
        IOrdersService Orders { get; }
        IAddressesService Addresses { get; }
        IShippingService Shipping { get; }
        ITrackingService Tracking { get; }
        IUsersService Users { get; }
        ICarriersService Carriers { get; }
        IWebhooksService Webhooks { get; }
        IAdminService Admin { get; }
        Task<bool> TestConnectionAsync();
    }
}