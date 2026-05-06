using Microsoft.EntityFrameworkCore;

namespace PropertyLeasingMVC.ViewModels
{
    /// <summary>
    /// P3: minimal pagination wrapper used by every Index view.
    /// Carries the current page slice plus paging metadata so views can render
    /// "Previous / 1 2 3 / Next" without doing the math themselves.
    /// </summary>
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex   { get; }
        public int TotalPages  { get; }
        public int PageSize    { get; }
        public int TotalCount  { get; }
        public bool HasPrevious => PageIndex > 1;
        public bool HasNext     => PageIndex < TotalPages;

        public PaginatedList(IEnumerable<T> items, int totalCount, int pageIndex, int pageSize)
        {
            PageIndex  = pageIndex;
            PageSize   = pageSize;
            TotalCount = totalCount;
            TotalPages = totalCount == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize);
            AddRange(items);
        }

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize  < 1) pageSize  = 20;
            var total = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, total, pageIndex, pageSize);
        }
    }
}
