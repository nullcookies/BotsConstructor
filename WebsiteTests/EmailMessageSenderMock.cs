using MyLibrary;
using Website.Services;

namespace WebsiteTests
{
    public class EmailMessageSenderMock:EmailMessageSender
    {
        public EmailMessageSenderMock(SimpleLogger logger) : base(logger)
        {
        }

        public override bool SendEmailCheck(string email, string name, string link, string emailSender = null, string emailSenderPass = null)
        {
            return true;
        }
    }
}