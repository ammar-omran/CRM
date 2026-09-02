namespace Modules.Users.Domain.Authentication;

/// <summary>
/// Generates and validates stateless password-reset / account-activation tokens.
/// The token encodes the target UserId + Expiry and is self-contained, so no
/// database storage is required to validate it.
/// </summary>
public interface IPasswordResetTokenGenerator
{
    /// <summary>Creates a signed token carrying the <paramref name="userId"/> and <paramref name="expires"/>.</summary>
    string GenerateToken(string userId, DateTime expires);

    /// <summary>
    /// Validates the token signature/issuer/audience and returns its payload, or
    /// <c>null</c> when the token is malformed, tampered, or not signed by us.
    /// Lifetime is intentionally NOT enforced here so callers can distinguish an
    /// expired token from an invalid one.
    /// </summary>
    (string UserId, DateTime Expiry)? ValidateToken(string token);
}
