using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using ReaderB;

namespace experiments
{
    class RfidTagReader
    {
        enum memoryMask : byte
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
        byte antennaSet = 0x03; // 
        int tagsAmount = 0;
        int EPClistLength = 0;
        
        public void GetTags(int interval, int count, ref List<string> tagsSet, out string timeStamp)
            {

            int connectionResult = StaticClassReaderB.OpenNetPort(port, host.ToString(), ref readerAdress, ref portReturned);
           // System.Console.WriteLine("The connection result is {0:X}", connectionResult);

            //Переводим китайское время в формат dd-mm-yy hh:mm:ss
            var timeStamp_ = new byte[6];
            var timeResult = StaticClassReaderB.GetTime(ref readerAdress, timeStamp_, portReturned);
            timeStamp = Convert.ToString(timeStamp_[2]) + '.' + Convert.ToString(timeStamp_[1]) + '.' + Convert.ToString(timeStamp_[0]) + ' ' +
            Convert.ToString(timeStamp_[3]) + ':' + Convert.ToString(timeStamp_[4]) + ':' + Convert.ToString(timeStamp_[5]);

            var EpcList = new byte[10000];

            for (int i = 0; i < count; i++)
            {
                int inventoryStatus = StaticClassReaderB.Inventory_G2(
                    ref readerAdress,
                    (byte)memoryMask.EPC,
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

                        var epcData = BitConverter.ToString(EpcList, j + 1, currentEpcLength);

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
         //   System.Console.WriteLine("The disconnection result is {0:X}", disconnectionResult);
        }
    }
}
