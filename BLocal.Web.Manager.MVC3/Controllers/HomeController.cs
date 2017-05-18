using System;
using System.Configuration;
using System.Net;
using System.Linq;
using System.Web.Mvc;
using BLocal.Web.Manager.Context;
using BLocal.Web.Manager.Models.GoogleAuthorisation;
using Newtonsoft.Json;

namespace BLocal.Web.Manager.Controllers
{
    [Authenticate]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Overview()
        {
            return View();
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Authenticate(String username, String password)
        {
            if (password == ConfigurationManager.AppSettings["password"])
                Session["auth"] = DateTime.Now;
            Session["author"] = username;
            return RedirectToAction("Overview");
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult GoogleLogin(string token)
        {
            // specify allowed domains
            string[] allowedDomains = new string[1] { "ichoosr.com" };
            // decode google token_id    
            GoogleToken googleToken = DecodeToken(token);
            bool data = false;
            // check if domain is allowed to enter
            if (googleToken.EmailVerified == "true" && allowedDomains.Contains(googleToken.Email.Split('@')[1]))
            {
                Session["auth"] = DateTime.Now;
                Session["author"] = googleToken.Email;
                data = true;
            }
            // return result of the action
            return new JsonResult() { Data = data };
        }

        private GoogleToken DecodeToken(string token)
        {
            string url = "https://www.googleapis.com/oauth2/v3/tokeninfo?id_token={token_id}";
            WebRequest httpRequest = WebRequest.Create(url.Replace("{token_id}", token));
            httpRequest.Method = "GET";
            var response = httpRequest.GetResponse();
            string result = null;
            using (var reader = new System.IO.StreamReader(response.GetResponseStream()))
            {
                result = reader.ReadToEnd();
            }
            var googleToken = JsonConvert.DeserializeObject<GoogleToken>(result);
            return googleToken;
        }
    }
}
