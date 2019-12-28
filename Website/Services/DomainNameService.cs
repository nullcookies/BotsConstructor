using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.CodeAnalysis;

namespace Website.Services
{
    public class DomainNameService
    {
        private readonly string domainName;
        public DomainNameService()
        {
            domainName = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "botsconstructor.com" : "localhost:5001";
        }

        public string GetDomainName()
        {
            return domainName;
        }
    }
}