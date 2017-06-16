using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace BLocal.Web.Manager.Models.GoogleAuthorisation
{
    [JsonObject]
    public class GoogleToken
    {
        [JsonProperty(PropertyName = "iss")]
        public string Iss { get; set; }

        [JsonProperty(PropertyName = "sub")]
        public string Sub { get; set; }

        [JsonProperty(PropertyName = "azp")]
        public string Azp { get; set; }

        [JsonProperty(PropertyName = "aud")]
        public string Aud { get; set; }

        [JsonProperty(PropertyName = "iat")]
        public string Iat { get; set; }

        [JsonProperty(PropertyName = "exp")]
        public string Expiration { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "email_verified")]
        public string EmailVerified { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "picture")]
        public string Picture { get; set; }

        [JsonProperty(PropertyName = "given_name")]
        public string Given_name { get; set; }

        [JsonProperty(PropertyName = "family_name")]
        public string Family_name { get; set; }

        [JsonProperty(PropertyName = "locale")]
        public string Locale { get; set; }
    }
}