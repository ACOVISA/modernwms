using ModernWMS.Core.DI;

namespace ModernWMS.WMS.IServices
{
    /// <summary>
    /// Interface of FleetbaseIntegrationService
    /// </summary>
    public interface IFleetbaseIntegrationService : IDependency
    {
        /// <summary>
        /// Creates a delivery order in Fleetbase for a dispatch that just left the warehouse.
        /// No-ops silently when the integration is not configured (Fleetbase:ApiUrl / Fleetbase:ApiKey).
        /// </summary>
        Task CreateDeliveryOrderAsync(string dispatchNo, string customerName, string dropoffAddress, decimal weight, decimal volume, string carrier);
    }
}
