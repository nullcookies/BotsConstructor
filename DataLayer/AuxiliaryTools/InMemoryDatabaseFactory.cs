using System;
using Microsoft.EntityFrameworkCore;

namespace DataLayer
{
    public static class InMemoryDatabaseFactory
    {
        public static ApplicationContext Create()
        {
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var dbContext = new ApplicationContext(options);
            return dbContext;
        }
    }
}