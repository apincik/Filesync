using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInfo
{
    public class FileDataInfo : IDataInfo
    {
        public string path { get; set; }
        public string name { get; set; }
        public int size { get; set; }

        public FileDataInfo(string name, int size, string path)
        {
            this.name = name;
            this.size = size;
            this.path = path;
        }
    }
}
