namespace CRM.SharedKernel.API.Responses;

public class BaseResponse<T> : BaseResponse
{
	public T Data { get; set; }
}

public class BaseResponse
{
	public ResponseStatus Status { get; set; } = default!;
	public Dictionary<string, string> Errors { get; set; } = [];
}

public class ResponseStatus
{
	public int Code { get; set; }
	public string Message { get; set; }

	public ResponseStatus(int code = 1, string message = "Unexpected Error Occurred!")
	{
		Code = code;
		Message = message;
	}

	public ResponseStatus(bool status, string message = "Unexpected Error Occurred!")
	{
		Code = status ? 0 : 1;
		Message = message;
	}
}
