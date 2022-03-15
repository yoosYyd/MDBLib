using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDBLib.HEADERS
{
    internal class MBAP
    {
        public byte[] mbap;
        private Random rnd = new Random();
        public MBAP(ushort pduLen)
        {
            ushort len = (ushort)(1 + pduLen);
            ushort protId = 0;
            ushort transId = (ushort)(1 +rnd.Next());
            byte unitID = (byte)(1 + rnd.Next());
            List<byte> buf = new List<byte>();
            buf.AddRange(BitConverter.GetBytes(transId));
            buf.AddRange(BitConverter.GetBytes(protId));
            byte[] lenArr = BitConverter.GetBytes(len);
            Array.Reverse(lenArr);
            buf.AddRange(lenArr);
            buf.Add(unitID);
            mbap = buf.ToArray();
        }
    }
    internal class PDUComposer
    {
        private ushort CalcMDBCrc(byte[] pdu)
        {
            UInt16 crc = 0xFFFF;
            for (int pos = 0; pos < pdu.Length; pos++)
            {
                crc ^= (UInt16)pdu[pos];
                for (int i = 8; i != 0; i--)
                {
                    if ((crc & 0x0001) != 0)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                        crc >>= 1;
                }
            }
            return crc;
        }
        public byte[] ComposeRTUreq(byte devAdr, byte[] pdu)
        {
            List<byte> ret = new List<byte>();
            ret.Add(devAdr);
            ret.AddRange(pdu);
            byte[] crc = BitConverter.GetBytes(CalcMDBCrc(ret.ToArray()));
            ret.AddRange(crc);
            return ret.ToArray();
        }
        public bool RestorePDUFromRTUreq(byte[] data,out byte devAdr,out byte[] pdu)
        {
            byte[] buf = new byte[data.Length - 2];
            Array.Copy(data, buf, data.Length - 2);
            devAdr = buf[0];
            byte[] crc = BitConverter.GetBytes(CalcMDBCrc(buf));
            buf = new byte[data.Length - 3];
            Array.Copy(data, 1, buf,0, data.Length - 3);
            pdu = buf;
            return crc[0] == data[data.Length - 2] && crc[1] == data[data.Length - 1];
        }
        public byte[] ComposeTCPreq(byte[] pdu)
        {
            List<byte> buf = new List<byte>();
            byte[] mbap = new MBAP((ushort)pdu.Length).mbap;
            buf.AddRange(mbap);
            buf.AddRange(pdu);
            return buf.ToArray();
        }
        public byte[] RestorePDUFromTCPreq(byte[] data)
        {
            byte[] ret = new byte[data.Length-7];
            Array.Copy(data, 7, ret, 0, data.Length - 7);
            return ret;
        }
    }
}
