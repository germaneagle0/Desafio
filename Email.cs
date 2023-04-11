using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using System.Text;

namespace Email
{
    public abstract class EmailSender {
        public abstract void send(string message, string emailTo);
    }

    public class Logger: EmailSender {
        public override void send(string message, string emailTo)
        {
            Console.WriteLine($"Logger sends email to {emailTo} with the message:");
            Console.WriteLine(message);
        }
    }

    public class Gmail: EmailSender
    {
        string[] Scopes = { GmailService.Scope.GmailSend };
        string senderEmail;
        string TitleMsg;
        
        UserCredential credential;
        GmailService service;

        public Gmail(string senderEmail, string AppName, string FilePath, string TitleMsg)
        {
            this.TitleMsg = TitleMsg;
            this.senderEmail = senderEmail;
            // Get Credentials
            string file = Path.Combine(Environment.CurrentDirectory, FilePath);
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                this.credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    new[] { GmailService.Scope.GmailSend },
                    "me",
                    CancellationToken.None).Result;
            }
            // Create Gmail API service
            this.service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = AppName
            });
        }

        public Gmail(Config.PrivateEmailData data):
            this(data.Email, data.AppName, data.CredentialFile, data.Title) {}

        string Base64UrlEncode(string input)
        {
            var data = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(data).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        public override void send(string message, string emailTo)
        {
            var msg = new Message();
            msg.Raw = Base64UrlEncode(CreateMessage(this.senderEmail, emailTo, this.TitleMsg, message));

            // Send message
            var request = this.service.Users.Messages.Send(msg, "me");
            request.Execute();
        }

        static string CreateMessage(string from, string to, string subject, string body)
        {
            var message = new StringBuilder();
            message.Append("From: <" + from + ">\r\n");
            message.Append("To: <" + to + ">\r\n");
            message.Append("Subject: " + subject + "\r\n");
            message.Append("Content-Type: text/html; charset=UTF-8\r\n");
            message.Append("\r\n");
            message.Append(body);
            return message.ToString();
        }
    }
}
