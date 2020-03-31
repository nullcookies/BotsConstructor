using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Website;

namespace TestProject1
{
    public class BasicTests 
        : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> factory;

        public BasicTests(WebApplicationFactory<Startup> factory)
        {
            this.factory = factory;
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/Index")]
        [InlineData("/About")]
        [InlineData("/Privacy")]
        [InlineData("/Contact")]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
        {
            // Arrange
            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }
        
        
    }
}