using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FTPClient;
using FTPSync;
using FSW;
using System.Threading;
using ConfigManager;
using System.Runtime.Remoting;
using System.IO;
using System.Reflection;

namespace Test
{
    class Program
    {

        static void Main(string[] args)
        {

            Config.ReadConfig("config.xml");

            var client = new Client(Config.FTPRootPath, Config.Username, Config.Pasword, Config.LocalRootPath.Replace("\\\\", "\\"));

            FTPSyncModule sync = new FTPSyncModule(client, Config.LocalRootPath.Replace("\\\\", "\\"));
            sync.Run();

            var watcher = new FileWatcher(Config.LocalRootPath, client);
            watcher.watch();
        }


        /*static void run()
        {
            var client = new Client("ftp://127.0.0.1/", "andrej", "2108", "d:\\ftpclient/");
            var watcher = new FileWatcher("d:\\ftpclient/", client);
            watcher.watch();

            while (true)
            {
                Thread.Sleep(100);
            }
        }*/
    }
}
