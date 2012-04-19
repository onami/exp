using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace rfid
{
    public class RfidWebClient
    {
        public HttpStatusCode StatusCode { get; private set; }
        public string ResponseMsg;
        HttpWebResponse Response;

        Configuration Conf;
        CookieCollection Cookies = new CookieCollection();
        SHA1CryptoServiceProvider Sha1 = new SHA1CryptoServiceProvider();
        UTF8Encoding UniEncoding = new UTF8Encoding();

        
        public RfidWebClient(Configuration conf)
        {
            this.Conf = conf;
            if (conf.isRegistered == false) Signup();
        }

        public void SendRfidReports(List<RfidSession> unshippedSessions)
        {
            if (Auth() != 200)
            {
                return;
            }
            foreach (var unshippedSession in unshippedSessions)
            {
                //Если дубликат, тоже не отправлять.                
                if (SendRfidReport(unshippedSession) == 200 || ResponseMsg == "duplicatedMessage")
                {
                    Console.WriteLine("Success");
                    unshippedSession.deliveryStatus = RfidSession.DeliveryStatus.Shipped;
                }

                else
                {
                    Trace.WriteLine(ResponseMsg);
                }
            }
        }

        //Регистрация
        void Signup()
        {
            string post = "login=" + Conf.login + "&pass=" + Conf.pass;
            string url = Conf.server + "/rfid/signup/";

            SendPostData(url, post);

            if ((int)StatusCode == 200)
            {
                Conf.pass = ResponseMsg;
                Conf.isRegistered = true;
                Conf.serialize();
            }
        }

        //Перед отсылкой надо сделать аутентификацию и получить куки
        int Auth()
        {
            string post = "login=" + Conf.login + "&pass=" + Conf.pass;

            SendPostData(Conf.server + "/rfid/auth/", post);

            if ((int)StatusCode == 200)
            {
                Cookies = Response.Cookies;
                Cookies["session_id"].Expires = DateTime.MaxValue;
            }

            return (int)StatusCode;
        }

        //Отсылаем сессию данных
        int SendRfidReport(RfidSession session)
        {
            var jsonHashString = String.Empty;
            var jsonString = JsonConvert.SerializeObject(session);

            byte[] inHashBytes = UniEncoding.GetBytes(jsonString);
            byte[] outHashBytes = Sha1.ComputeHash(inHashBytes);
            foreach (var b in outHashBytes)
            {
                jsonHashString += b.ToString("x2");
            }

            var post = "json=" + jsonString + "&checksum=" + jsonHashString;
            SendPostData(Conf.server + "/rfid/post/", post);

            return (int)StatusCode;
        }

        void ProcessResponse(HttpWebResponse response)
        {
            this.ResponseMsg = (new StreamReader(response.GetResponseStream())).ReadToEnd();
            this.StatusCode = response.StatusCode;
            this.Response = response;
        }

        void SendPostData(string URL, string postData)
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
            webRequest.CookieContainer.Add(Cookies);

            webpageStream = webRequest.GetRequestStream();
            webpageStream.Write(byteArray, 0, byteArray.Length);
            webpageStream.Close();

            try
            {
                ProcessResponse((HttpWebResponse)webRequest.GetResponse());
            }

            catch (WebException e)
            {
                ProcessResponse((HttpWebResponse)e.Response);
            }
        }
    }
}
