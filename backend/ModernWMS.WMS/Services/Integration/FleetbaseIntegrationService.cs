using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModernWMS.WMS.IServices;

namespace ModernWMS.WMS.Services.Integration
{
    /// <summary>
    /// Fires a delivery order over to Fleetbase whenever a dispatch is confirmed out of the warehouse.
    /// Configured via Fleetbase:ApiUrl / Fleetbase:ApiKey / Fleetbase:PickupAddress (appsettings or env vars);
    /// left blank, the integration is a no-op so the WMS keeps working standalone.
    /// </summary>
    public class FleetbaseIntegrationService : IFleetbaseIntegrationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FleetbaseIntegrationService> _logger;

        public FleetbaseIntegrationService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<FleetbaseIntegrationService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task CreateDeliveryOrderAsync(string dispatchNo, string customerName, string dropoffAddress, decimal weight, decimal volume, string carrier)
        {
            var baseUrl = _configuration["Fleetbase:ApiUrl"];
            var apiKey = _configuration["Fleetbase:ApiKey"];

            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
            {
                return;
            }

            var pickupAddress = _configuration["Fleetbase:PickupAddress"];
            if (string.IsNullOrWhiteSpace(pickupAddress))
            {
                pickupAddress = "Acovisa Warehouse";
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                var payload = new
                {
                    pickup = new { address = pickupAddress },
                    dropoff = new { address = string.IsNullOrWhiteSpace(dropoffAddress) ? customerName : dropoffAddress },
                    meta = new
                    {
                        dispatch_no = dispatchNo,
                        customer_name = customerName,
                        weight,
                        volume,
                        carrier,
                        source = "ModernWMS",
                    },
                };

                var response = await client.PostAsJsonAsync("/v1/orders", payload);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Fleetbase order creation failed for dispatch {DispatchNo}: {Status} {Body}", dispatchNo, response.StatusCode, body);
                }
                else
                {
                    _logger.LogInformation("Fleetbase order created for dispatch {DispatchNo}", dispatchNo);
                }
            }
            catch (Exception ex)
            {
                // Never let a Fleetbase outage block a warehouse dispatch.
                _logger.LogError(ex, "Error creating Fleetbase order for dispatch {DispatchNo}", dispatchNo);
            }
        }
    }
}
