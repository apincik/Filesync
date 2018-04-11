using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using FTPClient;
using FTPSync;
using ConfigManager;
using FSW;
using System.Threading;
using System.Diagnostics;

namespace FTPService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Debugger.Launch();
            //set environment dir
            System.Environment.CurrentDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            
            //config
            Config.ReadConfig("config.xml");
            var client = new Client(Config.FTPRootPath, Config.Username, Config.Pasword, Config.LocalRootPath.Replace("\\\\", "\\"));

            FTPSyncModule sync = new FTPSyncModule(client, Config.LocalRootPath.Replace("\\\\", "\\"));
            sync.Run();

            var watcher = new FileWatcher(Config.LocalRootPath, client);
            watcher.watch();

            //Task.Factory.StartNew(async () => await run(client, Config.LocalRootPath));
            /*Thread thread = new Thread(() => run(client, Config.LocalRootPath));
            thread.Start(client);*/

        }

       /* public async Task run(Client client, string localPath)
        {
            var watcher = new FileWatcher(localPath, client);
            watcher.watch();

            while (true)
            {
                Thread.Sleep(100);
            }
        }*/

        protected override void OnStop()
        {
        }
    }
}
