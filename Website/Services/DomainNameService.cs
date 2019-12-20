using Microsoft.AspNetCore.Hosting;

namespace Website.Services
{
    public class DomainNameService
    {
        private readonly string domainName;

        public DomainNameService(IHostingEnvironment environment)
        {
            domainName = !environment.IsDevelopment() ? "botsconstructor.com" : "localhost:5001";
        }

        public string GetDomainName()
        {
            return domainName;
        }
    }
}