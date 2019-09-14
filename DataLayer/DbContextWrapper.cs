using DataLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace DataLayer
{
    public class DbContextFactory
    {
        private readonly string _connectionString;       

        public DbContextFactory(IConfiguration configuration)
        {

            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            if (isWindows)
                _connectionString = configuration.GetConnectionString("PostgresConnectionDevelopment");
            else
                _connectionString = configuration.GetConnectionString("PostgresConnectionLinux");
            
        }
        public DbContextFactory(string connectionString)
        {
            if (connectionString != null)
            {
                _connectionString = connectionString;
            }
        }


        public ApplicationContext GetNewDbContext()
        {
            return new ApplicationContext(
                new DbContextOptionsBuilder<ApplicationContext>()
                    .UseNpgsql(_connectionString)
                    .Options);
        }
    }
}
