using System;
using System.IO;
using System.Net;
using System.Text;

namespace GCDC
{
    public static class WebUtils
    {
        public static string Request(string url, string post = null)
        {
            WebRequest request = WebRequest.CreateHttp("https://gcdc.herokuapp.com/api/" + url);
            request.Method = post != null ? "POST" : "GET";

            Stream dataStream;
            if (post != null)
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(post);

                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;

                dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
            }

            WebResponse response = request.GetResponse();
            Console.WriteLine(((HttpWebResponse) response).StatusDescription);

            string responseFromServer;
            using (dataStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(dataStream);
                responseFromServer = reader.ReadToEnd();
            }

            response.Close();
            return responseFromServer;
        }
    }
}