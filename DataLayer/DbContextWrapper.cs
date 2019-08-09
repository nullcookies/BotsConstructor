using DataLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace DataLayer
{
    public class DbContextWrapper
    {
        private readonly string _connextionString;       

        public DbContextWrapper(IConfiguration configuration)
        {

            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            if (isWindows)
                _connextionString = configuration.GetConnectionString("PostgresConnectionDevelopment");
            else
                _connextionString = configuration.GetConnectionString("PostgresConnectionLinux");
            
        }


        public ApplicationContext GetDbContext()
        {
            return new ApplicationContext(
                new DbContextOptionsBuilder<ApplicationContext>()
                .UseNpgsql(_connextionString)
                .Options);
        }
    }
}
