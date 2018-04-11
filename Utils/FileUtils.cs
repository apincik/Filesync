using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataInfo;

namespace Utils
{
    /*
     *Helper methods. 
     */
    public class FileUtils
    {

        public static string getFilenameFromPath(string filePathName)
        {
            string[] array = filePathName.Split('\\');
            string lastFilename = array[array.Length - 1];

            //fix slashes
            if (lastFilename.Contains("/"))
            {
                array = filePathName.Split('/');
                return array[array.Length - 1];
            }

            return lastFilename;
        }


        public static string GetFullDirectoryPath(DirectoryInfo directory)
        {
            string path = "";
            while (true)
            {
                if (directory.Parent == null)
                {
                    break;
                }

                path = directory.Name + "/" + path;
                directory = directory.Parent;
            }

            return Path.Combine(directory.Name, path);//.Replace("\\", "//");
        }


        public static List<IDataInfo> DirectorySearch(string sDir)
        {

            List<IDataInfo> paths = new List<IDataInfo>();
            var files = Directory.GetFiles(sDir, "*", SearchOption.TopDirectoryOnly);
            var directories = Directory.GetDirectories(sDir, "*", SearchOption.TopDirectoryOnly);

            foreach (string path in files)
            {
                FileInfo info = new FileInfo(path);
                var fileDataInfo = new FileDataInfo(info.Name, (int)info.Length, path);
                paths.Add(fileDataInfo);
            }

            foreach (string directoryPath in directories)
            {
                DirectoryInfo info = new DirectoryInfo(directoryPath);
                var directoryDataInfo = new DirectoryDataInfo(info.Name, 0, directoryPath);
                List<IDataInfo> newPaths = DirectorySearch(directoryPath + "/");

                //add files & directories info to class sublist
                directoryDataInfo.subFiles = newPaths;
                paths.Add(directoryDataInfo);
            }

            return paths;
        }
    }
}
