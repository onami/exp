using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace DL6970.Rfid
{
    public class RfidWebClient
    {
        public enum ResponseStatus
        {
            Ok = 0,
            InternalServerError = 1,
            SessionExpired = 2,
            CorruptedChecksum = 3,
            CorruptedFormat = 4,
            DuplicatedMessage = 5,
            InvalidCredentials = 6,
            EmptyRequest = 7,
            ConnectionFail = 8
        };

        public bool AuthStatus { get; private set; }

        public string Result { get; private set;}
        public ResponseStatus Status { get; private set; }


        HttpWebResponse HttpResponse;
        CookieCollection Cookies = new CookieCollection();

        readonly Configuration Conf;
        
        readonly SHA1CryptoServiceProvider Sha1 = new SHA1CryptoServiceProvider();
        readonly UTF8Encoding UniEncoding = new UTF8Encoding();

        public RfidWebClient(Configuration conf)
        {
            Conf = conf;
            if (Conf.IsRegistered == false)
            {
                Signup();
            }
        }

        public void SendRfidReports(List<RfidSession> unshippedSessions)
        {
            if (AuthStatus == false)
            {
                Status = ResponseStatus.ConnectionFail;
                return;
            }
            foreach (var unshippedSession in unshippedSessions)
            {
                //Если дубликат, тоже не отправлять.
                SendRfidReport(unshippedSession);
                if (Status == ResponseStatus.Ok || Status == ResponseStatus.DuplicatedMessage)
                {
                    unshippedSession.deliveryStatus = RfidSession.DeliveryStatus.Shipped;
                }
            }
        }

        //Перед отсылкой надо сделать аутентификацию и получить куки
        public void Auth()
        {
            var post = "login=" + Conf.Login + "&pass=" + Conf.Pass;

            SendPostData(Conf.Server + "/rfid/auth/", post);

            if (Status == ResponseStatus.Ok)
            {
                Cookies = HttpResponse.Cookies;
                var cookie = Cookies["session_id"];
                if (cookie != null) cookie.Expires = DateTime.MaxValue;

                AuthStatus = true;
            }
        }

        //Регистрация
        void Signup()
        {
            var post = "login=" + Conf.Login + "&pass=" + Conf.Pass;
            var url = Conf.Server + "/rfid/signup/";

            SendPostData(url, post);

            if (Status == ResponseStatus.Ok)
            {
                Conf.Pass = Result;
                Conf.IsRegistered = true;
                Conf.Serialize();
            }
        }

        //Отсылаем сессию данных
        void SendRfidReport(RfidSession session)
        {
            var jsonHashString = String.Empty;
            var jsonString = JsonConvert.SerializeObject(session);

            var inHashBytes = UniEncoding.GetBytes(jsonString);
            var outHashBytes = Sha1.ComputeHash(inHashBytes);
            jsonHashString = outHashBytes.Aggregate(jsonHashString, (current, b) => current + b.ToString("x2"));

            var post = "json=" + jsonString + "&checksum=" + jsonHashString;
            SendPostData(Conf.Server + "/rfid/post/", post);
        }

        void ProcessResponse(HttpWebResponse response)
        {
            HttpResponse = response;
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>((new StreamReader(response.GetResponseStream())).ReadToEnd());
            Result = dict["result"];
            int Status;
            if (dict["error"] == null)
            {
                this.Status = ResponseStatus.Ok;
            }
            else if (Int32.TryParse(dict["error"], out Status))
            {
                this.Status = (ResponseStatus)Status; 
            }
            else
            {
                this.Status = ResponseStatus.InternalServerError;
            }
        }

        void SendPostData(string url, string postData)
        {
            var byteArray = Encoding.UTF8.GetBytes(postData);
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Proxy = null; //в противном случае webRequest начинает поиск прокси и тратит кучу времени
            webRequest.Method = "POST";
            webRequest.AllowAutoRedirect = false;
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = byteArray.Length;
            webRequest.CookieContainer = new CookieContainer();
            webRequest.CookieContainer.Add(Cookies);
            try
            {
                var webpageStream = webRequest.GetRequestStream();
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
            catch
            {
                Status = ResponseStatus.ConnectionFail;
                // throw new WebException();
            }
        }
    }
}
