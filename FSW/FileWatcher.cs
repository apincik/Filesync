using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FTPClient;
using DataInfo;
using HashManager;
using Utils;

namespace FSW
{
    public class FileWatcher
    {
        public string watchedDirectoryPath { get; set; }

        public FileSystemWatcher watcher;

        private Client ftpClient;

        public FileWatcher(string directoryPath, Client client)
        {
            this.watchedDirectoryPath = directoryPath;
            this.ftpClient = client;
            watcher = new FileSystemWatcher();
        }

        public void watch()
        {
            try
            {
                watcher.Path = watchedDirectoryPath;
                watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                watcher.Filter = "*.*";
                watcher.IncludeSubdirectories = true;
                watcher.Changed += new FileSystemEventHandler(onChanged);
                watcher.Created += new FileSystemEventHandler(onCreated);
                watcher.Deleted += new FileSystemEventHandler(onDeleted);
                watcher.Renamed += onRenamed;

                watcher.EnableRaisingEvents = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("Filewatcher.Watch() exception, {0}", e.Message));
            }

            Console.WriteLine("Filewatcher has started.");
        }

        public void stop()
        {
            watcher.EnableRaisingEvents = false;
        }

        public void onChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                Console.WriteLine("FileWatcher - onChanged called.");
                
                FileInfo file = new FileInfo(e.FullPath);
                FileAttributes attr = file.Attributes;
                string directoryPath = FileUtils.GetFullDirectoryPath(file.Directory);

                Console.WriteLine("File in watched directory changed.");
                if ((attr & FileAttributes.Directory) != FileAttributes.Directory && file.Name.Contains("-hash") == false)
                {
                    //create hash file
                    watcher.EnableRaisingEvents = false;
                    Manager hash = new Manager(file.FullName);
                    hash.removeHash();
                    hash.writeHash();
                    watcher.EnableRaisingEvents = true;

                    //upload changes on FTP server
                    string uploadPath = directoryPath + file.Name;
                    string uploadHashPath = directoryPath + hash.getHashFilename(file.Name);

                    Task.Factory.StartNew(async () => await ftpClient.UploadFile(uploadPath));
                    Task.Factory.StartNew(async () => await ftpClient.UploadFile(uploadHashPath));
                    //ftpClient.UploadFile(uploadPath);
                    //ftpClient.UploadFile(uploadHashPath);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Exception thrown in fileWatcher onChanged method, {0}", ex.Message));
            }

        }

        public void onCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                Console.WriteLine("FileWatcher - onCreated called.");
                FileInfo file = new FileInfo(e.FullPath);
                FileAttributes attr = File.GetAttributes(file.FullName);
                string directoryPath = FileUtils.GetFullDirectoryPath(file.Directory);

                if ((attr & FileAttributes.Directory) != FileAttributes.Directory && file.Name.Contains("-hash") == false)
                {
                    //create hash file
                    watcher.EnableRaisingEvents = false;
                    Manager hash = new Manager(file.FullName);
                    hash.writeHash();
                    watcher.EnableRaisingEvents = true;

                    //upload on created
                    string uploadPath = directoryPath + file.Name;
                    string uploadHashPath = directoryPath + hash.getHashFilename(file.Name);
                    Task.Factory.StartNew(async () => await ftpClient.UploadFile(uploadPath));
                    Task.Factory.StartNew(async () => await ftpClient.UploadFile(uploadHashPath));
                    //ftpClient.UploadFile(uploadPath);
                    //ftpClient.UploadFile(uploadHashPath);
                }
                else if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    string directoryMakePath = directoryPath + file.Name;
                    Task.Factory.StartNew(async () => await ftpClient.MakeDirectory(directoryMakePath));
                    //ftpClient.MakeDirectory(directoryMakePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Error during watcher event onCreated with message {0}", ex.Message));
            }
        }

        public void onDeleted(object sender, FileSystemEventArgs e)
        {
            try
            {
                Console.WriteLine("FileWatcher - onDeleted called.");

                FileInfo file = new FileInfo(e.FullPath);
                FileAttributes attr = file.Attributes; //File.GetAttributes(e.FullPath);
                string directoryPath = FileUtils.GetFullDirectoryPath(file.Directory);

                if (file.Extension != "" && file.Name.Contains("-hash") == false)
                {
                    //remove hash
                    watcher.EnableRaisingEvents = false;
                    Manager hash = new Manager(file.FullName);
                    hash.removeHash();
                    watcher.EnableRaisingEvents = true;

                    //remove files on server
                    string deletePath = directoryPath + file.Name;
                    string deleteHashPath = directoryPath + hash.getHashFilename(file.Name);
                    Task.Factory.StartNew(async () => await ftpClient.DeleteFile(deletePath));
                    Task.Factory.StartNew(async () => await ftpClient.DeleteFile(deleteHashPath));
                    //ftpClient.DeleteFile(deletePath);
                    //ftpClient.DeleteFile(deleteHashPath);
                }
                else if (file.Extension == "")
                {
                    string deletePath = directoryPath + file.Name;
                    Task.Factory.StartNew(async () => await ftpClient.RemoveDirectory(deletePath));
                    //ftpClient.RemoveDirectory(deletePath);
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("File has been already deleted by system.");
            }
        }

        public void onRenamed(object sender, RenamedEventArgs e)
        {
            Console.WriteLine("FileWatcher - onRenamed called.");

            try
            {
                FileInfo file = new FileInfo(e.FullPath);
                FileAttributes attr = file.Attributes;

                if ((attr & FileAttributes.Directory) != FileAttributes.Directory && file.Name.Contains("-hash") == false)
                {
                    watcher.EnableRaisingEvents = false;                    //disable raising for hash, no need to check
                    Manager oldHash = new Manager(e.OldFullPath);
                    oldHash.removeHash();
                    Manager newHash = new Manager(e.FullPath);
                    newHash.writeHash();
                    watcher.EnableRaisingEvents = true;                     //enable raising back

                    //rename files on ftp server
                    string directoryPath = FileUtils.GetFullDirectoryPath(file.Directory);
                    string oldPathName = directoryPath + FileUtils.getFilenameFromPath(e.OldName);
                    string newPathName = file.Name;
                    string oldHashPath = directoryPath + FileUtils.getFilenameFromPath(oldHash.getHashFilename(e.OldName));
                    string newHashPath = newHash.getHashFilename(file.Name);
                    Task.Factory.StartNew(async () => await ftpClient.RenameFile(oldPathName, newPathName));
                    Task.Factory.StartNew(async () => await ftpClient.RenameFile(oldHashPath, newHashPath));
                    //ftpClient.RenameFile(oldPathName, newPathName);
                    //ftpClient.RenameFile(oldHashPath, newHashPath);                         //second parameter just name
                }
                else
                {
                    string directoryPath = FileUtils.GetFullDirectoryPath(file.Directory);
                    string oldPathName = e.OldName.Replace("\\", "/");      //fix directory slashes for path

                    string newPathName = file.Name;
                    Task.Factory.StartNew(async () => await ftpClient.RenameFile(oldPathName, newPathName));
                    //ftpClient.RenameFile(oldPathName, newPathName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Exception thrown in fileWatcher, onRenamed - {0}", ex.Message));
            }
        }

    }
}
