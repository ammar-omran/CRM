using System.Security.Claims;

namespace CRM.SharedKernel.Infrastructure.Services;

public interface IJwtTokenGenerator
{
    string GenerateToken(string userId, string email, IEnumerable<Claim>? additionalClaims = null);
    string GenerateRefreshToken();
}
