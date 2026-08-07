namespace CRM.SharedKernel.Application.API.Responses;

public class PaginationResponse<T>(List<T> items, int page, int pageSize, int totalItems = -1)
{
	public List<T> Items { get; set; } = items;
	public int Page { get; set; } = page;
	public int TotalItemsCount { get; set; } = totalItems;
	public int PageSize { get; set; } = pageSize;
}
