using Microsoft.EntityFrameworkCore;

namespace DataLayer
{
    public class DbContextFactory
    {

        
        private  static string ReleaseConnectionString =
            @"User ID = postgres;
            Password=3t0ssszheM3G4MMM0Ch~n`yparollb_wubfubrkmdbwiyro38;
            Server=127.0.0.1;
            Port=5432;
            Database=CombatVersion0008_guid=02u2h2-f2f2f2-frvebrtc;
            Integrated Security=true;
            Pooling=true;";

//        private static string DevelopmentConnectionString =
//            @"User ID = postgres;
//            Password=3t0ssszheM3G4MMM0Ch~n`yparollb_wubfubrkmdbwiyro38;
//            Server=54.89.247.235;
//            Port=5432;
//            Database=Dev_R_04_10_2019_2;
//            Integrated Security=true;
//            Pooling=true;";


        public static string GetConnectionString()
        {
            return ReleaseConnectionString;
        }
 

        public  ApplicationContext GetNewDbContext()
        {
            string connectionString = ReleaseConnectionString;
            
            return new ApplicationContext(
                new DbContextOptionsBuilder<ApplicationContext>()
                    .UseNpgsql(connectionString)
                    .Options);
        }
    }
}
