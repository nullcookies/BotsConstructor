using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;


namespace DataLayer
{
	public sealed class ApplicationContext: DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<RoleType> AuthRoles { get; set; }
        public DbSet<BotDB> Bots { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
		public DbSet<OrderStatusGroup> OrderStatusGroups { get; set; }
		public DbSet<Item> Items { get; set; }
		public DbSet<InventoryItem> InventoryItems { get; set; }
		public DbSet<Inventory> Inventories { get; set; }
		public DbSet<SessionText> SessionTexts { get; set; }

		public DbSet<SessionFile> SessionFiles { get; set; }
		public DbSet<ImageMy> Images { get; set; }
        public DbSet<UnconfirmedEmail> UnconfirmedEmails { get; set; }
        public DbSet<AccountToResetPassword> AccountsToResetPassword { get; set; }
        public DbSet<RouteRecord> RouteRecords { get; set; }
        public DbSet<LogMessage> LogMessages { get; set; }
        public DbSet<BotForSalesStatistics> BotForSalesStatistics { get; set; }
        public DbSet<Moderator> Moderators { get; set; }
        public DbSet<BotForSalesPrice> BotForSalesPrices { get; set; }
        public DbSet<BotLaunchRecord> BotLaunchRecords { get; set; }

        public DbSet<WithdrawalLog> WithdrawalLog { get; set; }
        public DbSet<Record_BotUsername_UserTelegramId> BotUsers { get; set; }
        public DbSet<BannedUser> BannedUsers { get; set; }
        public DbSet<BotWorkLog> BotWorkLogs { get; set; }
        public DbSet<SpyRecord> SpyRecords { get; set; }
        
        public DbSet<PingRecord> PingRecords { get; set; }
      

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
           : base(options)
        {
             Database.EnsureCreated();
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RoleType>().HasIndex(role => new { role.Name }).IsUnique();

            //Нельзя добавить модератора дважды к аккаунту
            modelBuilder.Entity<Moderator>().HasIndex(_mo => new { _mo.AccountId, _mo.BotId}).IsUnique();

            //Нельзя дважды банить одного пользователя
            // modelBuilder.Entity<BannedUser>().HasIndex(_bu => new { _bu.BotUsername, _bu.UserTelegramId}).IsUnique();

            //Пользователь считается однажды
            modelBuilder.Entity<Record_BotUsername_UserTelegramId>()
                .HasIndex(_rec => 
                    new
                    {
                        _rec.BotUsername,
                        _rec.BotUserTelegramId
                    })
                .IsUnique();


            //В одну дату нельзя снимать деньги больше одного раза
            modelBuilder.Entity<WithdrawalLog>().HasIndex(_wl=> new
            {
                _wl.BotId,
                _wl.DateTime
            }).IsUnique();

            var roles = new List<RoleType>()
            {
                new RoleType(){ Id = 1, Name="admin"},
                new RoleType(){ Id = 2, Name = "user"},
                new RoleType(){ Id = 3, Name = "moderator"}
            };
            modelBuilder.Entity<RoleType>().HasData(roles);

            modelBuilder.Entity<Account>()
                .HasIndex(a => a.Email)
                .IsUnique();

            modelBuilder.Entity<Account>()
                .Property(_acc => _acc.Money)
                .HasDefaultValue(0);

            var accounts = new List<Account>()
            {
                new Account(){Id = 1_000_000,Email="qqq1@qqq" , Password="qqq", Name="Иван Иванов", RoleTypeId = 1 },
                new Account(){Id = 1_000_001,Email="qqq2@qqq" ,  Password="qqq", Name="Пётр Петров",  RoleTypeId = 2 , Money = 1000},
                new Account(){Id = 1_000_002,Email="qqq3@qqq" ,  Password="qqq", Name="Сидор Сидоров",  RoleTypeId = 1 }
            };


            BotForSalesPrice price =
                new BotForSalesPrice()
                {
                    Id = int.MinValue,
                    MaxPrice = 3,
                    MinPrice = 2,
                    MagicParameter = 10,
                    DateTime = DateTime.UtcNow,
                    DailyPrice = 7
                };

            modelBuilder.Entity<BotForSalesPrice>().HasData(price);



            modelBuilder.Entity<Account>().HasData(accounts);

            modelBuilder.Entity<BotForSalesStatistics>()
                 .Property(_botStat => _botStat.NumberOfUniqueMessages)
                 .HasDefaultValue(0);

            modelBuilder.Entity<BotForSalesStatistics>()
                .Property(_botStat => _botStat.NumberOfUniqueUsers)
                .HasDefaultValue(0);

            modelBuilder.Entity<BotForSalesStatistics>()
                .Property(_botStat => _botStat.NumberOfOrders)
                .HasDefaultValue(0);

            modelBuilder.Entity<OrderStatusGroup>().Property(group => group.IsOld).HasDefaultValue(false);

            modelBuilder.Entity<OrderStatus>().Property(status => status.IsOld).HasDefaultValue(false);


            modelBuilder.Entity<BotDB>().HasIndex(_bot => _bot.Token).IsUnique();

            modelBuilder.Entity<BotDB>().HasData(new List<object>
			{
				new {
                    Id = 1_000_000,
                    BotName = "my_pizzeria_bot",
                    OwnerId = 1_000_001,
                    BotType ="BotForSales",
                    Token = "724246784:AAHLOtr3Vz_q0Cf5iQvuY_bf-kVm0s-JAMU"
                }
			});

            
            modelBuilder.Entity<BotLaunchRecord>().HasData(new List<BotLaunchRecord>
            {
                new BotLaunchRecord(){
                    Id = int.MinValue,
                    BotId = 1_000_000,
                    Time  = DateTime.UtcNow.AddHours(-5)                   
                }
            });


            modelBuilder.Entity<Moderator>().HasData(new Moderator()
            {
                Id = 1_000_000,
                AccountId = 1_000_002,
                BotId= 1_000_000
            });

            modelBuilder.Entity<BotForSalesStatistics>().HasData(new List<BotForSalesStatistics>
            {
                new BotForSalesStatistics(){BotId=1_000_000}
            });

            var statusGroups = new List<object>()
			{
				new {Id = 1_000_001, Name = "Стандартный набор статусов", OwnerId = 1_000_001}
			};

			modelBuilder.Entity<OrderStatusGroup>().HasData(statusGroups);

            var statuses = new List<object>()
			{
				new {Id = 1_000_001, GroupId = 1_000_001, Name = "В обработке", Message = "Ваш заказ находится в обработке."},
				new {Id = 1_000_002, GroupId = 1_000_001, Name = "В пути", Message = "Ваш заказ в пути."},
				new {Id = 1_000_003, GroupId = 1_000_001, Name = "Принят", Message = "Ваш заказ был принят."},
				new {Id = 1_000_004, GroupId = 1_000_001, Name = "Отменён", Message = "Ваш заказ был отменён."}
			};

			modelBuilder.Entity<OrderStatus>().HasData(statuses);

			

        }
    }


  
}
