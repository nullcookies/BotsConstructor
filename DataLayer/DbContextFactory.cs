using DataLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;

namespace DataLayer
{
    public class DbContextFactory
    {

        private  static string ReleaseConnectionString =
@"User ID = postgres;
Password=3t0ssszheM3G4MMM0Ch~n`yparollb_wubfubrkmdbwiyro38;
Server=127.0.0.1;
Port=5432;
Database=CombatVersion0006;
Integrated Security=true;
Pooling=true;";

        private static string DevelopmentConnectionString = @"
User ID = postgres;
Password=v3rRh4rdp455lidzomObCl4vui49ri4;
Server=194.9.71.76;
Port=5432;
Database=Dev05;
Integrated Security=true;
Pooling=true;";


        public static string GetConnectionString(IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                return DevelopmentConnectionString;
            
            return ReleaseConnectionString;
        }
 

        public  ApplicationContext GetNewDbContext()
        {
            string connectionString = DevelopmentConnectionString;
            
            return new ApplicationContext(
                new DbContextOptionsBuilder<ApplicationContext>()
                    .UseNpgsql(connectionString)
                    .Options);
        }
    }
}
