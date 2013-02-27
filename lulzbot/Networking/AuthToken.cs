using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace lulzbot.Networking
{
    public class AuthToken
    {
        private const String _login_uri = "https://www.deviantart.com/users/login";
        private const String _chat_uri  = "http://chat.deviantart.com/chat/datashare";
        private const String _regex     = "dAmn_Login\\( \"[^\"]*\", \"([^\"]*)\" \\);";

        /// <summary>
        /// Grabs the authtoken for the username and password.
        /// </summary>
        /// <param name="username">dA username</param>
        /// <param name="password">dA password</param>
        /// <returns>authtoken</returns>
        public static String Grab (String username, String password)
        {
            // This should really be replaced with an OAuth method, or the likes.

            // Make sure we can bypass certificate checks on Linux machines.
            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);

            // Initialize the request and variables.
            String page_content         = String.Empty;
            CookieContainer cookie_jar  = new CookieContainer();
            HttpWebRequest request      = (HttpWebRequest)HttpWebRequest.Create(_login_uri);

            // Create our POST data string
            String post_data = String.Format("&username={0}&password={1}&reusetoken=1", Uri.EscapeUriString(username), Uri.EscapeUriString(password));

            // Set a few request parameters
            request.KeepAlive = false;
            request.Proxy = null;
            request.CookieContainer = cookie_jar;
            request.Accept = "text/html";
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = post_data.Length;

            // Create a temporary stream writer
            using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
            {
                // Write the post data to the request, and POST the request.
                writer.Write(post_data);
                writer.Flush();
                request.GetResponse();
            }

            // Now we make a request to the chat page to get the real authtoken
            HttpWebRequest page_request = (HttpWebRequest)HttpWebRequest.Create(_chat_uri);

            // Request parameters
            page_request.Method = "GET";
            page_request.KeepAlive = false;
            page_request.Proxy = null;
            page_request.CookieContainer = cookie_jar;
            page_request.Accept = "text/html";

            // Create a temporary stream reader
            using (StreamReader reader = new StreamReader(page_request.GetResponse().GetResponseStream()))
            {
                // Grab the entire page contents
                page_content = reader.ReadToEnd();
            }

            // If the page contains the dAmn_Login function
            if (page_content.Contains("dAmn_Login"))
            {
                // Grab and return the authtoken
                Match match = Regex.Match(page_content, _regex);
                return Regex.Replace(match.Value, _regex, "$1");
            } // Otherwise, return null
            else return null;
        }

        /// <summary>
        /// Bypass certificate checks on Linux.
        /// </summary>
        private static bool ValidateRemoteCertificate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            return true;
        }
    }
}
