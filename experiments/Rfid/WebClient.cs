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
    /// <summary>
    /// Используется модификация стандарта JSON-RPC:
    ///     В result записывается некие данные, которые возвращает сервер в ответ на запрос; 
    ///     В error записывается либо null, если всё прошло нормально, либо же код ошибки.
    ///     В последнем случае в result записывается возможное словесное описание ошибки. 
    /// </summary>
    struct RpcResponse
    {
        public string Result;
        public StatusCode Status;

        /// <summary>
        /// Перечисление определяет возможный статус запроса, возвращаемый сервером.
        /// InternalServerError и ConnectionFail могут
        /// быть установлены самим клиентом в SendPostData
        /// </summary>
        public enum StatusCode
        {
            Ok = 0,
            InternalServerError = 1,
            InvalidDeviceKey = 2,
            CorruptedChecksum = 3,
            CorruptedFormat = 4,
            DuplicatedMessage = 5,
            EmptyRequest = 6,
            ConnectionFail = 7
        };
    }

    /// <summary>
    /// Класс, отвечающий за посылку данных считывания на сервер
    /// </summary>
    public class RfidWebClient : WebClient
    {
        /// <summary>
        /// Определяет настройки подключения: сервер, ключ устройства.
        /// Смотри файл config.xml.
        /// </summary>
        readonly Configuration Configuration;

        readonly SHA1CryptoServiceProvider Sha1 = new SHA1CryptoServiceProvider();
        readonly UTF8Encoding UniEncoding = new UTF8Encoding();

        /// <summary>
        ///  Инициализация вебклиента
        /// </summary>
        /// <param name="conf"></param>
        public RfidWebClient(Configuration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Посылка сессий считывания
        /// </summary>
        /// <param name="unshippedSessions">Список сессий считывания со статусом Session.DeliveryStatus.Unshipped.
        /// Список заполняется TagsCollector.GetUnshippedTags()</param>
        public void SendRfidReports(List<RfidSession> unshippedSessions)
        {
            var url = "post/" + Configuration.DeviceKey + "/";

            foreach (var session in unshippedSessions)
            {
                var jsonHashString = String.Empty;
                var jsonString = JsonConvert.SerializeObject(session);

                var inHashBytes = UniEncoding.GetBytes(jsonString);
                var outHashBytes = Sha1.ComputeHash(inHashBytes);

                jsonHashString = outHashBytes.Aggregate(jsonHashString, (current, b) => current + b.ToString("x2"));

                var response = SendPostData(url, "json=" + jsonString + "&checksum=" + jsonHashString);

                //Если дубликат, тоже не отправлять.
                if (response.Status == RpcResponse.StatusCode.Ok || response.Status == RpcResponse.StatusCode.DuplicatedMessage)
                {
                    session.deliveryStatus = RfidSession.DeliveryStatus.Shipped;
                }
            }
        }

        /// <summary>
        /// Отправка и обработка данных. Обёртка над UploadString.
        /// </summary>
        /// <param name="url">Метод</param>
        /// <param name="data">Параметры</param>
        RpcResponse SendPostData(string url, string data)
        {
            var response = new RpcResponse();

            try
            {
                var client = new WebClient();
                client.BaseAddress = Configuration.Server;
                client.Headers["Content-type"] = "application/x-www-form-urlencoded";
                client.Proxy = null; //в противном случае webRequest начинает поиск прокси и тратит кучу времени
                
                var responseStr = client.UploadString(url, data);

                var responseJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseStr);
                response.Result = responseJson["result"];

                if (responseJson["error"] == null)
                {
                    response.Status = RpcResponse.StatusCode.Ok;
                }
                else
                {
                    response.Status = (RpcResponse.StatusCode)Int32.Parse(responseJson["error"]);
                }

                return response;
            }
            catch (Exception e)
            {
                response.Result = e.Message;
                response.Status = RpcResponse.StatusCode.InternalServerError;
                return response;
            }
        }
    }
}
