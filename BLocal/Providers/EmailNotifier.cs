using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using BLocal.Core;

namespace BLocal.Providers
{
    public class EmailNotifier : INotifier
    {
        public static String DefaultHost = "mail.fe.dom";
        public static String DefaultFrom = "smr@foreach.be";
        public static Func<Qualifier, IEnumerable<String>> DefaultToFuction = qualifier => new[] { "smr@foreach.be" };
        public static Func<Qualifier, IEnumerable<String>> DefaultCCFuction = qualifier => new String[0];
        public static Func<Qualifier, IEnumerable<String>> DefaultBccFuction = qualifier => new String[0];
        public static Func<Qualifier, String> DefaultSubjectFunction = qualifier => "Localization: Key not found";
        public static Func<Qualifier, String> DefaultBodyFunction = qualifier => String.Format("Could not find key [part: {0}, locale: {1}, key: {2}]", qualifier.Part, qualifier.Locale, qualifier.Key);

        internal EmailSettings Settings;

        public EmailNotifier(EmailSettings settings)
        {
            Settings = settings;
            // test settings
            if (settings.ToFunction == null || settings.From == null || settings.Host == null || settings.SubjectFunction == null || settings.BodyFunction == null)
                throw new Exception("Please at least fill in the following settings: Host, From, ToFunction, SubjectFunction, BodyFunction");
            
            new SmtpClient(settings.Host, settings.Port ?? 25);
        }

        public class EmailSettings
        {

            public static EmailSettings DefaultSettings()
            {
                return new EmailSettings
                {
                    From = DefaultFrom,
                    Host = DefaultHost,
                    ToFunction = DefaultToFuction,
                    CCFunction = DefaultCCFuction,
                    BccFunction = DefaultBccFuction,
                    SubjectFunction = DefaultSubjectFunction,
                    BodyFunction = DefaultBodyFunction
                };
            }

            public String Host { get; set; }
            public int? Port { get; set; }
            public String From { get; set; }
            public Func<Qualifier, IEnumerable<String>> ToFunction { get; set; }
            public Func<Qualifier, IEnumerable<String>> CCFunction { get; set; }
            public Func<Qualifier, IEnumerable<String>> BccFunction { get; set; }
            public Func<Qualifier, String> SubjectFunction { get; set; }
            public Func<Qualifier, String> BodyFunction { get; set; }
            public Func<Qualifier, MailPriority> PriorityFunction { get; set; }
            public Encoding MailEncoding { get; set; }
            public Action<Qualifier, Exception> ExceptionHandler { get; set; }
        }

        public virtual void NotifyMissing(Qualifier qualifier)
        {
            try
            {
                var mail = new MailMessage { From = new MailAddress(Settings.From) };

                PrepReceivers(mail, qualifier);
                PrepContent(mail, qualifier);
                PrepMisc(mail, qualifier);

                SendMail(mail);
            }
            catch (Exception e)
            {
                if (!HandleException(e, qualifier))
                    throw;
            }
        }

        internal virtual SmtpClient PrepClient()
        {
            return new SmtpClient(Settings.Host, Settings.Port ?? 25);
        }

        internal virtual bool HandleException(Exception e, Qualifier qualifier)
        {
            if (Settings.ExceptionHandler == null)
                return false;
            Settings.ExceptionHandler(qualifier, e);
            return true;
        }

        internal virtual void PrepReceivers(MailMessage mail, Qualifier qualifier)
        {
            if (Settings.ToFunction != null)
                foreach (var address in Settings.ToFunction(qualifier))
                    mail.To.Add(address);
            if (Settings.ToFunction != null)
                foreach (var address in Settings.CCFunction(qualifier))
                    mail.CC.Add(address);
            if (Settings.ToFunction != null)
                foreach (var address in Settings.BccFunction(qualifier))
                    mail.Bcc.Add(address); 
        }

        internal virtual void PrepContent(MailMessage mail, Qualifier qualifier)
        {
            if (Settings.SubjectFunction != null)
                mail.Subject = Settings.SubjectFunction(qualifier);
            if (Settings.BodyFunction != null)
                mail.Body = Settings.BodyFunction(qualifier);
        }

        internal virtual void PrepMisc(MailMessage mail, Qualifier qualifier)
        {
            if (Settings.PriorityFunction != null)
                mail.Priority = Settings.PriorityFunction(qualifier);
        }

        internal virtual void SendMail(MailMessage mail)
        {
            PrepClient().Send(mail);
        }
    }
}
