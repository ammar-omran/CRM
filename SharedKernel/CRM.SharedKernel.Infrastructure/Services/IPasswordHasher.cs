namespace CRM.SharedKernel.Infrastructure.Services;

public interface IPasswordHasher
{
	string Hash(string password);
	bool Verify(string password, string hashedPassword);
}
