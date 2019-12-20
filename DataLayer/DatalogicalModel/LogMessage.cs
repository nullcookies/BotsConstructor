using System;
using System.ComponentModel.DataAnnotations;

namespace DataLayer
{
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
        public LogLevel LogLevel { get; set; }


        [Required]
        public Source Source { get; set; }

    }
    
    public enum Source
    {
        WEBSITE,
        FOREST,
        MONITOR,
        OTHER,
        MONEY_COLLECTOR_SERVICE,
        WEBSITE_BOTS_AIRSTRIP_SERVICE,
        FOREST_BANNED_USERS_SYNCHRONIZER,
        FOREST_BOT_STATISTICS_SYNCHRONIZER,
        PASSWORD_RESET,
        WEBSITE_TOP_UP
    }
    
    public enum LogLevel
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
        IMPORTANT_INFO,
        EMAIL_SEND_FAILURE
    }

}