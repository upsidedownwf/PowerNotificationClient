using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerNotificationClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("********************************************************************");
            Console.WriteLine("WELCOME TO POWERNOTIFICATIONCLIENT " + DateTime.Now);
            Console.WriteLine("********************************************************************");
            Console.WriteLine();
            var watch = new Stopwatch();
            watch.Start();
            var send = new SendNotification();
            var status = await send.SendNotificationasync();
            watch.Stop();
            if (status.status == "Success")
            {
                Console.WriteLine("Renewal Notification Mails Sent Successfully " + DateTime.Now);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Renewal Notification Failed:" + status.message);
                Console.WriteLine(DateTime.Now);
                Console.ResetColor();
            }
            Console.WriteLine();
            Console.WriteLine("TimeElapsed is " + watch.ElapsedMilliseconds);
            Console.ReadKey();
        }
    }
}
