using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using ReaderB;

namespace DL6970.Rfid
{
    /// <summary>
    /// Класс для работы непосредственно с ридером RFID-меток
    /// </summary>
    class DL6970Reader
    {
        enum MemoryMask : byte
        {
            Epc = 1,
            Tid = 2,
            UserDefined = 3,
        }

        readonly IPAddress host = IPAddress.Parse("192.168.0.250");
        readonly int port = 27011;
        byte readerAdress = 0xFF; //comAdress
        int portReturned;

        readonly byte[] maskData = new byte[32]; // maskLen / 8
        readonly byte[] maskAddr = new byte[2]; //address of the start bit
        readonly byte maskLength = 0; //length of Mask
        readonly byte maskFlag = 0; // 0x00 - disable; 0x01 enabled;

        readonly byte tidAddr = 0; //address of the TID's start bit
        readonly byte tidLength = 0; // length of TID
        readonly byte tidFlag = 0; // 1 - TID; 0 - EPC
        byte antennaSet; // 

        int tagsAmount;
        int EPClistLength;

        public DL6970Reader()
        {
            var connectionResult = StaticClassReaderB.OpenNetPort(port, host.ToString(), ref readerAdress, ref portReturned);
            StaticClassReaderB.SetBeepNotification(ref readerAdress, 0, portReturned);    
        }

        public void CloseConnection()
        {
            StaticClassReaderB.CloseNetPort(portReturned);
        }

        public void GetTags(int interval, int count, ref List<string> tagsSet, out string timeMarker)
        {    

            //У стационарного считывателя отстаёт время
            //Берём самостоятельно. <Старый код см. в коммитах до 08.05.2012>
            timeMarker = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");

            var epcList = new byte[10000];

            for (var i = 0; i < count; i++)
            {
                var inventoryStatus = StaticClassReaderB.Inventory_G2(
                    ref readerAdress,
                    (byte)MemoryMask.Epc,
                    maskAddr,
                    maskLength,
                    maskData,
                    maskFlag,
                    tidAddr,
                    tidLength,
                    tidFlag,
                    epcList,
                    ref antennaSet,
                    ref EPClistLength,
                    ref tagsAmount,
                    portReturned);

                //Если считывание успешно
                if (inventoryStatus != 0xFB)
                {
                    // Console.WriteLine("{0} Inventory_G2 Status : {1:X2}\tAmount: {2:D3}\tEPC Length: {3:D3}\tAntennas: {4}", timeStr, inventoryStatus, tagsAmount, EPClistLength, antennaSet.ToString());
                    for (var j = 0; j < EPClistLength; j += epcList[j] + 1)
                    {
                        int currentEpcLength = epcList[j];

                        var epcData = BitConverter.ToString(epcList, j + 1, currentEpcLength).Replace("-", "");

                        if (tagsSet.Contains(epcData) == false)
                        {
                            tagsSet.Add(epcData);
                        }
                    }
                }

                if (inventoryStatus == 0x00)
                {
                    break;
                }
                Thread.Sleep(interval);
            }
        }
    }
}
