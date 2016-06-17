using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using System.Net;

namespace Corecode_Twitter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(addTwitter("x@live.nl", "xxx"));
            Console.WriteLine(addTwitter("x@live.nl", "xxx"));
            Console.ReadLine();
        }

        private static string addTwitter(string username_or_email, string password)
        {
            
            WebClient coreSX = new WebClient();
            string oauth_token = coreSX.DownloadString("http://core.sx/Twitter/requestUrl.php");

            CookieContainer Cookies = new CookieContainer();
            RestClient Twitter = new RestClient("https://api.twitter.com");

            RestRequest new_authenticity_token = new RestRequest("/oauth/authorize?oauth_token=" + oauth_token);
            var post_values = Twitter.Get(new_authenticity_token);
            string authenticity_token = getBetween(post_values.Content, "name=\"authenticity_token\" type=\"hidden\" value=\"", "\">");

            foreach (var cookie in post_values.Cookies)
            {
                Cookies.Add(new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
            }
            Twitter.CookieContainer = Cookies;

            RestRequest oauth_authorize = new RestRequest("/oauth/authorize", Method.POST);
            oauth_authorize.AddParameter("authenticity_token", authenticity_token);
            oauth_authorize.AddParameter("redirect_after_login", "https://api.twitter.com/oauth/authorize?oauth_token=" + oauth_token);
            oauth_authorize.AddParameter("oauth_token", oauth_token);
            oauth_authorize.AddParameter("session[username_or_email]", username_or_email);
            oauth_authorize.AddParameter("session[password]", password);
            var verifier_oauth = Twitter.Post(oauth_authorize);

            string account_information = String.Format("[{2}={3}] {0}: ", username_or_email, new string('*', password.Length), authenticity_token, oauth_token);

            if (verifier_oauth.Content.Contains("https://support.twitter.com/articles/63510"))
                return account_information + "Your IP is currently locked for 1 hour. Please change your IP and press a key to proceed";

            else if (verifier_oauth.Content.Contains("login-challenge-form"))
                return account_information + "Account locked";

            else if (verifier_oauth.Content.Contains("<div class=\"message\">"))
                return account_information + "Incorrect password";

            else if (verifier_oauth.Content.Contains("<title>Twitter / ?</title>"))
                return account_information + "Incorrect password";

            else {

                string requestUrl = getBetween(verifier_oauth.Content, "Twitter/requestUrl.php", "\"/>");
                string addAccount = coreSX.DownloadString("http://core.sx/Twitter/requestUrl.php" + requestUrl);

                return account_information + addAccount;
            }
        }

        private static string getBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        }
    }
}
