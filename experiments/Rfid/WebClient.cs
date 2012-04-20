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
        public enum ResponseStatus : int
        {
            OK = 0,
            internalServerError = 1,
            sessionExpired = 2,
            corruptedChecksum = 3,
            corruptedFormat = 4,
            duplicatedMessage = 5,
            invalidCredentials = 6,
            emptyRequest = 7
        };

        public string Result { get; private set;}
        public ResponseStatus Status { get; private set; }

        HttpWebResponse HttpResponse;

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
            foreach (var unshippedSession in unshippedSessions)
            {
                //Если дубликат, тоже не отправлять.
                SendRfidReport(unshippedSession);
                if (Status == ResponseStatus.OK || Status == ResponseStatus.duplicatedMessage)
                {
                    unshippedSession.deliveryStatus = RfidSession.DeliveryStatus.Shipped;
                }
            }
        }

        //Перед отсылкой надо сделать аутентификацию и получить куки
        public void Auth()
        {
            string post = "login=" + Conf.login + "&pass=" + Conf.pass;

            SendPostData(Conf.server + "/rfid/auth/", post);

            if (Status == ResponseStatus.OK)
            {
                Cookies = HttpResponse.Cookies;
                Cookies["session_id"].Expires = DateTime.MaxValue;
            }
        }

        //Регистрация
        void Signup()
        {
            string post = "login=" + Conf.login + "&pass=" + Conf.pass;
            string url = Conf.server + "/rfid/signup/";

            SendPostData(url, post);

            if (Status == null)
            {
                Conf.pass = Result;
                Conf.isRegistered = true;
                Conf.serialize();
            }
        }

        //Отсылаем сессию данных
        void SendRfidReport(RfidSession session)
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
        }

        void ProcessResponse(HttpWebResponse response)
        {
            this.HttpResponse = response;
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>((new StreamReader(response.GetResponseStream())).ReadToEnd());
            this.Result = dict["result"];
            int Status;
            if (dict["error"] == null)
            {
                this.Status = ResponseStatus.OK;
            }
            else if (Int32.TryParse(dict["error"], out Status) == true)
            {
                this.Status = (ResponseStatus)Status; 
            }
            else
            {
                this.Status = ResponseStatus.internalServerError;
            }
        }

        void SendPostData(string URL, string postData)
        {
            byte[] byteArray;
            Stream webpageStream;
            HttpWebRequest webRequest;

            byteArray = Encoding.UTF8.GetBytes(postData);
            webRequest = (HttpWebRequest)WebRequest.Create(URL);
            webRequest.Proxy = null; //в противном случае webRequest начинает поиск прокси и тратит кучу времени
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
