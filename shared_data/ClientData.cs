using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace shared_data
{
    [Serializable]
    public class ClientData
    {
        public string Content { get; set; } = string.Empty;
        public string RequestCount { get; set; } = string.Empty;
        public HttpStatusCode Status { get; set; } 
    }
}
