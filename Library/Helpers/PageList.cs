using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Helpers
{
    public class PageList<T> : List<T>
    {
        public int CurrentPage { get; private set; }

        public int TotalPages { get; private set; }

        public int PageSize { get; private set; }

        public int TotalCount { get; private set; }

        public bool HasPrevious
        {
            get
            {
                return (CurrentPage < TotalPages);
            }
        }

        public bool HasNext
        {
            get
            {
                return (CurrentPage < TotalPages);
            }
        }

        public PageList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)PageSize);
        }

        public static PageList<T> Create (IQueryable<T> source, int pageNumber, int PageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageNumber - 1) * PageSize).Take(PageSize).ToList();
            return new PageList<T>(items, count, pageNumber, PageSize);
        }
    }
}
