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

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
           : base(options)
        {
             Database.EnsureCreated();
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<RoleType>().HasIndex(role => new { role.Name }).IsUnique();

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
                new Account(){Id = 1_000_000,Email="qqq1@qqq" , Password="qqq", Name="шокаво", RoleTypeId = 1 },
                new Account(){Id = 1_000_001,Email="qqq2@qqq" ,  Password="qqq", Name="шокаво",  RoleTypeId = 2 }
            };

            
        

            modelBuilder.Entity<Account>().HasData(accounts);

            modelBuilder.Entity<BotDB>()
             .Property(_bot => _bot.NumberOfUniqueMessages)
             .HasDefaultValue(0);

            modelBuilder.Entity<BotDB>()
            .Property(_bot => _bot.NumberOfUniqueUsers)
            .HasDefaultValue(0);

            modelBuilder.Entity<BotDB>()
            .Property(_bot => _bot.NumberOfOrders)
            .HasDefaultValue(0);

            // Для тестирования
            modelBuilder.Entity<BotDB>().HasData(new List<object>
			{
				new {Id = 1_000_000, BotName = "Zradabot01", OwnerId = 1_000_001, BotType="BotForSales"}
			});

			var statusGroups = new List<object>()
			{
				new {Id = 1, Name = "Стандартный набор статусов", OwnerId = 1_000_001}
			};

			modelBuilder.Entity<OrderStatusGroup>().HasData(statusGroups);

			var statuses = new List<object>()
			{
				new {Id = 1, GroupId = 1, Name = "В обработке", Message = "Ваш заказ находится в обработке."},
				new {Id = 2, GroupId = 1, Name = "В пути", Message = "Ваш заказ в пути."},
				new {Id = 3, GroupId = 1, Name = "Принят", Message = "Ваш заказ был принят."},
				new {Id = 4, GroupId = 1, Name = "Отменён", Message = "Ваш заказ был отменён."}
			};

			modelBuilder.Entity<OrderStatus>().HasData(statuses);

			// Для тестирования
			modelBuilder.Entity<Order>().HasData(new List<object>
			{
				new {Id = 101, SenderId = 440090552, SenderNickname = "Ruslan Starovoitov", BotId = 1_000_000, ContainerId = 101, OrderStatusGroupId = 1,                    DateTime = DateTime.UtcNow},
				new {Id = 102, SenderId = 440090552, SenderNickname = "Ruslan Starovoitov", BotId = 1_000_000, ContainerId = 102, OrderStatusGroupId = 1, OrderStatusId = 1, DateTime = DateTime.UtcNow},
				new {Id = 103, SenderId = 440090552, SenderNickname = "Ruslan Starovoitov", BotId = 1_000_000, ContainerId = 103, OrderStatusGroupId = 1, OrderStatusId = 3, DateTime = DateTime.UtcNow}
			});

			modelBuilder.Entity<Inventory>().HasData(new List<object>
			{
				new {Id = 101, SessionId = 440090552},
				new {Id = 102, SessionId = 440090552},
				new {Id = 103, SessionId = 440090552},
				new {Id = 104, SessionId = 440090552, ParentId = 102}
			});

			modelBuilder.Entity<SessionText>().HasData(new List<object>
			{
				new {Id = 101, Text = "Sho tam?",				InventoryId = 101},
				new {Id = 102, Text = "N0rmaln0!",				InventoryId = 102},
				new {Id = 103, Text = "Waiting for Zrada...",	InventoryId = 103},
				new {Id = 104, Text = "Still waiting...",		InventoryId = 103},
				new {Id = 105, Text = "Peremoga?",				InventoryId = 103},
				new {Id = 106, Text = "She ne vmer!",			InventoryId = 104}
			});

			modelBuilder.Entity<ImageMy>().HasIndex(i => new { i.BotId, i.ProductId}).IsUnique();


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

        public string Token { get; set; }

        public string BotName { get; set; }

		[Required]
		public int OwnerId { get; set; }

        [ForeignKey("OwnerId")]
        public Account Owner { get; set; }

        public string Markup { get; set; }

        public string BotType { get; set; }

        public int NumberOfUniqueUsers { get; set; }
        public long NumberOfUniqueMessages { get; set; }
        public long NumberOfOrders { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
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
        
        //public Exception Exception{ get; set; }
        public DateTime DateTime { get; set; }
        public LogLevelMyDich LogLevel { get; set; }

        public string Message{ get; set; }
        public string LogLevelString { get; set; }

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
        I_AM_AN_IDIOT
    }


    public class RouteRecord
    {
        [Key]
        public int BotId { get; set; }



        /// <summary>
        /// Хранит ссылку на сервер леса.
        /// Например http://localhost:8080/Home/ http://15.41.87.12/Home/
        /// Если нужно остановить бота, то добавьте StopBot
        /// Если нужно запустить бота, то добавьте RunNewBot
        /// </summary>
        [Required]
        public string ForestLink { get; set; }
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
}
