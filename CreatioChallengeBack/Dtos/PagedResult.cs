using System.Collections.Generic;

namespace CreatioChallengeBack.Dtos
{
    public class PagedResult<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public IEnumerable<T> Items { get; set; } = new List<T>();
    }
}
