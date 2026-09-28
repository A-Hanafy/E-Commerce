namespace ECommerce.API.Helpers;

public sealed class Pagination<T>
{
    public Pagination(int pageIndex, int pageSize, int count, IReadOnlyList<T> data)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        Count = count;
        Data = data;
    }

    public int PageIndex { get; }

    public int PageSize { get; }

    public int Count { get; }

    public IReadOnlyList<T> Data { get; }
}
