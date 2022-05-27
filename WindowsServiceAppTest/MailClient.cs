using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServiceAppTest
{
    class MailClient
    {
        private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            // Get the unique identifier for this asynchronous operation.
            string token = (string)e.UserState;

            if (e.Cancelled)
            {
                Console.WriteLine("[{0}] Send canceled.", token);
            }
            if (e.Error != null)
            {
                Console.WriteLine("[{0}] {1}", token, e.Error.ToString() + "hier ben ik");
            }
            else
            {
                Console.WriteLine("Message sent.");
            }
        }

        public static void TestMail(string title, string text, string FilePath, KrXmlData xmlData)
        {
            string UserEmail = xmlData.Email;
            // Command-line argument must be the SMTP host.
            SmtpClient client = new SmtpClient(xmlData.MailServerNaam, xmlData.MailServerPoort);
            // Specify the email sender.
            // Create a mailing address that includes a UTF8 character
            // in the display name.
            MailAddress from = new MailAddress(xmlData.MailVerzendenVanuitEmail,
                           "Foutmelding " + title,
                       System.Text.Encoding.UTF8);
            // Set destinations for the email message.
            MailAddress to = new MailAddress(UserEmail);
            // Specify the message content.
            MailMessage message = new MailMessage(from, to);
            // Include some non-ASCII characters in body and subject.
            message.Body = text;
            message.Subject = "Foutmelding " + title;
            message.Attachments.Add(new Attachment(FilePath));
            message.BodyEncoding = System.Text.Encoding.UTF8;
            message.SubjectEncoding = System.Text.Encoding.UTF8;
            // Set the method that is called back when the send operation ends.
            client.SendCompleted += new
            SendCompletedEventHandler(SendCompletedCallback);
            // The userState can be any object that allows your callback
            // method to identify this send operation.
            // For this example, the userToken is a string constant.
            if (xmlData.MailServerGebruikersnaam != string.Empty && xmlData.MailServerWachtwoord != string.Empty)
            {
                client.Credentials = new NetworkCredential(xmlData.MailServerGebruikersnaam,
                                                           xmlData.MailServerWachtwoord);
            }
            client.Send(message);
            string answer = Console.ReadLine();
            // Clean up.
            message.Dispose();
        }
    }
}
