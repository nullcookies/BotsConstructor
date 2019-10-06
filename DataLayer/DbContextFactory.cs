using System.Data.Common;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;

namespace DataLayer
{
    public class DbContextFactory
    {
        //private static string ReleaseConnectionString =
        //    @"User ID = postgres;
        //    Password=3t0ssszheM3G4MMM0Ch~n`yparollb_wubfubrkmdbwiyro38;
        //    Server=127.0.0.1;
        //    Port=5432;
        //    Database=CombatVersion0008_guid=02u2h2-f2f2f2-frvebrtc;
        //    Integrated Security=true;
        //    Pooling=true;";

        //        private static string DevelopmentConnectionString =
        //            @"User ID = postgres;
        //            Password=3t0ssszheM3G4MMM0Ch~n`yparollb_wubfubrkmdbwiyro38;
        //            Server=54.89.247.235;
        //            Port=5432;
        //            Database=Dev_R_04_10_2019_2;
        //            Integrated Security=true;
        //            Pooling=true;";

        private static readonly string ConnectionString;

        static DbContextFactory()
        {
            var conStrBuilder = new DbConnectionStringBuilder
            {
                {"User ID", "postgres"},
                {"Password", "3t0ssszheM3G4MMM0Ch~n`yparollb_wubfubrkmdbwiyro38" },
                { "Port", 5432 },
                { "Database", "MainDB001" },
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
                conStrBuilder.Add("Server", "3.91.239.153");
            }

            ConnectionString = conStrBuilder.ConnectionString;
        }

        public static string GetConnectionString()
        {
            return ConnectionString;
        }
 

        public  ApplicationContext GetNewDbContext()
        {
            return new ApplicationContext(
                new DbContextOptionsBuilder<ApplicationContext>()
                    .UseNpgsql(GetConnectionString())
                    .Options);
        }
    }
}
