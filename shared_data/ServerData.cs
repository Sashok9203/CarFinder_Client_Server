using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared_data
{
    [Serializable]
    public class ServerData
    {
        public string CarFindString { get; set; } = string.Empty;

        public bool IsCarNumber { get; set; }

       
    }
}
