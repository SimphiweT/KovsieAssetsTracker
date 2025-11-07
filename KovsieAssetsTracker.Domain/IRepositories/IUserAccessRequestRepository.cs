using KovsieAssetsTracker.Domain.Entities;
using KovsieAssetsTracker.Domain.IRepositories.Base;

namespace KovsieAssetsTracker.Domain.IRepositories
{
    /// <summary>
    /// Repository contract for managing user access requests.
    /// </summary>
    public interface IUserAccessRequestRepository : IRepositoryBase<UserAccessRequest>
    {
        /// <summary>Retrieves all pending access requests (not approved or declined).</summary>
        Task<IEnumerable<UserAccessRequest>> GetPendingRequestsAsync();

        /// <summary>Retrieves all reviewed access requests (approved or declined).</summary>
        Task<IEnumerable<UserAccessRequest>> GetReviewedRequestsAsync();
    }
}
