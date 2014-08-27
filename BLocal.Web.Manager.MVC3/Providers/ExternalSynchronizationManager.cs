using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;
using BLocal.Core;

namespace BLocal.Web.Manager.Providers
{
    public class ExternalSynchronizationManager : ILocalizedValueManager
    {
        private readonly string _targetUrl;
        private readonly string _targetPassword;
        private readonly string _targetProviderPair;

        public ExternalSynchronizationManager(String targetUrl, String targetPassword, String targetProviderPair)
        {
            _targetUrl = targetUrl;
            _targetPassword = targetPassword;
            _targetProviderPair = targetProviderPair;
        }

        public string GetValue(Qualifier.Unique qualifier, string defaultValue = null)
        {
            throw new NotImplementedException();
        }

        public void SetValue(Qualifier.Unique qualifier, string value)
        {
            throw new NotImplementedException();
        }

        public QualifiedValue GetQualifiedValue(Qualifier.Unique qualifier, string defaultValue = null)
        {
            throw new NotImplementedException();
        }

        public void Reload()
        {
            /*var postData = new StringBuilder();
            postData.Append("password=" + HttpUtility.HtmlEncode(_targetPassword));
            postData.Append("providerPair=" + HttpUtility.HtmlEncode(_targetProviderPair));
            var ascii = new ASCIIEncoding();
            byte[] postBytes = ascii.GetBytes(postData.ToString());

            var request = HttpWebRequest.Create(_targetUrl);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            request.GetRequestStream().wr*/

        }

        public void UpdateCreateValue(QualifiedValue value)
        {
            throw new NotImplementedException();
        }

        public void CreateValue(Qualifier.Unique qualifier, string value)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<QualifiedValue> GetAllValuesQualified()
        {
            throw new NotImplementedException();
        }

        public void DeleteValue(Qualifier.Unique qualifier)
        {
            throw new NotImplementedException();
        }

        public void DeleteLocalizationsFor(Part part, string key)
        {
            throw new NotImplementedException();
        }
    }
}