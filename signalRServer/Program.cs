using Microsoft.Owin.Hosting;
using System;

namespace signalRServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //http://localhost:8080/
            //when using anything other than localhost, must run app as administrator
            using (WebApp.Start<Startup>("http://*:8080/"))
            {
                Console.WriteLine("Server running at http://*:8080/");
                Console.ReadLine();
            }
        }
    }
}
