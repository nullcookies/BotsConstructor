using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer
{
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
}