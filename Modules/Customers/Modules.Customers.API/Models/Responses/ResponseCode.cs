namespace Modules.Customers.API.Models.Responses;

/// <summary>
/// An enumeration representing standardized response codes for Current API project operations,
/// facilitating consistent error handling and status reporting across the application.
/// </summary>
public enum ResponseCode
{
    Success = 0,
    GeneralError = 1,
    NoDataFound = 2,
    Conflict = 4,
}

public static class StatusMapper
{
    public static ResponseCode ToResponseStatus(this string errorCode)
    {
        // Directly parse by name — assumes naming conventions match
        if (Enum.TryParse<ResponseCode>(errorCode, out var responseStatus))
        {
            return responseStatus;
        }

        throw new ArgumentException(
            $"No matching {nameof(ResponseCode)} found for {errorCode}",
            nameof(errorCode)
        );
    }
    public static int ToInt(this ResponseCode status) => (int)status;
}
