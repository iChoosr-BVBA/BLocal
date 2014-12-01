using System;
using System.Security.Cryptography;
using System.Text;

namespace BLocal.Core
{
    public class ContentHasher
    {
        public String CalculateHash(String content)
        {
            if (content == null)
                return String.Empty;

            var hashBuilder = new StringBuilder();
            using (var md5 = MD5.Create())
            {
                var byteHash = md5.ComputeHash(Encoding.Unicode.GetBytes(content));
                foreach (var b in byteHash)
                {
                    hashBuilder.Append(b.ToString("x2"));
                }
            }
            return hashBuilder.ToString();
        }
    }
}
