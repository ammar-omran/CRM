using CRM.SharedKernel.Application.API.Extensions;
using CRM.SharedKernel.Application.API.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Customers.Domain.Authentication;
using Modules.Customers.Domain.Policies;
using Modules.Customers.Features.Customers.ConfirmEmail;
using Modules.Customers.Features.Customers.GetAllCustomers;
using Modules.Customers.Features.Customers.GetCustomerById;
using Modules.Customers.Features.Customers.Login;
using Modules.Customers.Features.Customers.RefreshToken;
using Modules.Customers.Features.Customers.Register;
using Modules.Customers.Features.Customers.ResendOtp;
using Modules.Customers.Features.Customers.Shared;

namespace Modules.Customers.API.Controllers;

/// <summary>
/// Customer identity and portal endpoints.
/// </summary>
[ApiController]
[Route("api/customers")]
[Produces("application/json")]
public sealed class CustomersController : ControllerBase
{
    /// <summary>
    /// Register a new customer and send OTP for email verification.
    /// </summary>
    /// <remarks>Anonymous. Creates inactive customer, hashes password via Identity, generates OTP and hashed email, sends verification email with masked email returned.</remarks>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        [FromServices] IValidator<RegisterRequest> validator,
        [FromServices] IRegisterHandler handler,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return BadRequest(validation.ToDictionary());

        var result = await handler.HandleAsync(request, cancellationToken);
        return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
    }

    /// <summary>
    /// Confirm email verification via OTP.
    /// </summary>
    /// <remarks>Anonymous. Validates OTP, checks expiry and lockout, activates customer and assigns default Customer role.</remarks>
    [HttpPost("confirm-email")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ConfirmEmail(
        [FromBody] ConfirmEmailRequest request,
        [FromServices] IValidator<ConfirmEmailRequest> validator,
        [FromServices] IConfirmEmailHandler handler,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return BadRequest(validation.ToDictionary());

        var result = await handler.HandleAsync(request, cancellationToken);
        return result.IsError ? result.Errors.ToMVCProblem() : NoContent();
    }

    /// <summary>
    /// Resend OTP for email verification.
    /// </summary>
    [HttpPost("resend-otp")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ResendOtp(
        [FromBody] ResendOtpRequest request,
        [FromServices] IValidator<ResendOtpRequest> validator,
        [FromServices] IResendOtpHandler handler,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return BadRequest(validation.ToDictionary());

        var result = await handler.HandleAsync(request, cancellationToken);
        return result.IsError ? result.Errors.ToMVCProblem() : NoContent();
    }

    /// <summary>
    /// Authenticate customer with email and password.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        [FromServices] IValidator<LoginRequest> validator,
        [FromServices] ILoginHandler handler,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return BadRequest(validation.ToDictionary());

        var result = await handler.HandleAsync(request, cancellationToken);
        return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
    }

    /// <summary>
    /// Refresh customer JWT using refresh token.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CustomerRefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status409Conflict)]
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
    /// Get paginated list of customers.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = CustomerPolicyConstants.ViewPolicy)]
    [ProducesResponseType(typeof(PaginationResponse<CustomerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetAllCustomersRequest request,
        [FromServices] IValidator<GetAllCustomersRequest> validator,
        [FromServices] IGetAllCustomersHandler handler,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return BadRequest(validation.ToDictionary());

        var result = await handler.HandleAsync(request, cancellationToken);
        return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
    }

    /// <summary>
    /// Get customer by identifier.
    /// </summary>
    [HttpGet("{customerId}")]
    [Authorize(Policy = CustomerPolicyConstants.ViewPolicy)]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        string customerId,
        [FromServices] IGetCustomerByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(customerId, cancellationToken);
        return result.IsError ? result.Errors.ToMVCProblem() : Ok(result.Value);
    }
}
