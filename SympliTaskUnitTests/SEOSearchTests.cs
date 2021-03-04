using Microsoft.VisualStudio.TestTools.UnitTesting;
using SympliTaskBackend.Controllers;
using SympliTaskBackend.Entities;

namespace SympliTaskUnitTests
{
    [TestClass]
    public class SEOSearchTests
    {
        [TestMethod]
        //Should fail as no headers in API request
        public void TestNoHeaders()
        {
            var ctl = new SympliTaskBackend.Controllers.SEOSearchController();
            var response = ctl.Get();
            Assert.IsTrue(!response.Success);
        }
        [TestMethod]
        //Search google for "e-Settlements" and return the number of matches pointing to sympli.com.au in the first 100 results.
        public void TestGoogleSearchSuccess()
        {

            var mockContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
    
            mockContext.Request.Headers.Add("searchString", "e-Settlements");
            mockContext.Request.Headers.Add("targetUrl", "www.sympli.com.au");
            mockContext.Request.Headers.Add("resultsCount", "100");
            mockContext.Request.Headers.Add("engineTypeId", $"{(int)SearchEngineType.Google}");

            var ctl = new SEOSearchController() { ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext() { HttpContext = mockContext  } };
            
            var response = ctl.Get();

            Assert.IsTrue(response.Success);
        }
        [TestMethod]
        //Search google for "e-Settlements" and return the number of matches pointing to sympli.com.au in the first 100 results, cache the result, search again 1 second later to obtain from cache instead
        public void TestGoogleCachedResult()
        {

            var mockContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();

            mockContext.Request.Headers.Add("searchString", "e-Settlements");
            mockContext.Request.Headers.Add("targetUrl", "www.sympli.com.au");
            mockContext.Request.Headers.Add("resultsCount", "100");
            mockContext.Request.Headers.Add("engineTypeId", $"{(int)SearchEngineType.Google}");

            var ctl = new SEOSearchController() { ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext() { HttpContext = mockContext } };

            var response = ctl.Get();
            System.Threading.Thread.Sleep(1000);
            var secondResponse = ctl.Get();

            Assert.IsTrue(response.Success && secondResponse.Success && secondResponse.SearchDate == response.SearchDate);
        }
        [TestMethod]
        //Tests two separate google results, ensuring that incorrect data isn't pulled from the cache
        public void TestSeparateGoogleResults()
        {

            var mockContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();

            mockContext.Request.Headers.Add("searchString", "e-Settlements");
            mockContext.Request.Headers.Add("targetUrl", "www.sympli.com.au");
            mockContext.Request.Headers.Add("resultsCount", "100");
            mockContext.Request.Headers.Add("engineTypeId", $"{(int)SearchEngineType.Google}");

            var ctl = new SEOSearchController() { ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext() { HttpContext = mockContext } };

            var response = ctl.Get();
            System.Threading.Thread.Sleep(1000);

            var mockContext2 = new Microsoft.AspNetCore.Http.DefaultHttpContext();

            mockContext2.Request.Headers.Add("searchString", "Digital Settlements");
            mockContext2.Request.Headers.Add("targetUrl", "www.sympli.com.au");
            mockContext2.Request.Headers.Add("resultsCount", "50");
            mockContext2.Request.Headers.Add("engineTypeId", $"{(int)SearchEngineType.Google}");

            ctl.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext() { HttpContext = mockContext2 };

            var secondResponse = ctl.Get();

            Assert.IsTrue(response.Success && secondResponse.Success && secondResponse.SearchDate != response.SearchDate);
        }

        //Attempts to search Bing - this functionality works on the fully constructed HTML body for a bing result - but not on the HttpWebResponse obtained from simply calling the correct search url.
        //Javascript is not executing. I would typically use Selenium / PhantomJS to execute DOM functions to get the full results, but the task spec required no external packages.
        [TestMethod]
        public void TestBingSearchSuccess()
        {

            var mockContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();

            mockContext.Request.Headers.Add("searchString", "e-Settlements");
            mockContext.Request.Headers.Add("targetUrl", "www.sympli.com.au");
            mockContext.Request.Headers.Add("resultsCount", "100");
            mockContext.Request.Headers.Add("engineTypeId", $"{(int)SearchEngineType.Bing}");

            var ctl = new SympliTaskBackend.Controllers.SEOSearchController() { ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext() { HttpContext = mockContext } };

            var response = ctl.Get();

            Assert.IsTrue(response.Success);
        }

        //Attempts to search Yahoo - I think I got myself banned from Yahoo search on my network from running this as since 
        //I ran it the first time with an incorrect parameter in my url, I can no longer load any Yahoo search results from my network - getting a 500 error each time...
        //So run this one with care! (Sorry!)
        [TestMethod]
        public void TestYahooSearchSuccess()
        {

            var mockContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();

            mockContext.Request.Headers.Add("searchString", "e-Settlements");
            mockContext.Request.Headers.Add("targetUrl", "www.sympli.com.au");
            mockContext.Request.Headers.Add("resultsCount", "100");
            mockContext.Request.Headers.Add("engineTypeId", $"{(int)SearchEngineType.Yahoo}");

            var ctl = new SympliTaskBackend.Controllers.SEOSearchController() { ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext() { HttpContext = mockContext } };

            var response = ctl.Get();

            Assert.IsTrue(response.Success);
        }
    }
}
