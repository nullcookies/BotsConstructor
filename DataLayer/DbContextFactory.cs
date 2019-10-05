﻿using DataLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;

namespace DataLayer
{
    public class DbContextFactory
    {

//        private  static string ReleaseConnectionString =
//            @"User ID = postgres;
//            Password=3t0ssszheM3G4MMM0Ch~n`yparollb_wubfubrkmdbwiyro38;
//            Server=127.0.0.1;
//            Port=5432;
//            Database=CombatVersion0006;
//            Integrated Security=true;
//            Pooling=true;";

        private static string DevelopmentConnectionString =
            @"User ID = postgres;
            Password=3t0ssszheM3G4MMM0Ch~n`yparollb_wubfubrkmdbwiyro38;
            Server=3.91.239.153;
            Port=5432;
            Database=Dev0165184R;
            Integrated Security=true;
            Pooling=true;";


        public static string GetConnectionString()
        {
            return DevelopmentConnectionString;
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
