using Microsoft.VisualStudio.TestTools.UnitTesting;
using SympliTaskBackend.Controllers;
using SympliTaskBackend.Entities;

namespace SympliTaskUnitTests
{
    [TestClass]
    public class SEOSearchTests
    {
        [TestMethod]
        public void TestNoHeaders()
        {
            var ctl = new SympliTaskBackend.Controllers.SEOSearchController();
            var response = ctl.Get();
            Assert.IsTrue(!response.Success);
        }
        [TestMethod]
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
