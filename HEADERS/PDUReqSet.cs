using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDBLib.HEADERS
{
    internal class PDUReqSet
    {
        private byte[] PrebuiltREQget(byte func,ushort adr,ushort count)
        {
            List<byte> buf = new List<byte>();
            buf.Add(func);
            byte[] adrArr = BitConverter.GetBytes(adr);
            Array.Reverse(adrArr);
            buf.AddRange(adrArr);
            byte[] countArr = BitConverter.GetBytes(count);
            Array.Reverse(countArr);
            buf.AddRange(countArr);
            return buf.ToArray();
        }
        public byte[] GetREQfunc01PDU(ushort firstCoil,ushort coilsCount)
        {
            return PrebuiltREQget(1, firstCoil, coilsCount);
        }
        public byte[] GetREQfunc02PDU(ushort firstCoil, ushort coilsCount)
        {
            return PrebuiltREQget(2, firstCoil, coilsCount);
        }
        public byte[] GetREQfunc03PDU(ushort firstWord, ushort wordsCount)
        {
            return PrebuiltREQget(3, firstWord, wordsCount);
        }
        public byte[] GetREQfunc04PDU(ushort firstWord, ushort wordsCount)
        {
            return PrebuiltREQget(4, firstWord, wordsCount);
        }
        public byte[] GetREQfunc05PDU(ushort coilAdr, bool val)
        {
            List<byte> buf = new List<byte>();
            buf.Add(5);
            byte[] adrArr = BitConverter.GetBytes(coilAdr);
            Array.Reverse(adrArr);
            buf.AddRange(adrArr);
            if(val)
            {
                buf.Add(0xFF);
                buf.Add(0);
            }
            else
            {
                buf.Add(0);
                buf.Add(0);
            }
            return buf.ToArray();
        }
        public byte[] GetREQfunc06PDU(ushort adr, ushort val)
        {
            return PrebuiltREQget(6, adr, val);
        }
        public byte[] GetREQfunc15PDU(ushort firstCoil, ushort coilsCount,byte followingDataLen,byte[] data)
        {
            List<byte> buf = new List<byte>();
            buf.AddRange(PrebuiltREQget(15, firstCoil, coilsCount));
            buf.Add(followingDataLen);
            buf.AddRange(data);
            return buf.ToArray();
        }
        public byte[] GetREQfunc16PDU(ushort firstWord, ushort wordsCount, byte followingDataLen, byte[] data)
        {
            List<byte> buf = new List<byte>();
            buf.AddRange(PrebuiltREQget(16, firstWord, wordsCount));
            buf.Add(followingDataLen);
            buf.AddRange(data);
            return buf.ToArray();
        }
    }
}
