using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using System.Threading;
using BLocal.Core;

namespace BLocal.Providers
{
    public class BatchEmailNotifier : INotifier
    {
        private List<String> _mails;
        private TimeSpan _maxMailTimeout;
        private int _maxQueueSize;
        private bool _sending;
        private Thread _timeThread;
        private DateTime? _lastAdd;
        private String _from;
        private String _to;
        private String _server;
        private String _subject;
        private Func<Qualifier, String> _bodyLineFunction;


        public BatchEmailNotifier(String fromAddress, String toAddress, String subject, String mailserver, Func<Qualifier, String> bodyLineFunction, TimeSpan maxMailTimeout, int maxMailCount)
        {
            _subject = subject;
            _mails = new List<String>();
            _maxMailTimeout = maxMailTimeout;
            _maxQueueSize = maxMailCount;
            _bodyLineFunction = bodyLineFunction;
            _from = fromAddress;
            _to = toAddress;
            _server = mailserver;
            _timeThread = new Thread(Watch);
            _timeThread.Start();
        }

        public void NotifyMissing(Qualifier qualifier)
        {
            lock (_mails)
            {
                _mails.Add(_bodyLineFunction(qualifier));
                if (_mails.Count > _maxQueueSize)
                    SendBatch();
                if (_lastAdd == null)
                    _lastAdd = DateTime.Now;
            }
        }

        private void Watch()
        {
            for (;;)
            {
                // check once every 30 seconds
                Thread.Sleep(30000);
                if(_lastAdd == null)
                    continue;
                if ((DateTime.Now - _lastAdd) > _maxMailTimeout)
                {
                    _lastAdd = null;
                    if(_mails.Count > 0)
                        SendBatch();
                }
            }
        }

        private void SendBatch()
        {
            lock (_mails)
            {
                if (_sending)
                    return;
                _sending = true;

                var client = new SmtpClient(_server);
                var message = new MailMessage(_from, _to) {Subject = _subject};
                var s = new StringBuilder();

                foreach (var mail in _mails)
                    s.AppendLine(mail);
                _mails.Clear();

                message.Body = s.ToString();

                client.Send(message);

                _sending = false;
            }
        }
    }
}
