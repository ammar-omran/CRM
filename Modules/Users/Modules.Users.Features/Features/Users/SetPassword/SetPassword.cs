using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CRM.SharedKernel.Application.API.Abstractions;
using CRM.SharedKernel.Application.API.Extensions;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using Modules.Users.Domain.Authentication;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.UserAggregate;
using Modules.Users.Features.Users.Shared.Routes;
using Modules.Users.Features.Validators;

namespace Modules.Users.Features.Users.SetPassword;

/// <summary>
/// Completes first-time password setup / account activation. The <see cref="Token"/>
/// is issued by <see cref="IPasswordResetTokenGenerator"/>
/// and carries the target user id + expiry.
/// </summary>
public sealed record SetPasswordRequest(string Token, string Password);



internal interface ISetPasswordHandler : IHandler
{
	Task<Result<Success>> HandleAsync(SetPasswordRequest request, CancellationToken cancellationToken);
}

internal sealed class SetPasswordHandler(
		UserManager<User> userManager,
		IPasswordResetTokenGenerator passwordResetTokenGenerator,
		ILogger<SetPasswordHandler> logger)
		: ISetPasswordHandler
{
	public async Task<Result<Success>> HandleAsync(
			SetPasswordRequest request,
			CancellationToken cancellationToken)
	{
		var passwordValidator = new PasswordValidator();

		// 1. Validate token signature/issuer/audience and decode its payload.
		var payload = passwordResetTokenGenerator.ValidateToken(request.Token);
		if (payload is null)
		{
			return UserErrors.InvalidOrExpiredLink();
		}

		// 2. Load the target user.
		var user = await userManager.FindByIdAsync(payload.Value.UserId);
		if (user is null)
		{
			return UserErrors.NotFound(payload.Value.UserId);
		}

		// 3. Is-active guard: only an inactive (pending activation) account may set its password.
		if (user.IsActive)
		{
			return UserErrors.UserAlreadyActive(user.Email!);
		}

		// 4. Expiry guard.
		if (payload.Value.Expiry < DateTime.UtcNow)
		{
			return UserErrors.TokenExpired();
		}

		// 5. Strong-password guard.
		var passwordValidation = await passwordValidator.ValidateAsync(request.Password, cancellationToken);
		if (!passwordValidation.IsValid)
		{
			return UserErrors.WeakPassword(
					string.Join(" | ", passwordValidation.Errors.Select(e => e.ErrorMessage)));
		}

		// 6. Persist the new password hash and activate the account.
		user.PasswordHash = userManager.PasswordHasher.HashPassword(user, request.Password);
		user.IsActive = true;

		var result = await userManager.UpdateAsync(user);
		if (!result.Succeeded)
		{
			logger.LogError(
					"Failed to set password for user {UserId}: {@Errors}",
					user.Id, result.Errors);
			return UserErrors.UpdateFailed(result.Errors);
		}

		logger.LogInformation(
				"User {UserId} ({Email}) set password and activated account",
				user.Id, user.Email);

		return Result.Success;
	}
}


public class SetPasswordRequestValidator : AbstractValidator<SetPasswordRequest>
{
	public SetPasswordRequestValidator()
	{
		RuleFor(x => x.Token)
				.NotEmpty().WithMessage("Token is required");

		RuleFor(x => x.Password)
				.NotEmpty().WithMessage("Password is required");
	}
}


public class SetPasswordEndpoint : IApiEndpoint
{
	public void MapEndpoint(WebApplication app)
	{
		app.MapPost(RouteConsts.SetPassword, Handle);
	}

	private static async Task<Microsoft.AspNetCore.Http.IResult> Handle(
			[FromBody] SetPasswordRequest request,
			IValidator<SetPasswordRequest> validator,
			ISetPasswordHandler handler,
			CancellationToken cancellationToken)
	{
		var validationResult = await validator.ValidateAsync(request, cancellationToken);
		if (!validationResult.IsValid)
		{
			return Results.ValidationProblem(validationResult.ToDictionary());
		}

		var response = await handler.HandleAsync(request, cancellationToken);
		if (response.IsError)
		{
			return response.Errors.ToProblem();
		}

		return Results.NoContent();
	}
}
