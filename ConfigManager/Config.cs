using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ConfigManager
{
    public static class Config
    {
        public static string XMLConfigFilePath { get; set; }
        public static string FTPRootPath { get; set; }
        public static string LocalRootPath { get; set; }
        public static string Username { get; set; }
        public static string Pasword { get; set; }

        public static void ReadConfig(string filePath)
        {
            XMLConfigFilePath = filePath;

            XmlDocument doc = null;
            try
            {
                doc = new XmlDocument();
                doc.Load(XMLConfigFilePath);

                //load values
                FTPRootPath = doc.DocumentElement.SelectSingleNode("/config/ftp_root_path").InnerText;
                LocalRootPath = doc.DocumentElement.SelectSingleNode("/config/local_root_path").InnerText;
                Username = doc.DocumentElement.SelectSingleNode("/config/user").InnerText;
                Pasword = doc.DocumentElement.SelectSingleNode("/config/password").InnerText;
            }
            catch (Exception e)
            {
                //could be handled
            }
        }

        public static bool WriteConfig(string filePath, string[] array)
        {
            XMLConfigFilePath = filePath;

            XmlDocument doc = null;
            try
            {
                doc = new XmlDocument();
                doc.Load(XMLConfigFilePath);

                //load values
                FTPRootPath = doc.DocumentElement.SelectSingleNode("/config/ftp_root_path").InnerText = array[0];
                LocalRootPath = doc.DocumentElement.SelectSingleNode("/config/local_root_path").InnerText = array[1];
                Username = doc.DocumentElement.SelectSingleNode("/config/user").InnerText = array[2];
                Pasword = doc.DocumentElement.SelectSingleNode("/config/password").InnerText = array[3];

                //save
                doc.Save(filePath);
            }
            catch (Exception e)
            {
                //could be handled
                return false;
            }

            return true;
        }
    }
}
