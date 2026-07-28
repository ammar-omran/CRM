namespace CRM.SharedKernel.API.Requests;

public class PaginationRequest
{
	private int limit = 30;
	private int skip;

	public int Limit
	{
		get => limit;
		set => limit = value < 1 ? 1 : value;
	}

	public int Skip
	{
		get => skip;
		set => skip = value < 0 ? 0 : value;
	}

	/// <summary>
	/// If it is set the total counts query will be skipped,
	/// and the response fields totalItems and totalPages will have -1 value.
	/// This could drastically speed up the search queries when the total counters
	/// are not needed or cursor based pagination is used.
	/// </summary>
	public bool SkipTotal { get; set; }
}
