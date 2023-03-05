using System;
using System.Diagnostics;
using System.Threading;

using System.Device.I2c;
using System.Buffers.Binary;
using nanoFramework.M5Stack;
using Console = nanoFramework.M5Stack.Console;

namespace NF_Core2_SCD40
{
    public class Program
    {
        private static SpanByte start => new byte[] { 0x21, 0xb1 };         //スタートコマンド
        private static SpanByte read => new byte[] { 0xec, 0x05 };          //リードコマンド
        private static I2cDevice device;

        public static void Main()
        {

            M5Core2.InitializeScreen();
            Console.Clear();

            //SCD40
            device = M5Core2.GetGrove(0x62);

            //デバイスのスタート
            device.Write(start);

            Thread.Sleep(1000);

            //データ取得開始
            device.Write(read);

            Thread.Sleep(50);

            Timer timer = new Timer(getData, null, 0, 30000);        //30秒タイマー

            Thread.Sleep(Timeout.Infinite);

        }

        private static void getData(object state)
        {
            SpanByte buffer = new byte[9];

            device.Read(buffer);
            var co2 = BinaryPrimitives.ReadInt16BigEndian(buffer.Slice(0, 3));
            var tmp = -45 + 175 * (float)(BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(3, 3))) / 65536;
            var hum = 100 * (float)(BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(6, 3))) / 65536;

            Console.Clear() ;
            Console.CursorTop = 1;
            Console.CursorLeft = 0;
            Console.Write("Co2 : ");
            Console.WriteLine(co2.ToString());
            Console.Write("Tempreture : ");
            Console.WriteLine(tmp.ToString("F2"));
            Console.Write("Humidity : ");
            Console.WriteLine(hum.ToString("F2"));

            //アップロード用データ
            string data = $"{{\"co2\":{co2.ToString()},\"tmp\":{tmp.ToString("F2")},\"hum\":{hum.ToString("F2")}}}\r\n";
            Debug.Write(data);
        }
    }
}
