using Modules.Organizations.Domain.Entities;
using Modules.Organizations.Domain.Repositories;
using Modules.Organizations.Infrastructure.Database;

namespace Modules.Organizations.Infrastructure.Repositories;

public class UserCommentRepository : IUserCommentRepository
{
	private readonly OrganizationsDbContext _context;

	public UserCommentRepository(OrganizationsDbContext context)
	{
		_context = context ?? throw new ArgumentNullException(nameof(context));
	}

	public async Task<int> AddCommentAsync(UserComment comment)
	{
		_context.UserComments.Add(comment);
		await _context.SaveChangesAsync();
		return comment.Id;
	}
}
