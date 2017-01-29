using System.Collections.Generic;

namespace TraktImporter
{
    public class OmdbSearchResult
    {
        public string Title { get; set; }
        public string Year { get; set; }
        public string imdbID { get; set; }
        public string Type { get; set; }
        public string Poster { get; set; }
    }

    public class OmdbResultRootObject
    {
        public List<OmdbSearchResult> Search { get; set; }
        public string totalResults { get; set; }
        public string Response { get; set; }
    }
}
