using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HashProvider
{
    public class Hash
    {
        public byte[] computeHash(FileStream stream)
        {
            var md5 = MD5.Create();
            return md5.ComputeHash(stream);
        }
    }
}
