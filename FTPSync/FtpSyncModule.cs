using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataInfo;
using FTPClient;
using System.IO;
using HashManager;
using System.Threading;

namespace FTPSync
{
    //Sync class for syncing data from FTP server.
    public class FTPSyncModule
    {

        const int SYNC_DELAY = 1000 * 60 * 10;
        public Client client { get; set; }
        public string localRootPath { get; set; }
        public bool isRunning = false;
        public object _lock = new Object();

        public FTPSyncModule(Client client, string localRootPath)
        {
            this.client = client;
            this.localRootPath = localRootPath;
        }

        public void Run()
        {
            Thread thread = new Thread(new ThreadStart(StartSyncing));
            thread.Start();
        }

        public void StartSyncing()
        {
            isRunning = true;
            bool running = isRunning;

            while(running)
            {
                Thread.Sleep(SYNC_DELAY);
                SyncFromServer();

                lock (_lock)
                {
                    running = isRunning;
                }
            }
        }

        public void Stop()
        {
            lock(_lock)
            {
                isRunning = false;
            }
        }

        private void SyncFromServer()
        {
            Console.WriteLine("Sync from server has started.");
            List<IDataInfo> serverFiles = client.getDirectoryDetail();
            foreach (IDataInfo file in serverFiles)
            {
                SyncDirectory(file, localRootPath);
            }
        }

        private void SyncDirectory(IDataInfo fileInfo, string localPath)
        {
            string path = "";

            if (fileInfo is FileDataInfo)
            {
                //kontrola suboru
                FileDataInfo file = (FileDataInfo)fileInfo;
                path = Path.Combine(localPath, file.name);

                //if file does not exists on local, just download it
                if (File.Exists(path) == false)
                {
                    Console.WriteLine("File exists on server, but not on local client. Path: " + path);
                    Task.Factory.StartNew(async () => await client.DownloadFile(path, path));
                    //client.DownloadFile(path, path);
                }
                else
                {
                    //if file exists on local, check hash
                    if (file.name.Contains("-hash") == false)    //only real file, don't do hash for hash file
                    {
                        Manager hash = new Manager(path);                           //local hash
                        string hashFilename = hash.getHashFilename(path);
                        string remoteHashPath = localPath + hashFilename; //Path.Combine(localPath, hash.getHashFilename(path));
                        string remoteHashFileContent = client.DownloadFileContent(remoteHashPath);       //
                        string localHashFileContent = hash.getTextHash();

                        if (localHashFileContent != remoteHashFileContent)
                        {
                            Console.WriteLine(String.Format("File hash {0} is not the same with remote.", path));
                            Task.Factory.StartNew(async () => await client.DownloadFile(path, path));
                            Task.Factory.StartNew(async () => await client.DownloadFile(remoteHashPath, remoteHashPath));
                            //client.DownloadFile(path, path);
                            //client.DownloadFile(remoteHashPath, remoteHashPath);
                        }
                    }
                }
            }
            else
            {
                //folder scan
                DirectoryDataInfo dir = (DirectoryDataInfo)fileInfo;
                path = Path.Combine(localPath, dir.name);

                if (Directory.Exists(path) == false)
                {
                    Console.WriteLine("Dir exists on server, but not on local client. Path: " + path);
                    Directory.CreateDirectory(path);    //make directory
                }

                foreach (IDataInfo subFile in dir.subFiles)
                {
                    string newPath = Path.Combine(localPath, dir.name) + "\\";
                    SyncDirectory(subFile, newPath);
                }
            }
        }
    }
}
