using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CRM.SharedKernel.Application.API.Extensions;
using CRM.SharedKernel.Application.API.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Users.Domain.Authentication;
using Modules.Users.Domain.Policies;
using Modules.Users.Features.Organizations.SetCurrentOrganization;
using Modules.Users.Features.Users.DeleteUser;
using Modules.Users.Features.Users.GetUserById;
using Modules.Users.Features.Users.LoginUser;
using Modules.Users.Features.Users.RefreshToken;
using Modules.Users.Features.Users.RegisterUser;
using Modules.Users.Features.Users.SetPassword;
using Modules.Users.Features.Users.Shared;

namespace Modules.Users.API.Controllers;

/// <summary>
/// Workforce identity and authentication endpoints.
/// </summary>
[ApiController]
[Route("api/users")]
[Produces("application/json")]
public sealed class UsersController : ControllerBase
{
    /// <summary>
    /// Authenticate a user and obtain a JWT + refresh token.
    /// </summary>
    /// <remarks>Anonymous. Validates credentials and returns tokens. Subsequent requests require Bearer token.</remarks>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginUserRequest request,
        [FromServices] IValidator<LoginUserRequest> validator,
        [FromServices] ILoginUserHandler handler,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return BadRequest(validation.ToDictionary());

        var result = await handler.HandleAsync(request, cancellationToken);
        return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
    }

    /// <summary>
    /// Register a new workforce user.
    /// </summary>
    /// <remarks>Anonymous. Creates an inactive user; password setup is required via SetPassword.</remarks>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserRequest request,
        [FromServices] IValidator<RegisterUserRequest> validator,
        [FromServices] IRegisterUserHandler handler,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return BadRequest(validation.ToDictionary());

        var result = await handler.HandleAsync(request, cancellationToken);
        return result.IsError ? result.Errors.ToMVCProblem() : Created($"/api/users/{result.Value?.Id}", result.Value);
    }

    /// <summary>
    /// Refresh an expired JWT using a valid refresh token.
    /// </summary>
    /// <remarks>Anonymous. Requires both the expired JWT and the refresh token.</remarks>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        [FromServices] IValidator<RefreshTokenRequest> validator,
        [FromServices] IRefreshTokenHandler handler,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return BadRequest(validation.ToDictionary());

        var result = await handler.HandleAsync(request, cancellationToken);
        return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
    }

    /// <summary>
    /// Complete first-time password setup and activate the account.
    /// </summary>
    /// <remarks>Anonymous. Token is issued by password-reset generator and carries user id + expiry.</remarks>
    [HttpPost("set-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SetPassword(
        [FromBody] SetPasswordRequest request,
        [FromServices] IValidator<SetPasswordRequest> validator,
        [FromServices] ISetPasswordHandler handler,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return BadRequest(validation.ToDictionary());

        var result = await handler.HandleAsync(request, cancellationToken);
        return result.IsError ? result.Errors.ToMVCProblem() : NoContent();
    }

    /// <summary>
    /// Get a user by identifier.
    /// </summary>
    [HttpGet("{userId}")]
    [Authorize(Policy = UserPolicyConstants.ReadPolicy)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        string userId,
        [FromServices] IGetUserByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(userId, cancellationToken);
        return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
    }

    /// <summary>
    /// Delete a workforce user.
    /// </summary>
    [HttpDelete("{userId}")]
    [Authorize(Policy = UserPolicyConstants.DeletePolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        string userId,
        [FromServices] IDeleteUserHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(userId, cancellationToken);
        return result.IsError ? result.Errors.ToMVCProblem() : NoContent();
    }

    /// <summary>
    /// Set the current organization for the authenticated user.
    /// </summary>
    /// <remarks>Stores OrganizationId as UserClaim and syncs the agent sub-role (FirstLine/SecondLine/TeamLead) to UserRoles. Requires membership in the organization.</remarks>
    [HttpPut("current-organization")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetCurrentOrganization(
        [FromBody] SetCurrentOrganizationRequest request,
        [FromServices] IValidator<SetCurrentOrganizationRequest> validator,
        [FromServices] ISetCurrentOrganizationHandler handler,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return BadRequest(validation.ToDictionary());

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                     ?? User.FindFirstValue("userid")
                     ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var result = await handler.HandleAsync(userId, request, cancellationToken);
        return result.IsError ? result.Errors.ToMVCProblem() : NoContent();
    }
}
