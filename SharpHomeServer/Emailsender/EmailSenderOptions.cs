using System.Collections.Generic;

namespace SharpHomeServer.EmailSender
{
    public class EmailSenderOptions
    {
        public static string EmailSender => "Emailing";

        public bool Enabled { get; set; }

        public string FromEmail {  get; set; }

        public string FromName { get; set; }

        public SmtpServerOptions Server { get; set; }

        public List<EmailRecipient> Recipients { get; set; }
    }
}
