using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace experiments
{
   public class RfidClient
    {
        Configuration conf;
        CookieCollection cookies;
        public HttpStatusCode StatusCode;

        public RfidClient(Configuration conf)
        {
            this.conf = conf;
            if (conf.isRegistered == false) Signup();
        }
        
        void Signup()
        {
            string msg;
            string post = "login="+conf.login+"&pass="+conf.pass;
            string url = conf.server+"/rfid/signup/";

            var response = SendPostData(url, post, out msg);

            if ((int)StatusCode == 200)
            {
                conf.pass = msg;
                Console.WriteLine("Success: {0}", msg);
                conf.isRegistered = true;
                conf.serialize();
            }
        }

        public void Auth()
        {
            string msg;
            string post = "login=" + conf.login + "&pass=" + conf.pass;
            string url = conf.server + "/rfid/auth/";

            var response = SendPostData(url, post, out msg);

            if ((int)StatusCode == 200)
            {
                cookies = response.Cookies;
                Console.WriteLine("Success! {0}", cookies["session_id"].Value);
            }
        }

        public void SendRfidReport(List<>, List<>)
        {


        }

        HttpWebResponse ProcessResponse(HttpWebResponse response, out string responseMsg)
        {
            responseMsg = (new StreamReader(response.GetResponseStream())).ReadToEnd();
            this.StatusCode = response.StatusCode;
            return response;
        }

        HttpWebResponse SendPostData(string URL, string postData, out string responseMsg)
        {
            byte[] byteArray;
            Stream webpageStream;
            HttpWebRequest webRequest;

            byteArray = Encoding.UTF8.GetBytes(postData);
            webRequest = (HttpWebRequest)WebRequest.Create(URL);
            webRequest.Method = "POST";
            webRequest.AllowAutoRedirect = false;
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = byteArray.Length;
            webRequest.CookieContainer = new CookieContainer();

            webpageStream = webRequest.GetRequestStream();
            webpageStream.Write(byteArray, 0, byteArray.Length);
            webpageStream.Close();

            try
            {
                return ProcessResponse((HttpWebResponse)webRequest.GetResponse(), out responseMsg);
            }

            catch (WebException e)
            {
                return ProcessResponse((HttpWebResponse)e.Response, out responseMsg);
            }
        }

    }
}
