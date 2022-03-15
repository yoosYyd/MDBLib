using MDBLib.HEADERS;
using System;
using System.Collections.Generic;

/*The MIT License
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), 
to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

namespace MDBLib
{
    internal class DataDecoder
    {
        public byte[] SwapArrayWords(byte[] arr)
        {
            List<byte> ret = new List<byte>();
            int i = 1;
            while(i<arr.Length)
            {
                ret.Add(arr[i]);
                ret.Add(arr[i - 1]);
                i += 2;
            }
            if (arr.Length%2>0)
            {
                ret.Add(0);
                ret.Add(arr[arr.Length - 1]);
            }
            return ret.ToArray();
        }
        private byte[] SwapTwoByteVal(byte[] val)
        {
            Array.Reverse(val);
            return val;
        }
        private byte[] SwapFourByteVal(byte[] val)
        {
            byte[] buf = new byte[val.Length];
            buf[0] = val[1];
            buf[1] = val[0];
            buf[2] = val[3];
            buf[3] = val[2];
            return buf;
        }
        private byte[] SwapEightByteVal(byte[] val)
        {
            byte[] buf = new byte[val.Length];
            buf[0] = val[1];
            buf[1] = val[0];
            buf[2] = val[3];
            buf[3] = val[2];

            buf[4] = val[5];
            buf[5] = val[4];
            buf[6] = val[7];
            buf[7] = val[6];
            return buf;
            //return val;
        }
        public short DecodeINT16(byte[] val)
        {
            return BitConverter.ToInt16(SwapTwoByteVal(val));
        }
        public ushort DecodeUINT16(byte[] val)
        {
            return BitConverter.ToUInt16(SwapTwoByteVal(val));
        }
        public int DecodeINT32(byte[] val)
        {
            return BitConverter.ToInt32(SwapFourByteVal(val));
        }
        public uint DecodeUINT32(byte[] val)
        {
            return BitConverter.ToUInt32(SwapFourByteVal(val));
        }
        public long DecodeINT64(byte[] val)
        {
            return BitConverter.ToInt64(SwapEightByteVal(val));
        }
        public ulong DecodeUINT64(byte[] val)
        {
            return BitConverter.ToUInt64(SwapEightByteVal(val));
        }
        public float DecodeFLOAT(byte[] val)
        {
            return BitConverter.ToSingle(SwapFourByteVal(val));
        }
        public double DecodeDOUBLE(byte[] val)
        {
            return BitConverter.ToDouble(SwapEightByteVal(val));
        }
        public byte[] EncodeUINT32(uint val)
        {
            return SwapFourByteVal(BitConverter.GetBytes(val));
        }
        public byte[] EncodeINT32(int val)
        {
            return SwapFourByteVal(BitConverter.GetBytes(val));
        }
        public byte[] EncodeUINT64(ulong val)
        {
            return SwapEightByteVal(BitConverter.GetBytes(val));
        }
        public byte[] EncodeINT64(long val)
        {
            return SwapEightByteVal(BitConverter.GetBytes(val));
        }
        public byte[] EncodeFLOAT(float val)
        {
            return SwapFourByteVal(BitConverter.GetBytes(val));
        }
        public byte[] EncodeDOUBLE(double val)
        {
            return SwapEightByteVal(BitConverter.GetBytes(val));
        }
        public byte[] EncodeUINT16(ushort val)
        {
            return SwapTwoByteVal(BitConverter.GetBytes(val));
        }
        public byte[] EncodeINT16(short val)
        {
            return SwapTwoByteVal(BitConverter.GetBytes(val));
        }
    }
    public class Modbus
    {
        private PDUReqSet pduReq = new PDUReqSet();
        private PDUResponSet pduResp = new PDUResponSet();
        private DataDecoder decoder = new DataDecoder();
        private Commutation comm;
        public Modbus(Commutation comm)
        {
            this.comm = comm;
        }
        public bool SetUINT16(ushort adr, ushort val, byte dev = 1)
        {
            byte[] pdu = pduReq.GetREQfunc06PDU(adr, BitConverter.ToUInt16(decoder.EncodeUINT16(val)));
            comm.WriteData(pdu, dev);
            pdu = comm.ReadData();
            if (pdu != null)
            {
                byte[] result = null;
                pduResp.GetDataFunc06(pdu, out result);
                if (result != null)
                {
                    return val == BitConverter.ToUInt16(result);
                }
            }
            return false;
        }
        public bool SetINT16(ushort adr, short val, byte dev = 1)
        {
            return SetUINT16(adr, (ushort)val, dev);
        }
        public bool SetUINT32(ushort adr, uint val, byte dev = 1)
        {
            byte[] pdu = pduReq.GetREQfunc16PDU(adr, 2, 4, decoder.EncodeUINT32(val));
            comm.WriteData(pdu, dev);
            pdu = comm.ReadData();
            if (pdu != null)
            {
                ushort first, count;
                pduResp.GetDataFunc16(pdu, out first, out count);
                return first == adr && count == 2;
            }
            return false;
        }
        public bool SetINT32(ushort adr, int val, byte dev = 1)
        {
            return SetUINT32(adr, (uint)val, dev);
        }
        public bool SetFLOAT(ushort adr, float val, byte dev = 1)
        {
            byte[] pdu = pduReq.GetREQfunc16PDU(adr, 2, 4, decoder.EncodeFLOAT(val));
            comm.WriteData(pdu, dev);
            pdu = comm.ReadData();
            if (pdu != null)
            {
                ushort first, count;
                pduResp.GetDataFunc16(pdu, out first, out count);
                return first == adr && count == 2;
            }
            return false;
        }
        public bool SetUINT64(ushort adr, ulong val, byte dev = 1)
        {
            byte[] pdu = pduReq.GetREQfunc16PDU(adr, 4, 8, decoder.EncodeUINT64(val));
            comm.WriteData(pdu, dev);
            pdu = comm.ReadData();
            if (pdu != null)
            {
                ushort first, count;
                pduResp.GetDataFunc16(pdu, out first, out count);
                return first == adr && count == 4;
            }
            return false;
        }
        public bool SetINT64(ushort adr, long val, byte dev = 1)
        {
            return SetUINT64(adr, (ulong)val, dev);
        }
        public bool SetDOUBLE(ushort adr, double val, byte dev = 1)
        {
            byte[] pdu = pduReq.GetREQfunc16PDU(adr, 4, 8, decoder.EncodeDOUBLE(val));
            comm.WriteData(pdu, dev);
            pdu = comm.ReadData();
            if (pdu != null)
            {
                ushort first, count;
                pduResp.GetDataFunc16(pdu, out first, out count);
                return first == adr && count == 4;
            }
            return false;
        }
        public bool SetARRAY(ushort adr, byte[] array, byte dev = 1)
        {
            if (array.Length < 256)
            {
                ushort wordsLen = (ushort)Math.Ceiling((float)(array.Length / 2));
                byte[] pdu = pduReq.GetREQfunc16PDU(adr, wordsLen, (byte)array.Length, decoder.SwapArrayWords(array));
                comm.WriteData(pdu, dev);
                pdu = comm.ReadData();
                if (pdu != null)
                {
                    ushort first, count;
                    pduResp.GetDataFunc16(pdu, out first, out count);
                    return first == adr && count == wordsLen;
                }
            }
            return false;
        }
        public bool GetArray(ushort adr, out byte[] array, byte arrayLen, byte dev = 1)
        {
            byte[] ret = null;
            int len = arrayLen;
            if (arrayLen % 2 != 0)
            {
                len = arrayLen / 2 + 1;
            }
            else
            {
                len = arrayLen / 2;
            }
            byte[] pdu = pduReq.GetREQfunc03PDU(adr, (ushort)len);
            comm.WriteData(pdu, dev);
            pdu = comm.ReadData();
            if (pdu != null)
            {
                Array.Reverse(pdu);
                Array.Resize<byte>(ref pdu, pdu.Length - 2);
                Array.Reverse(pdu);
                ret = decoder.SwapArrayWords(pdu);
            }
            array = ret;
            return ret != null;
        }
        public bool GetUINT16(ushort adr, out ushort val, byte dev = 1)
        {
            byte[] pdu = pduReq.GetREQfunc03PDU(adr, 1);
            comm.WriteData(pdu, dev);
            pdu = comm.ReadData();
            if (pdu != null)
            {
                byte[] res = null;
                if (pduResp.GetDataFunc03(pdu, out res))
                {
                    //val = decoder.DecodeUINT16(res);
                    val = BitConverter.ToUInt16(res);
                    return true;
                }
            }
            val = 0;
            return false;
        }
        public bool GetINT16(ushort adr, out short val, byte dev = 1)
        {
            ushort res = 0;
            bool ret = GetUINT16(adr, out res, dev);
            val = (short)res;
            return ret;
        }
        public bool GetUINT32(ushort adr, out uint val, byte dev = 1)
        {
            byte[] pdu = pduReq.GetREQfunc03PDU(adr, 2);
            comm.WriteData(pdu, dev);
            pdu = comm.ReadData();
            if (pdu != null)
            {
                byte[] res = null;
                if (pduResp.GetDataFunc03(pdu, out res))
                {
                    val = decoder.DecodeUINT32(res);
                    return true;
                }
            }
            val = 0;
            return false;
        }
        public bool GetINT32(ushort adr, out int val, byte dev = 1)
        {
            uint res = 0;
            bool ret = GetUINT32(adr, out res, dev);
            val = (int)res;
            return ret;
        }
        public bool GetFLOAT(ushort adr, out float val, byte dev = 1)
        {
            byte[] pdu = pduReq.GetREQfunc03PDU(adr, 2);
            comm.WriteData(pdu, dev);
            pdu = comm.ReadData();
            if (pdu != null)
            {
                byte[] res = null;
                if (pduResp.GetDataFunc03(pdu, out res))
                {
                    val = decoder.DecodeFLOAT(res);
                    return true;
                }
            }
            val = 0xDEADFFFF;
            return false;
        }
        public bool GetUINT64(ushort adr, out ulong val, byte dev = 1)
        {
            byte[] pdu = pduReq.GetREQfunc03PDU(adr, 4);
            comm.WriteData(pdu, dev);
            pdu = comm.ReadData();
            if (pdu != null)
            {
                byte[] res = null;
                if (pduResp.GetDataFunc03(pdu, out res))
                {
                    val = decoder.DecodeUINT64(res);
                    return true;
                }
            }
            val = 0;
            return false;
        }
        public bool GetINT64(ushort adr, out long val, byte dev = 1)
        {
            ulong res = 0;
            bool ret = GetUINT64(adr, out res, dev);
            val = (long)res;
            return ret;
        }
        public bool GetDOUBLE(ushort adr, out double val, byte dev = 1)
        {
            byte[] pdu = pduReq.GetREQfunc03PDU(adr, 4);
            comm.WriteData(pdu, dev);
            pdu = comm.ReadData();
            if (pdu != null)
            {
                byte[] res = null;
                if (pduResp.GetDataFunc03(pdu, out res))
                {
                    val = decoder.DecodeDOUBLE(res);
                    return true;
                }
            }
            val = 0xDEADDEADFFFFFFFF;
            return false;
        }
        public byte[] GetArray(ushort adr, ushort len, byte dev = 1)
        {
            if (len < 256)
            {
                byte[] pdu = pduReq.GetREQfunc03PDU(adr, len);
                comm.WriteData(pdu, dev);
                pdu = comm.ReadData();
                if (pdu != null)
                {
                    byte[] res = null;
                    if (pduResp.GetDataFunc03(pdu, out res))
                    {
                        return res;
                    }
                }
            }
            return null;
        }
        public List<string> GetErrorsList()
        {
            return ErrorsBuffer.Instance.GetErros();
        }
    }
}
