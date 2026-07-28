using Modules.Organizations.Domain.Entities;

namespace Modules.Organizations.Domain.Repositories;

public interface IUserCommentRepository
{
	/// <summary>
	/// Adds a new comment to a ticket.
	/// </summary>
	/// <param name="comment">The <see cref="UserComment"/> entity containing the comment details.</param>
	/// <returns>The ID of the newly created comment.</returns>
	/// <exception cref="KeyNotFoundException">Thrown if the ticket is not found for the given ID.</exception>
	/// <exception cref="Exception">Thrown if adding the comment fails for any other reason.</exception>
	Task<int> AddCommentAsync(UserComment comment);
}

