using System.Data.Common;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;

namespace DataLayer
{
    public class DbContextFactory
    {
        private static readonly string ConnectionString;
        
        static DbContextFactory()
        {
            var conStrBuilder = new DbConnectionStringBuilder
            {
                {"User ID", "postgres"},
                {"Password", "3t0ssszheM3G4MMM0Ch~n`yparollb_wubfubrkmdbwiyro38" },
                { "Port", 5432 },
                { "Database", "MainDB001" },
                //{ "Database", "Ruslan_22_11_2019_number2" },
                { "Integrated Security", true },
                { "Pooling", true }
            };
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                conStrBuilder["Database"] += "Rel";
                conStrBuilder.Add("Server", "127.0.0.1");
            }
            else
            {
                conStrBuilder["Database"] += "Dev";
                conStrBuilder.Add("Server", "3.88.252.188");
            }
            ConnectionString = conStrBuilder.ConnectionString;
        }

        public static string GetConnectionString() => ConnectionString;

        public  ApplicationContext GetNewDbContext()
        {
            return new ApplicationContext(
                new DbContextOptionsBuilder<ApplicationContext>()
                    .UseNpgsql(GetConnectionString())
                    .Options);
        }
    }
}
