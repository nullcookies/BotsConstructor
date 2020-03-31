using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;


namespace DataLayer
{
	public sealed class ApplicationContext: DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        // public DbSet<RoleType> AuthRoles { get; set; }
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
        public DbSet<TemporaryAccountWithUsernameAndPassword> TemporaryAccountWithUsernameAndPassword { get; set; }
        public DbSet<EmailLoginInfo>EmailLoginInfo { get; set; }
        public DbSet<TelegramLoginInfo> TelegramLoginInfo { get; set; }
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
            modelBuilder.Entity<Moderator>().HasIndex(_mo => new { _mo.AccountId, _mo.BotId}).IsUnique();
            
            modelBuilder.Entity<EmailLoginInfo>().HasIndex(info => info.Email).IsUnique();
            modelBuilder.Entity<EmailLoginInfo>().HasIndex(info => info.AccountId).IsUnique();
            
            modelBuilder.Entity<TelegramLoginInfo>().HasIndex(info => info.TelegramId).IsUnique();
            modelBuilder.Entity<TemporaryAccountWithUsernameAndPassword>()
                .HasIndex(tmp => tmp.Email).IsUnique();
            
            modelBuilder.Entity<TemporaryAccountWithUsernameAndPassword>()
                .HasIndex(tmp => tmp.Guid).IsUnique();


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

        

            modelBuilder.Entity<EmailLoginInfo>()
                .HasIndex(a => a.Email)
                .IsUnique();
            
            modelBuilder.Entity<EmailLoginInfo>()
                .HasIndex(a => a.AccountId)
                .IsUnique();
            
            modelBuilder.Entity<TelegramLoginInfo>()
                .HasIndex(a => a.TelegramId)
                .IsUnique();
            
            modelBuilder.Entity<TelegramLoginInfo>()
                .HasIndex(a => a.AccountId)
                .IsUnique();
            
            modelBuilder.Entity<Account>()
                .Property(_acc => _acc.Money)
                .HasDefaultValue(0);
            

            BotForSalesPrice price =
                new BotForSalesPrice
                {
                    Id = int.MinValue,
                    MaxPrice = 3,
                    MinPrice = 2,
                    MagicParameter = 10,
                    DateTime = DateTime.UtcNow,
                    DailyPrice = 7
                };

            modelBuilder.Entity<BotForSalesPrice>().HasData(price);



            // modelBuilder.Entity<Account>().HasData(accounts);
            // modelBuilder.Entity<EmailLoginInfo>().HasData(emailPasswordLoginInfo);

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
        }
    }
}
