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
#warning Нужно указать адрес сервера перед запуском
                {"Server", ""},
                {"User ID", "postgres"},
#warning Нужно указать пароль перед запуском
                {"Password", "" },
                { "Port", 5432 },
#warning Нужно указать БД перед запуском
                { "Database", "" },
                { "Integrated Security", true },
                { "Pooling", true }
            };
            ConnectionString = conStrBuilder.ConnectionString;
        }

        public static string GetConnectionString() => ConnectionString;

        public  ApplicationContext GetNewDbContext()
        {
            return new ApplicationContext(
                new DbContextOptionsBuilder<ApplicationContext>()
                    .UseNpgsql(GetConnectionString()).Options);
        }
    }
}
