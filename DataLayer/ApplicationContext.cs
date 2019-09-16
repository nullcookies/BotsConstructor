using DataLayer.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Models
{
	public class ApplicationContext: DbContext
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
            modelBuilder.Entity<BannedUser>().HasIndex(_bu => new { _bu.BotUsername, _bu.UserTelegramId}).IsUnique();

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

   //         // Для тестирования
   //         modelBuilder.Entity<BotDB>().HasData(new List<object>
			//{
			//	new {
   //                 Id = 1_000_000,
   //                 BotName = "ping_uin_bot",
   //                 OwnerId = 1_000_001,
   //                 BotType ="BotForSales",
   //                 Token = "825321671:AAFoJoGk7VIMU19wvOmiwZHKRwyGptvAqJ4"
   //             }
			//});
              // Для тестирования
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

			// Для тестирования
			modelBuilder.Entity<Order>().HasData(new List<object>
			{
				new {Id = 101, SenderId = 440090552, SenderNickname = "Ivan Ivanov",
                    BotId = 1_000_000, ContainerId = 101, OrderStatusGroupId = 1_000_001,                    DateTime = DateTime.UtcNow},
                new {Id = 102, SenderId = 460805780, SenderNickname = "Petro Ivanov",
                    BotId = 1_000_000, ContainerId = 102, OrderStatusGroupId = 1_000_001,                    DateTime = DateTime.UtcNow.AddMinutes(-1)}
                //,
				//new {Id = 102, SenderId = 440090552, SenderNickname = "Ruslan Starovoitov",
    //                BotId = 1_000_000, ContainerId = 102, OrderStatusGroupId = 1, OrderStatusId = 1, DateTime = DateTime.UtcNow},
				//new {Id = 103, SenderId = 440090552, SenderNickname = "Ruslan Starovoitov",
    //                BotId = 1_000_000, ContainerId = 103, OrderStatusGroupId = 1, OrderStatusId = 3, DateTime = DateTime.UtcNow}
			});

			modelBuilder.Entity<Inventory>().HasData(new List<object>
			{
				new {Id = 101, SessionId = 440090552},
				new {Id = 102, SessionId = 460805780}
               
			});
           
            int id = 101;
            modelBuilder.Entity<SessionText>().HasData(new List<object>
			{
				new {Id = id++, Text = "Сет Патриот 359 ₴: 1",				InventoryId = 101},
				new {Id = id++, Text = "Баварская 30 см Хот-дог борт (id40) 3 ₴: 1",				InventoryId = 101},
				new {Id = id++, Text = "Кальцоне 25 см Обычный борт (id45) 9 ₴: 1",				InventoryId = 101},
				new {Id = id++, Text = "Стоимость:  371 ₴",				InventoryId = 101},
				new {Id = id++, Text = "221B Baker Street",				InventoryId = 101},
				new {Id = id++, Text = "Доставьте пиццу холодной, пожалуйста.",				InventoryId = 101}

                ,

                new {Id = id++, Text = "Карбонара 30 см Хот-дог борт (id22) 2 ₴: 1",              InventoryId = 102},
                new {Id = id++, Text = "⚙️🍕Собранная пицца🍕⚙️: Помидоры (2); Грибы(2); = 6₴: 1",                InventoryId = 102},
                new {Id = id++, Text = "Калифорния с креветкой 99 ₴: 1",             InventoryId = 102},
                new {Id = id++, Text = "Стоимость: 107 ₴",             InventoryId = 102},
                new {Id = id++, Text = "221B Baker Street",             InventoryId = 102},

                new {Id = id++, Text = "Доставьте пиццу гарячей, пожалуйста.",             InventoryId = 102}
             



            });

			//modelBuilder.Entity<ImageMy>().HasIndex(i => new { i.BotId, i.ProductId}).IsUnique();

            

        }
    }

  

    [Table("Accounts")]
    public class Account
    {
		//public Account()
		//{
		//	Bots = new HashSet<BotDB>();
		//}

        [Key]
		[Column("AccountId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        //[Required]
        //public string Login { get; set; }
        [Required]
        public string Name { get; set; }

        //логин через телеграм
        public string Password { get; set; }

        public RoleType RoleType { get; set; }

        [Column("RoleTypeId")]
        [Required]
        public int RoleTypeId { get; set; }

        //email не обязателен, тк возможен логин через телеграм
        public string Email { get;  set; }

        public int TelegramId { get; set; }

        public decimal Money { get; set; }

        public virtual ICollection<BotDB> Bots { get; set; }
    }

    [Table("RoleTypesMy")]
    public class RoleType
    {
		//public RoleType()
		//{
		//	this.Accounts = new HashSet<Account>();
		//}

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Name { get; set; }

        public virtual ICollection<Account> Accounts { get; set; }
    }

    [Table("Bots")]
    public class BotDB
    {
		//public BotDB()
		//{
		//	this.Orders = new HashSet<Order>();
		//}

        [Key]
		[Column("BotId")]
        public int Id { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public string BotName { get; set; }

		[Required]
		public int OwnerId { get; set; }

        [ForeignKey("OwnerId")]
        public Account Owner { get; set; }

        public string Markup { get; set; }

        public string BotType { get; set; }


        public virtual ICollection<Order> Orders { get; set; }
    }

    public class BotForSalesStatistics
    {
        [Key]
        public int BotId { get; set; }

        [ForeignKey("BotId")]
        public BotDB Bot { get; set; } 

        [Required]
        public long NumberOfOrders          { get; set; }
        [Required]
        public int  NumberOfUniqueUsers     { get; set; }
        [Required]
        public long NumberOfUniqueMessages  { get; set; }
    }

	/// <summary>
	/// Статус заказа.
	/// </summary>
	[Table("OrderStatuses")]
	public class OrderStatus
	{
		[Key]
		[Required]
		[Column("OrderStatusId")]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[ForeignKey("OrderStatusGroupId")]
		public int GroupId { get; set; }

		[Required]
		[ForeignKey("GroupId")]
		public virtual OrderStatusGroup Group { get; set; }

		[Required]
		public string Name { get; set; }

		[Required]
		public string Message { get; set; }
	}

	/// <summary>
	/// Группа из статусов заказа определённого владельца, из которой можно выбирать статусы.
	/// </summary>
	[Table("OrderStatusesGroups")]
	public class OrderStatusGroup
	{
		[Key]
		[Required]
		[Column("OrderStatusGroupId")]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		public string Name { get; set; }

		[Required]
		[ForeignKey("AccountId")]
		public virtual int OwnerId { get; set; }

		[Required]
		[ForeignKey("OwnerId")]
		public virtual Account Owner { get; set; }

		public virtual ICollection<OrderStatus> OrderStatuses { get; set; }
	}

	

	[Table("Images")]
    public class ImageMy
    {
        public int Id { get; set; }
        public long ProductId { get; set; }
        public int BotId { get; set; }
        public string Name { get; set; }
        public byte[] Photo { get; set; }
    }

    [Table("UnconfirmedEmails")]
    public class UnconfirmedEmail
    {
        public int Id { get; set; }
        [Required]
        public int AccountId { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public Guid GuidPasswordSentToEmail { get; set; }

    }

    [Table("AccountsToResetPassword")]
    public class AccountToResetPassword
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int AccountId { get; set; }
        [Required]
        public Guid GuidPasswordSentToEmail { get; set; }

    }

    public class LogMessage
    {
        [Key]
        public int Id { get; set; }
        public string LogLevelString { get; set; }
        [Required]
        public string SourceString
        {
            get
            {
                return Source.ToString();
            }
            set{}
         }
        public string Message{ get; set; }

        public int AccountId { get; set; }

        public DateTime DateTime { get; set; }
        public LogLevelMyDich LogLevel { get; set; }


        [Required]
        public Source Source { get; set; }

    }
    public enum LogLevelMyDich
    {
        INFO,
        CRITICAL_SECURITY_ERROR,
        LOGICAL_DATABASE_ERROR,        
        ERROR,
        FATAL,
        USER_ERROR,
        UNAUTHORIZED_ACCESS_ATTEMPT,
        USER_INTERFACE_ERROR_OR_HACKING_ATTEMPT,
        I_AM_AN_IDIOT,
        WARNING,
        SPYING,
        IMPORTANT_INFO
    }

    [Table("RouteRecords")]
    public class RouteRecord
    {
        [Key]
        public int BotId { get; set; }

        [ForeignKey("BotId")]
        public BotDB Bot { get; set; }

        /// <summary>
        /// Хранит ссылку на сервер леса.
        /// Например http://localhost:8080/Home/ http://15.41.87.12/Home/
        /// </summary>
        [Required]
        public string ForestLink { get; set; }
    }

    public class BotLaunchRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BotId { get; set; }

        [Required]
        public BotStatus BotStatus { get; set; }

        [Required]
        public string BotStatusString {
            get
            {
                return BotStatus.ToString();
            }
            set{}
        }


        [Required]
        public DateTime Time { get; set; }
    }

    public enum BotStatus
    {
        STARTED,
        STOPPED
    }

    /// <summary>
    /// Заказ.
    /// </summary>
    [Table("Orders")]
    public class Order
    {
        [Key]
        [Column("OrderId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int SenderId { get; set; }

        public string SenderNickname { get; set; }

        [Required]
        [ForeignKey("BotId")]
        public int BotId { get; set; }

        [Required]
        [ForeignKey("BotId")]
        public virtual BotDB Bot { get; set; }

        [Required]
        public DateTime DateTime { get; set; }

        [ForeignKey("OrderStatusId")]
        public int? OrderStatusId { get; set; }

        [ForeignKey("OrderStatusId")]
        public virtual OrderStatus OrderStatus { get; set; }

        [Required]
        [ForeignKey("OrderStatusGroupId")]
        public int OrderStatusGroupId { get; set; }

        [Required]
        [ForeignKey("OrderStatusGroupId")]
        public virtual OrderStatusGroup OrderStatusGroup { get; set; }

        [Required]
        [ForeignKey("InventoryId")]
        public int ContainerId { get; set; }

        [Required]
        [ForeignKey("ContainerId")]
        public virtual Inventory Container { get; set; }
    }
    /// <summary>
    /// Множества элементов, принадлежащих одной сессии (например, заказы или содержимое сундуков в RPG).
    /// </summary>
    [Table("Inventories")]
    public class Inventory
    {
        [Key]
        [Required]
        [Column("InventoryId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int SessionId { get; set; }

        public virtual ICollection<InventoryItem> Items { get; set; }

        public virtual ICollection<SessionText> Texts { get; set; }

        public virtual ICollection<SessionFile> Files { get; set; }

        [ForeignKey("InventoryId")]
        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public virtual Inventory Parent { get; set; }

        public virtual ICollection<Inventory> Children { get; set; }
    }

    /// <summary>
    /// Предметы, которые привязаны к пользователю (товары в корзине).
    /// </summary>
    [Table("InventoryItems")]
    public class InventoryItem
    {
        [Key]
        [Required]
        [Column("InventoryItemId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int Count { get; set; }

        [Required]
        [ForeignKey("ItemId")]
        public int ItemId { get; set; }

        [Required]
        [ForeignKey("ItemId")]
        public virtual Item Item { get; set; }

        [Required]
        [ForeignKey("InventoryId")]
        public int InventoryId { get; set; }

        [Required]
        [ForeignKey("InventoryId")]
        public virtual Inventory Inventory { get; set; }
    }
    /// <summary>
    /// Тексты, которые были отправлены пользователем в специальный "инвентарь".
    /// </summary>
    [Table("SessionsTexts")]
    public class SessionText
    {
        [Key]
        [Required]
        [Column("SessionTextId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }

        [Required]
        [ForeignKey("InventoryId")]
        public int InventoryId { get; set; }

        [Required]
        [ForeignKey("InventoryId")]
        public virtual Inventory Inventory { get; set; }
    }

    /// <summary>
    /// Файлы, которые были отправлены пользователем в специальный "инвентарь".
    /// </summary>
    [Table("SessionsFiles")]
    public class SessionFile
    {
        [Key]
        [Required]
        [Column("SessionFileId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string FileId { get; set; }

        // Некоторые файлы не имеют превью
        public string PreviewId { get; set; }

        // Описание пользователь может и не добавить
        public string Description { get; set; }

        [Required]
        [ForeignKey("InventoryId")]
        public int InventoryId { get; set; }

        [Required]
        [ForeignKey("InventoryId")]
        public virtual Inventory Inventory { get; set; }
    }


    /// <summary>
    /// Предметы (товары), заданные владельцем бота.
    /// </summary>
    [Table("Items")]
    public class Item
    {
        [Key]
        [Required]
        [Column("ItemId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Value { get; set; }

        [Required]
        [ForeignKey("BotId")]
        public int BotId { get; set; }

        [Required]
        [ForeignKey("BotId")]
        public virtual BotDB Bot { get; set; }
    }

    [Table("Moderators")]
    public class Moderator
    {
        [Key]
        [Column("ModeratorId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int BotId { get; set; }
        [ForeignKey("BotId")]
        public BotDB BotDB { get; set; }

        [Required]
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public Account Account { get; set; }
    }

    /// <summary>
    ///  Таблица хранит всю историю цен. 
    ///  Актуальное значение костант, конечно же хранится последней
    ///  записью.
    /// </summary>
    public class BotForSalesPrice
    {
        [Key]
        [Column("BotForSalesPriceId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public decimal MaxPrice { get; set; }
        [Required]
        public decimal MinPrice { get; set; }
        [Required]
        public decimal DailyPrice { get; set; }
        [Required]
        public decimal MagicParameter { get; set; }
        [Required]
        public DateTime DateTime { get; set; }

    }

    public class WithdrawalLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AccountId { get; set; }
        public  Account Account { get; set; }

        [Required]
        public int BotId { get; set; }

        [ForeignKey("BotId")]
        public BotDB BotDB { get; set; }

        [Required]
        public TransactionStatus TransactionStatus { get; set; }
        [Required]
        public string TransactionStatusString {
            get
            {
                return TransactionStatus.ToString();
            }
            set{}
        }

        public decimal Price { get; set; }
        /// <summary>
        /// День, за который списываются деньги
        /// </summary>
        public DateTime DateTime { get; set;}

    }

    public enum TransactionStatus
    {
        TRANSACTION_STARTED,
        TRANSACTION_COMPLETED_SUCCESSFULL
    }

    public class Record_BotUsername_UserTelegramId
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string BotUsername { get; set; }
        [Required]
        public int BotUserTelegramId { get; set; }
    }

    public class BannedUser
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string BotUsername { get; set; }
        [Required]
        public int UserTelegramId { get; set; }
    }

    /// <summary>
    /// В этой таблице хранятся записи о работе ботов
    /// На её основе определяется работал бот в этот день или нет
    /// </summary>
    public class BotWorkLog
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int BotId { get; set; }
        [Required]
        public DateTime InspectionTime { get; set; }
    }

    public class SpyRecord
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public DateTime Time { get; set; }
        [Required]
        public string PathCurrent { get; set; }
        [Required]
        public string PathFrom { get; set; }
        [Required]
        public int AccountId { get; set; }
    }



}
