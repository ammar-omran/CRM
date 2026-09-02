using CRM.SharedKernel.Domain.Results;

namespace Modules.Users.Domain.Authentication;

public interface IClientAuthorizationService
{
	/// <summary>
	/// Updates a user's role and invalidates their refresh tokens
	/// </summary>
	/// <param name="userId">The ID of the user to update</param>
	/// <param name="newRole">The new role to assign to the user</param>
	/// <param name="cancellationToken">A cancellation token</param>
	/// <returns>A result containing success or failure information</returns>
	Task<Result<Success>> UpdateUserRoleAsync(string userId, string newRole, CancellationToken cancellationToken);
	Task<Result<LoginUserResponse>> LoginAsync(string email, string password, CancellationToken cancellationToken);
	Task<Result<RefreshTokenResponse>> RefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken);

	/// <summary>
	/// Sets the current organization for a user. Validates the user belongs to the organization via
	/// Agent -> OrganizationAgent, stores OrganizationId as a UserClaim (ClaimType = 'OrganizationId')
	/// and syncs the user's sub-role (FirstLine/SecondLine/TeamLead) to the role assigned in that
	/// organization (OrganizationAgent.AgentRoleId) in the UserRole table.
	/// </summary>
	Task<Result<Success>> SetCurrentOrganizationAsync(string userId, int organizationId, CancellationToken cancellationToken);
}
