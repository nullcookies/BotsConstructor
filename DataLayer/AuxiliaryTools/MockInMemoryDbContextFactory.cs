using System;
using System.Data.Common;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;

namespace DataLayer
{
    public interface IDbContextFactory
    {
        ApplicationContext CreateDbContext();
    }

    public class DbContextFactory:IDbContextFactory
    {
        public  ApplicationContext CreateDbContext()
        {
            return new ApplicationContext(
                new DbContextOptionsBuilder<ApplicationContext>()
                    .UseNpgsql(DbConnectionGlobals.GetConnectionString()).Options);
        }
    }
    
    public class MockInMemoryDbContextFactory:IDbContextFactory
    {
        private ApplicationContext mock; 
        public ApplicationContext Initialize()
        {
            if (mock == null)
            {
                mock =InMemoryDatabaseFactory.Create(); 
                return mock;
            }
            else
            {
                throw new Exception("Повторный вызов инициализации.");
            }
        }
        public ApplicationContext CreateDbContext()
        {
            if (mock == null)
            {
                throw new Exception("Вызов метода до вызова инициализации.");
            }
            return mock;
        }
    }

    public static class DbConnectionGlobals
    {
        static DbConnectionGlobals()
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

        private static readonly string ConnectionString;
        public static string GetConnectionString() => ConnectionString;
    }
}
