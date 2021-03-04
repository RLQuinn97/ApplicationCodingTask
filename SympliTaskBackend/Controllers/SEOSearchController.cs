using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SympliTaskBackend.Entities;
using System.Net;
using System.IO;
using System.Web;
using System.Text;
using System.Windows.Forms;

namespace SympliTaskBackend.Controllers
{
    [ApiController]
    [Route("api/getSEOResult")]
    public class SEOSearchController : ControllerBase
    {

        const string ResultCountPlaceholder = "{{numResults}}";
        const string SearchTextPlaceholder = "{{formattedSearch}}";

        ILogger<SEOSearchController> _logger;
        public SEOSearchController()
        {
            
        }
        public SEOSearchController(ILogger<SEOSearchController> logger)
        {
            _logger = logger;
        }

        //this could be defined serverside so that engines with basic mapping could be added on the fly - for this version I'm adding it here
        public List<SearchEngineMapper> EngineMappers = new List<SearchEngineMapper>()
        {
            new SearchEngineMapper(){ Engine = SearchEngineType.Google, BaseUrl = "https://www.google.com/search?&num={{numResults}}&q={{formattedSearch}}", SearchTag = "BNeawe UPmit AP7Wnd" },
            new SearchEngineMapper(){ Engine = SearchEngineType.Yahoo, BaseUrl = "https://search.yahoo.com/search?p={{formattedSearch}}&n={{numResults}}", SearchTag = "fz-ms fw-m fc-12th wr-bw lh-17" },
            new SearchEngineMapper(){ Engine = SearchEngineType.Bing, BaseUrl = "http://www.bing.com/search?q={{formattedSearch}}&count={{numResults}}", SearchTag = "b_attribution" }
        };


        /// <summary>
        /// Gets SEO results for the following required headers:
        /// <param name="searchString"> STRING: The text to execute the search for - i.e. e-Settlements </param>
        /// <param name="targetUrl"> STRING: The URL to check matches for - i.e. sympli.com.au </param>
        /// <param name="resultsCount"> INTEGER: Defines the the top x results to check</param>
        /// <param name="engineTypeId"> INTEGER: From user selection, corresponds with SearchEngineType in enums</param>
        /// </summary>
        /// <returns>SEOSearchEntity with success true if a search has been made and checked, false with error message if not.</returns>
        public SEOSearchEntity Get()
        {
            var headers = Request?.Headers;
            if (headers != null && headers.Any() && headers.Count() == 4)
            {
                string searchString = "";
                string targetUrl = "";
                int resultsCount = 0;
                SearchEngineType engineType = SearchEngineType.Unassigned;
                try
                {
                    searchString = headers["searchString"].FirstOrDefault();
                    targetUrl = headers["targetUrl"].FirstOrDefault();
                    resultsCount = Int32.Parse(headers["resultsCount"].FirstOrDefault());
                    engineType = (SearchEngineType)Int32.Parse(headers["engineTypeId"].FirstOrDefault());
                }
                catch(Exception e)
                {
                    return new SEOSearchEntity() { EngineName = "UNSPECIFIED", Success = false, ErrorMessage = $"Missing or incorrectly formatted headers - {e.Message}" };
                }

                //check if we have a mapping engine set up for this engine type (should always be the case since the selection will be a drop down box of the ones we have defined)
                SearchEngineMapper mapper = EngineMappers.FirstOrDefault(t => t.Engine == engineType);
                if (mapper != null)
                {
                    return GetSearchResults(mapper, searchString, targetUrl, resultsCount);
                }
                else
                {
                    return new SEOSearchEntity() { EngineName = Enum.GetName(typeof(SearchEngineType), engineType), Success = false, ErrorMessage = "No Mapping for Search Engine Found" };

                }
            }
            else
            {
                return new SEOSearchEntity() { EngineName = "UNSPECIFIED", Success = false, ErrorMessage = "Header Data missing from request" };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public SEOSearchEntity GetSearchResults(SearchEngineMapper mapper, string searchString, string targetUrl, int resultsToCheck)
        {
            var result = new SEOSearchEntity() { EngineName = Enum.GetName(typeof(SearchEngineType), mapper.Engine) };

           
            string searchUrl = mapper.BaseUrl.Replace(ResultCountPlaceholder, resultsToCheck.ToString())
                .Replace(SearchTextPlaceholder, WebUtility.UrlEncode(searchString.ToString()));
            
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(searchUrl);

            string htmlResponse = "";
            HttpWebResponse response = new HttpWebResponse();

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                response = (HttpWebResponse)e.Response;
                if (response == null || response.StatusCode != HttpStatusCode.Redirect) //the page has redirected us, but we can still use the result.
                {
                    throw e;
                }
            }
               
            if(response != null)
            {

                result.SearchDate = DateTime.UtcNow;
                result.SearchKeywords = searchString;
                result.SearchURL = targetUrl;

                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.ASCII))
                {
                    htmlResponse = reader.ReadToEnd();
                }
            }

            //Typically I would use the HTML Agility pack to read the HTML response as a class but the task spec requested no 3rd party libraries, implementing a basic tag parser looking for the html elements I'm after.
            //A performance improvement here would be proper Regex matching
            if (!string.IsNullOrEmpty(htmlResponse) && htmlResponse.Contains("<body"))
            {
                htmlResponse = htmlResponse.Substring(htmlResponse.IndexOf("<body"));
            }
            else
            {
                throw new WebException("Search Results could not be loaded - no HTML body found", WebExceptionStatus.UnknownError);
            }
            
            if (htmlResponse?.Contains(mapper.SearchTag) == true)
            {
                List<string> urls = new List<string>();
                
                //Searches for elements with the search tag in their definition. An improvement would be to specify particular class types, or particular IDs, but this was the quick way I chose to implement for this version.
                //Extensions to search for these specific parts of the tag would be fairly trivial to implement.
                foreach (var indexOfUrl in GetAllIndexesOf(htmlResponse, mapper.SearchTag))
                {
                    var startEl = htmlResponse.IndexOf(">", indexOfUrl);
                    var endEl = htmlResponse.IndexOf("<", startEl);
                    if (startEl != -1 && endEl != -1)
                    {
                        urls.Add(htmlResponse.Substring(startEl + 1, (endEl - startEl - 1) )?.Trim());
                    }
                }

                urls = urls.Where(u => !String.IsNullOrEmpty(u)).ToList();


                //todo: Regex to improve cleaning of user input of url to search for
                targetUrl = targetUrl.ToLower().Replace("http://", "").Replace("https://", "").Replace("www.", "");

                result.ResultCount = urls.Count;

                List<int> matchRankings = new List<int>();

                for (int i = 0; i < result.ResultCount; i++)
                {
                    if(urls[i].ToLower().Contains(targetUrl))
                    {
                        if (!result.HighestRanking.HasValue)
                        {
                            result.HighestRanking = i+1;
                        }
                        matchRankings.Add(i + 1);
                    }
                }
                if (!matchRankings.Any())
                {
                    matchRankings.Add(0);
                }

                result.Rankings = String.Join(", ", matchRankings);
                result.MatchCount = matchRankings.Count();
                result.Success = true;

            }
            if(result.ResultCount == 0)
            {
                result.ErrorMessage = $"Found no instances of search tag: {mapper.SearchTag}";
            }
            return result;
        }
        
        //gets all indexes of a particular substring within a parent string
        private static List<int> GetAllIndexesOf( string str, string value)
        {
        
            List<int> indexes = new List<int>();
            if (string.IsNullOrEmpty(str))
            {
                return indexes;
            }

            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                    return indexes;
                indexes.Add(index);
            }
        }
     
    }
}
