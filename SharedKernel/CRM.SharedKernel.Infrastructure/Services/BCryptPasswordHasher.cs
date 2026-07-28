namespace CRM.SharedKernel.Infrastructure.Services;

public class BCryptPasswordHasher : IPasswordHasher
{
	public string Hash(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			throw new ArgumentException("Value cannot be null or empty.", nameof(value));
		}
		// Use BCrypt to hash the password
		return BCrypt.Net.BCrypt.HashPassword(value);
	}

	public bool Verify(string password, string hashedPassword)
	{
		return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
	}
}
