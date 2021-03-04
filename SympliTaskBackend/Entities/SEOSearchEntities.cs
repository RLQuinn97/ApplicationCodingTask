using System;
using System.Net;
namespace SympliTaskBackend.Entities
{
    public class SEOSearchEntity
    {
        public SearchEngineType EngineType { get; set; }
        public string EngineName { get; set; }
        public DateTime SearchDate { get; set; }
        public string SearchKeywords { get; set; }
        public string SearchURL { get; set; }
        public int ResultCount { get; set; }
        public int MatchCount { get; set; }
        public int? HighestRanking { get; set; }
        public string Rankings { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }

    public enum SearchEngineType
    {
        Unassigned,
        Google,
        Bing,
        Yahoo,
    }

    public class SearchEngineMapper
    {
        public SearchEngineType Engine { get; set; }
        public string BaseUrl { get; set; }
        public string SearchTag { get; set; }
    }
}
