using Microsoft.AspNetCore.Http;
using CRM.SharedKernel.Domain.Results;
using CRM.SharedKernel.Application.API.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CRM.SharedKernel.Application.API.Extensions;

public static class EndpointResultsExtensions
{
	public static Microsoft.AspNetCore.Http.IResult ToProblem(this List<Error> errors)
	{
		if (errors.Count is 0)
		{
			return Results.Problem();
		}

		return CreateProblem(errors);
	}

	private static Microsoft.AspNetCore.Http.IResult CreateProblem(List<Error> errors)
	{
		var statusCode = errors.First().Type switch
		{
			ErrorType.Conflict => StatusCodes.Status409Conflict,
			ErrorType.Validation => StatusCodes.Status400BadRequest,
			ErrorType.NotFound => StatusCodes.Status404NotFound,
			ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
			ErrorType.Forbidden => StatusCodes.Status403Forbidden,

			ErrorType.Failure => StatusCodes.Status400BadRequest,
			ErrorType.Unexpected => StatusCodes.Status400BadRequest,
			ErrorType.Custom => StatusCodes.Status400BadRequest,

			_ => StatusCodes.Status500InternalServerError
		};

		var firstError = errors.First();

		var problemResponse = new BaseResponse
		{
			Status = new ResponseStatus
			{
				Code = firstError.NumericType,
				Message = firstError.Description,
			},
			Errors = errors.ToDictionary(e => e.Code, e => e.Description)
		};

		return Results.Json(data: problemResponse, statusCode: statusCode);
	}

	public static TOut Match<TOut>(
		this Domain.Results.IResult result,
		Func<TOut> onSuccess,
		Func<Domain.Results.IResult, TOut> onFailure)
	{
		return result.IsSuccess ? onSuccess() : onFailure(result);
	}

	public static TOut Match<TIn, TOut>(
		this Result<TIn> result,
		Func<TIn, TOut> onSuccess,
		Func<Result<TIn>, TOut> onFailure)
	{
		return result.IsSuccess ? onSuccess(result.Value) : onFailure(result);
	}

	public static IActionResult ToMVCProblem(this List<Error> errors)
	{
		var firstError = errors.First();

		var problemResponse = new BaseResponse
		{
			Status = new ResponseStatus
			{
				Code = firstError.NumericType,
				Message = firstError.Description,
			},
			Errors = errors.ToDictionary(e => e.Code, e => e.Description)
		};

		var statusCode = firstError.Type switch
		{
			ErrorType.Conflict => StatusCodes.Status409Conflict,
			ErrorType.Validation => StatusCodes.Status400BadRequest,
			ErrorType.NotFound => StatusCodes.Status404NotFound,
			ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
			ErrorType.Forbidden => StatusCodes.Status403Forbidden,

			ErrorType.Failure => StatusCodes.Status400BadRequest,
			ErrorType.Unexpected => StatusCodes.Status500InternalServerError,
			ErrorType.Custom => StatusCodes.Status400BadRequest,

			_ => StatusCodes.Status500InternalServerError
		};

		return new ObjectResult(problemResponse)
		{
			StatusCode = statusCode
		};
	}
}
