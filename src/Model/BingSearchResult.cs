using System.Collections.Generic;

namespace Model
{
    public class BingSearchResult
    {
        public WebPages WebPages { get; set; }
    }

    public class WebPages
    {
        public List<Value> Value { get; set; }
    }

    public class Value
    {
        public string Name { get; set; }
    }
}