using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataInfo;

namespace FTPClient
{
    public class Client
    {
        public string remoteHost { get; set; }
        public string remoteUser { get; set; }
        public string remotePassword { get; set; }

        public string localRootPath { get; set; }


        public Client(string host, string user, string password, string localRootPath)
        {
            remoteHost = host;
            remoteUser = user;
            remotePassword = password;
            this.localRootPath = localRootPath;
        }


        public FtpWebRequest GetFtpWebRequest(string filename = "")
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(remoteHost + filename);
            request.Credentials = new NetworkCredential(remoteUser, remotePassword);

            return request;
        }


        public List<string> ListRootDirectory()
        {
            List<string> result = new List<string>();

            try
            {
                FtpWebRequest request = GetFtpWebRequest();
                request.Method = WebRequestMethods.Ftp.ListDirectory;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    Stream responseStream = response.GetResponseStream();

                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        while (!reader.EndOfStream)
                        {
                            result.Add(reader.ReadLine());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown in FtpClient.ListRootDirectory()");
            }

            return result;
        }


        public async Task DownloadFile(string fullPathFilename, string destination)
        {
            try
            {
                //string file = fullPathFilename.Replace(localRootPath, "");
                string file = fullPathFilename.Replace("/", "\\");
                file = file.Replace(localRootPath, "");

                FtpWebRequest request = GetFtpWebRequest(file);
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                using (var response = request.GetResponse())
                {
                    Console.WriteLine(String.Format("{0} file has been downloaded.", fullPathFilename));
                    Stream responseStream = response.GetResponseStream();

                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        using (StreamWriter writer = new StreamWriter(destination))
                        {
                            writer.Write(reader.ReadToEnd());
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception thrown in FtpClient.DownloadFile");
                //return false;
            }

            //return true;
        }


        public string DownloadFileContent(string fullPathFilename)
        {
            try
            {
                //string file = fullPathFilename.Replace(localRootPath, "");
                string file = fullPathFilename.Replace("/", "\\");
                file = file.Replace(localRootPath, "");

                FtpWebRequest request = GetFtpWebRequest(file);
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                using (var response = request.GetResponse())
                {
                    Console.WriteLine(String.Format("{0} #HASH has been downloaded.", fullPathFilename));
                    Stream responseStream = response.GetResponseStream();

                    using (StreamReader reader = new StreamReader(responseStream, Encoding.Default))
                    {
                        //Encoding.Default.GetString(hash);
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("Exception thrown in FtpClient.DownloadFile, Message: {0}", e.Message));
                return "";
            }

        }

        public async Task UploadFile(string fullPathFilename)
        {
            try
            {
                //string filename = Path.GetFileName(FullPathFilename);
                //fullPathFilename = fullPathFilename.Replace("/", "\\");
                string filename = fullPathFilename.Replace("/", "\\");
                filename = filename.Replace(localRootPath, "");

                FtpWebRequest request = GetFtpWebRequest(filename);
                request.Method = WebRequestMethods.Ftp.UploadFile;

                using (StreamReader sourceStream = new StreamReader(fullPathFilename, Encoding.Default))
                {
                    byte[] fileContents = Encoding.Default.GetBytes(sourceStream.ReadToEnd());
                    request.ContentLength = fileContents.Length;

                    using (Stream requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(fileContents, 0, fileContents.Length);
                    }
                }

                using (var response = request.GetResponse())
                {
                    Console.WriteLine(String.Format("{0} file has been uploaded.", filename));
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(String.Format("Exception thrown in FtpClient.UploadFile. Message: {0}", e.Message));
                //return false;
            }

            //return true;
        }

        public async Task RenameFile(string originFilename, string newFilename)
        {
            try
            {
               // originFilename = originFilename.Replace(localRootPath, "");
                newFilename = newFilename.Replace(localRootPath, "");

                /*string filename = fullPathFilename.Replace("/", "\\");
                filename = filename.Replace(localRootPath, "");*/

                originFilename = originFilename.Replace("/", "\\");
                originFilename = originFilename.Replace(localRootPath, "");
                newFilename = newFilename.Replace("/", "\\");
                newFilename = newFilename.Replace(localRootPath, "");

                FtpWebRequest request = GetFtpWebRequest(originFilename);
                request.Method = WebRequestMethods.Ftp.Rename;
                request.RenameTo = newFilename;

                using(var response = request.GetResponse())
                {
                    Console.WriteLine(String.Format("{0} file has been renamed to {1}", originFilename, newFilename));
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(String.Format("FtpClient.RenameFile() exception - {0}", e.Message));
                //return false;
            }

            //return true;
        }


        public async Task DeleteFile(string fullPathFilename)
        {
            try
            {
                //string filename = fullPathFilename.Replace(localRootPath, "");
                string filename = fullPathFilename.Replace("/", "\\");
                filename = filename.Replace(localRootPath, "");

                FtpWebRequest request = GetFtpWebRequest(filename);
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                request.GetResponse();
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("FtpClient.DeleteFile() exception - {0}", e.Message));
                //return false;
            }

            //return true;
        }


        public async Task RemoveDirectory(string fullPathFilename)
        {
            try
            {
                //string filename = fullPathFilename.Replace(localRootPath, "");
                string filename = fullPathFilename.Replace("/", "\\");
                filename = filename.Replace(localRootPath, "");

                FtpWebRequest request = GetFtpWebRequest(filename);
                request.Method = WebRequestMethods.Ftp.RemoveDirectory;
                request.GetResponse();
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("FtpClient.RemoveDirectory() exception - {0}", e.Message));
                //return false;
            }

            //return true;
        }


        public async Task MakeDirectory(string fullPathDirectoryName)
        {
            try
            {
                //string filename = fullPathDirectoryName.Replace(localRootPath, "");
                string filename = fullPathDirectoryName.Replace("/", "\\");
                filename = filename.Replace(localRootPath, "");
                
                FtpWebRequest request = GetFtpWebRequest(filename);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                request.GetResponse();
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("FtpClient.MakeDirectory() exception - {0}", e.Message));
                //return false;
            }

            //return true;
        }


        public DateTime getDateTimestamp(string filename)
        {
            DateTime lastModified = new DateTime();
            try
            {
                FtpWebRequest request = GetFtpWebRequest(filename);
                request.Method = WebRequestMethods.Ftp.GetDateTimestamp;
                
                using(FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    lastModified = response.LastModified;
                }   
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("FtpClient.getDateTimestamp() exception - {0}", e.Message));             
            }

            return lastModified;
        }


        public List<IDataInfo> getDirectoryDetail(string dir = "")
        {

            List<IDataInfo> list = new List<IDataInfo>();

            try
            {
                FtpWebRequest request = GetFtpWebRequest(dir);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    Stream stream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(stream);

                    while(!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        string regex =
                                    @"^" +                          //# Start of line
                                    @"(?<dir>[\-ld])" +             //# File size          
                                    @"(?<permission>[\-rwx]{9})" +  //# Whitespace          \n
                                    @"\s+" +                        //# Whitespace          \n
                                    @"(?<filecode>\d+)" +
                                    @"\s+" +                        //# Whitespace          \n
                                    @"(?<owner>\w+)" +
                                    @"\s+" +                        //# Whitespace          \n
                                    @"(?<group>\w+)" +
                                    @"\s+" +                        //# Whitespace          \n
                                    @"(?<size>\d+)" +
                                    @"\s+" +                        //# Whitespace          \n
                                    @"(?<month>\w{3})" +            //# Month (3 letters)   \n
                                    @"\s+" +                        //# Whitespace          \n
                                    @"(?<day>\d{1,2})" +            //# Day (1 or 2 digits) \n
                                    @"\s+" +                        //# Whitespace          \n
                                    @"(?<timeyear>[\d:]{4,5})" +    //# Time or year        \n
                                    @"\s+" +                        //# Whitespace          \n
                                    @"(?<filename>(.*))" +          //# Filename            \n
                                    @"$";                           //# End of line


                            var split = new Regex(regex).Match(line);
                            //string sourceDir = split.Groups["dir"].ToString();
                            string fName = split.Groups["filename"].ToString();
                            int size = Int32.Parse(split.Groups["size"].ToString());
                            string actualDir = split.Groups["dir"].ToString();
                            bool isDirectory = !string.IsNullOrWhiteSpace(actualDir) && actualDir.Equals("d", StringComparison.OrdinalIgnoreCase);
                            IDataInfo fileInfo = null;    

                            //recursive walk in subdirectories
                            if (isDirectory == true)
                            {
                                DirectoryDataInfo directoryInfo = new DirectoryDataInfo(fName, size, "");
                                var subFilesList = getDirectoryDetail(dir + fName + "/");
                                directoryInfo.subFiles = subFilesList;
                                fileInfo = directoryInfo;
                            }
                            else
                            {
                                fileInfo = new FileDataInfo(fName, size, "");
                            }
                            
                            list.Add(fileInfo);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("FtpClient.getFileDetail() exception - {0}", e.Message));
            }

            return list;
        }
    }
}
