using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using ReaderB;

namespace rfid
{
    class DL6970Reader
    {
        enum MemoryMask : byte
        {
            EPC = 1,
            TID = 2,
            UserDefined = 3,
        }

        IPAddress host = IPAddress.Parse("192.168.0.250");
        int port = 27011;
        byte readerAdress = 0xFF; //comAdress
        int portReturned = 0;

        byte[] maskData = new byte[32]; // maskLen / 8
        byte[] maskAddr = new byte[2]; //address of the start bit
        byte maskLength = 0; //length of Mask
        byte maskFlag = 0; // 0x00 - disable; 0x01 enabled;

        byte tidAddr = 0; //address of the TID's start bit
        byte tidLength = 0; // length of TID
        byte tidFlag = 0; // 1 - TID; 0 - EPC
        byte antennaSet = 0; // 

        int tagsAmount = 0;
        int EPClistLength = 0;

        public void GetTags(int interval, int count, ref List<string> tagsSet, out string timeMarker)
        {
            int connectionResult = StaticClassReaderB.OpenNetPort(port, host.ToString(), ref readerAdress, ref portReturned);
            if (connectionResult != 0)
            {
                Trace.WriteLine(DateTime.Now.ToString() + "\tThe connection result is " + connectionResult);
            }

            StaticClassReaderB.SetBeepNotification(ref readerAdress, 0, portReturned);            

            //Переводим китайское время в формат dd-mm-yy hh:mm:ss
            var timeMarker_ = new byte[6];
            StaticClassReaderB.GetTime(ref readerAdress, timeMarker_, portReturned);
            //DL6970 cannot into Unix time
            //timeMarker = (new DateTime(2000 + Convert.ToInt32(timeMarker_[0]),
            //    Convert.ToInt32(timeMarker_[1]),
            //    Convert.ToInt32(timeMarker_[2]),
            //    Convert.ToInt32(timeMarker_[3]),
            //    Convert.ToInt32(timeMarker_[4]),
            //    Convert.ToInt32(timeMarker_[5])
            //    )).ToString("yyyy-MM-dd HH:mm:ss");
            timeMarker = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");

            var EpcList = new byte[10000];

            for (int i = 0; i < count; i++)
            {
                int inventoryStatus = StaticClassReaderB.Inventory_G2(
                    ref readerAdress,
                    (byte)MemoryMask.EPC,
                    maskAddr,
                    maskLength,
                    maskData,
                    maskFlag,
                    tidAddr,
                    tidLength,
                    tidFlag,
                    EpcList,
                    ref antennaSet,
                    ref EPClistLength,
                    ref tagsAmount,
                    portReturned);

                //Если считывание успешно
                if (inventoryStatus != 0xFB)
                {
                    // Console.WriteLine("{0} Inventory_G2 Status : {1:X2}\tAmount: {2:D3}\tEPC Length: {3:D3}\tAntennas: {4}", timeStr, inventoryStatus, tagsAmount, EPClistLength, antennaSet.ToString());
                    for (int j = 0; j < EPClistLength; j += EpcList[j] + 1)
                    {
                        int currentEpcLength = EpcList[j];

                        var epcData = BitConverter.ToString(EpcList, j + 1, currentEpcLength).Replace("-", "");

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

            int disconnectionResult = StaticClassReaderB.CloseNetPort(portReturned);
            //Console.WriteLine("The disconnection result is {0:X}", disconnectionResult);
        }
    }
}
