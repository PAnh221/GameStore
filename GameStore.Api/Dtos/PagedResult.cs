namespace GameStore.Api.Dtos;

public record class PagedResult<T>(int Page, int PageSize, int TotalPages, int TotalCount, IEnumerable<T> Items);