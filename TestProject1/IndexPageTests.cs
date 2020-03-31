using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Microsoft.AspNetCore.Mvc.Testing;
using RazorPagesProject.Tests.Helpers;
using Website;
using Xunit;

namespace TestProject1
{
    public class IndexPageTests :
        IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient client;
        private readonly CustomWebApplicationFactory<Startup> factory;

        public IndexPageTests(
            CustomWebApplicationFactory<Startup> factory)
        {
            this.factory = factory;
            client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }
        
        [Fact]
        public async Task Post_DeleteAllMessagesHandler_ReturnsRedirectToRoot()
        {
            // Arrange
            var defaultPage = await client.GetAsync("/");

            var content = await HtmlHelpers.GetDocumentAsync(defaultPage);

            //Act
            var response = await client.SendAsync(
                (IHtmlFormElement)content.QuerySelector("form[id='messages']"),
                (IHtmlButtonElement)content.QuerySelector("button[id='deleteAllBtn']"));

            // Assert
            Assert.Equal(HttpStatusCode.OK, defaultPage.StatusCode);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/", response.Headers.Location.OriginalString);
        }
    }
}