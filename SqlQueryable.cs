
using System.Collections.Generic;

namespace SqlFramework
{
    public class SqlQueryable<T>
    {
        public string OrderBy { get; set; }

        public string Where { get; set; }

        public int Skip { get; set; }

        public int PageSize { get; set; }

        public List<string> Include { get; set; }
    }
}
