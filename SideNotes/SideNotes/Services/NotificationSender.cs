using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Services.Abstract;
using SideNotes.Models;
using System.Net.Mail;

namespace SideNotes.Services
{
    public class NotificationSender : INotificationSender
    {
        readonly string fromAddress;
        readonly SmtpClient client;

        public NotificationSender(string fromAddress)
        {
            this.fromAddress = fromAddress;
            client = new SmtpClient();
        }


        public void SendAll()
        {
            List<Exception> exceptions = new List<Exception>();
            using (var context = new SideNotesEntities())
            {
                IList<Notification> notifications = context.Notifications.ToList();
                foreach (Notification note in notifications)
                {
                    MailMessage message = new MailMessage();
                    message.From = new MailAddress(fromAddress);
                    message.To.Add(new MailAddress(note.Email));
                    message.Subject = note.Subject;
                    message.Body = note.Body;
                    message.IsBodyHtml = true;

                    try
                    {
                        client.Send(message);

                        SentNotification sent = new SentNotification()
                        {
                            Email = note.Email,
                            Subject = note.Subject,
                            Body = note.Body,
                            Date = DateTime.Now
                        };

                        context.SentNotifications.AddObject(sent);
                        context.Notifications.DeleteObject(note);
                        context.SaveChanges();
                    }
                    catch(Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }
            }
            if (exceptions.Count > 0) throw exceptions.First();
        }
    }
}