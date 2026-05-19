using System.Collections.Generic;

namespace TruequeU.Models;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)System.Math.Ceiling(TotalCount / (double)System.Math.Max(1, PageSize));
}
