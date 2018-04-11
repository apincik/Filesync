using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Utils;
using HashProvider;
using System.Reflection;

namespace HashManager
{
    public class Manager
    {
        public string filePath { get; set; }

        public Manager(string filePath)
        {
            this.filePath = filePath;
        }

        private byte[] ComputeHashByReflectionProvider(FileStream stream)
        {
            //System.Diagnostics.Debugger.Break();

            byte[] byteArray = new byte[0];
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HashProvider.dll");
            Assembly assembly = Assembly.LoadFile(path);
            Type type = assembly.GetType("HashProvider.Hash");
            if (type != null)
            {
                MethodInfo methodInfo = type.GetMethod("computeHash", new Type[] { typeof(FileStream) });
                if (methodInfo != null)
                {
                    object result = null;
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    object classInstance = Activator.CreateInstance(type, null);
                    if (parameters.Length == 0)
                    {
                        result = methodInfo.Invoke(classInstance, null);
                    }
                    else
                    {
                        object[] parametersArray = new object[] { stream };            
                        result = methodInfo.Invoke(classInstance, parametersArray);

                        return (byte[]) result;
                    }
                }
            }

            return byteArray;
        }

        private byte[] computeHash()
        {
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        byte[] byteHashArray = ComputeHashByReflectionProvider(stream); //md5.ComputeHash(stream);
                        return byteHashArray;
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(String.Format("Error while computing hash for file {0}, error: {1}", filePath, e.Message));
                return null;
            }

        }


        public static byte[] computeHashByString(string fileContent)
        {
            try
            {
                using (var md5 = MD5.Create())
                {
                    byte[] encodedContent = new UTF8Encoding().GetBytes(fileContent);
                    byte[] byteHashArray = md5.ComputeHash(encodedContent);
                    return byteHashArray;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while computing hash for string");
                return null;
            }

        }

        public byte[] getHash()
        {
            return computeHash();
        }

        public string getTextHash()
        {
            byte[] hash = computeHash();

            if (hash == null)
            {
                return "";
            }

            return Encoding.Default.GetString(hash);
        }

        public string getHashFilename(string fileName)
        {
            string filename = fileName;
            string[] splittedName = filename.Split('.');
            string fileType = splittedName[splittedName.Length - 1];
            string newFilename = filename.Replace("." + fileType, "");
            newFilename = "." + FileUtils.getFilenameFromPath(newFilename) + "-hash" + ".txt";

            return newFilename;
        }

        public bool removeHash()
        {
            try
            {
                FileInfo file = new FileInfo(filePath);
                //byte[] hash = computeHash();
                string hashFilename = getHashFilename(file.Name);
                string hashFilePath = Path.Combine(file.Directory.FullName, hashFilename);

                if (File.Exists(hashFilePath))
                {
                    File.Delete(hashFilePath);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error writing hash into file.");
                return false;
            }

            return true;
        }

        public bool writeHash()
        {
            try
            {
                FileInfo file = new FileInfo(filePath);
                byte[] hash = computeHash();
                string hashFilename = getHashFilename(file.Name);
                string hashFilePath = Path.Combine(file.Directory.FullName, hashFilename);

                if (File.Exists(hashFilePath))
                {
                    File.Delete(hashFilePath);
                }

                File.WriteAllBytes(hashFilePath, hash);
            }
            catch(Exception e)
            {
                Console.WriteLine("Error writing hash into file.");
                return false;
            }

            return true;
        }
    }
}
