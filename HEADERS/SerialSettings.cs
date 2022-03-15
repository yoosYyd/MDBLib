using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDBLib.HEADERS
{
    public class SerialSettings
    {
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public System.IO.Ports.Parity Parity { get; set; }
        public System.IO.Ports.StopBits stopBit { get; set; }
        public int ReadTimeout { get; set; }
        public int WriteTimeout { get; set; }
    }
}
