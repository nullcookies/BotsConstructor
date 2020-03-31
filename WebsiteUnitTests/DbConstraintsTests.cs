// using System;
// using System.Data.Common;
// using DataLayer;
// using Microsoft.VisualStudio.TestTools.UnitTesting;
//
// namespace WebsiteTests
// {
//     [TestClass]
//     public class DbConstraintsTests
//     {
//         readonly ApplicationContext context;
//         
//         [TestInitialize]
//         public void Init()
//         {
//             TestDBHelper.BackupBeforeTest();
// 		
//             TestDBHelper.ExecScript("CreateTestUser.sql");
//
//             selenium = new DefaultSelenium("localhost", 4444, "*chrome", "http://localhost:12945/");
//             selenium.Start();
//             verificationErrors = new StringBuilder();
//         }
//
//         [TestCleanup]
//         public void CleanUp()
//         {
//             selenium.Stop();
// 		
//             TestDBHelper.RestoreAfterTest();
//         }
//
//         
//         public DbConstraintsTests()
//         {
//             string databaseName = "IntegrationTests_30_03_2020_version_01";
//             string connectionString = TestDbConfig.GetConnectionString(databaseName);
//             ApplicationContext context1 = DbContextFactory.CreateTestDbContext(connectionString);
//             context1.Accounts.Clear();
//             context1.SaveChanges();
//             context = DbContextFactory.CreateTestDbContext(connectionString);
//         }
//         
//         /// <summary>
//         /// В поле email-a можно записать только email
//         /// </summary>
//         [DataRow("emaibadEnailil.com")]
//         [TestMethod]
//         public void Test1(string email)
//         {
//             //Arrange
//            
//             
//             //Act
//             Account account = new Account()
//             {
//                 Name = "someName",
//                 EmailLoginInfo = new EmailLoginInfo
//                 {
//                     Email = email,
//                     Password = "q"
//                 }
//             };
//
//             context.Accounts.Add(account);
//             context.SaveChanges();
//             //Assert
//             
//             
//
//         }
//         /// <summary>
//         /// Название роли уникально
//         /// </summary>
//         [TestMethod]
//         public void Test2()
//         {
//             
//         }
//         /// <summary>
//         /// Нельзя добавить модератора дважды к аккаунту
//         /// </summary>
//         [TestMethod]
//         public void Test3()
//         {
//             
//         }
//         /// <summary>
//         /// Нельзя дважды банить одного пользователя
//         /// </summary>
//         [TestMethod]
//         public void Test4()
//         {
//             
//         }
//         
//     }
// }
//
//
// namespace WebsiteTests
// {
//     public static class TestDbConfig
//     {
//         public static string GetConnectionString(string databaseName)
//         {
//             if (string.IsNullOrEmpty(databaseName))
//             {
//                 throw new ArgumentException(databaseName);
//             }
//
//             var conStrBuilder = new DbConnectionStringBuilder
//             {
//                 {"User ID", "postgres"},
//                 {"Password", "3t0ssszheM3G4MMM0Ch~n`yparollb_wubfubrkmdbwiyro38"},
//                 {"Port", 5432},
//                 {"Integrated Security", true},
//                 {"Pooling", true},
//                 {"Database", databaseName},
//                 {"Server", "51.144.49.111"}
//             };
//
//             return conStrBuilder.ConnectionString;
//         }
//     }
// }