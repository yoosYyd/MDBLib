using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDBLib.HEADERS
{
    internal class PDUResponSet
    {
        private bool NoErrorsCheck(byte[] pdu)//
        {
            string msg = "";
            if (IsTheErrorReq(pdu, out msg))
            {
                Console.WriteLine(msg);
                ErrorsBuffer.Instance.AddError(msg);
                return false;
            }
            return true;
        }
        private bool[] ConvertByteArrayToBoolArray(byte[] bytes)
        {
            System.Collections.BitArray b = new System.Collections.BitArray(bytes);
            bool[] bitValues = new bool[b.Count];
            b.CopyTo(bitValues, 0);
            //Array.Reverse(bitValues);
            return bitValues;
        }
        public bool GetDataFunc01(byte[] pdu,out bool[] data)
        {
            bool isOK = NoErrorsCheck(pdu);
            if (isOK)
            {
                byte[] buf = new byte[pdu.Length - 2];
                Array.Copy(pdu, 2, buf, 0, pdu.Length - 2);
                data = ConvertByteArrayToBoolArray(buf);
            }
            else
            {
                data = null;
            }
            return isOK;
        }
        public bool GetDataFunc02(byte[] pdu, out bool[] data)
        {
            return GetDataFunc01(pdu, out data);
        }
        public bool GetDataFunc03(byte[] pdu, out byte[] data)
        {
            bool isOK = NoErrorsCheck(pdu);
            if (isOK)
            {
                byte[] buf = new byte[pdu.Length - 2];
                Array.Copy(pdu, 2, buf, 0, pdu.Length - 2);
                data = buf;
            }
            else
            {
                data = null;
            }
            return isOK;
        }
        public bool GetDataFunc04(byte[] pdu, out byte[] data)
        {
            return GetDataFunc03(pdu, out data);
        }
        public bool GetDataFunc05(byte[] pdu, out bool data)
        {
            bool isOK = NoErrorsCheck(pdu);
            if (isOK)
            {
                data = pdu[pdu.Length - 2] == 0xFF;
            }
            else
            {
                data = false;
            }
            return isOK;
        }
        public bool GetDataFunc06(byte[] pdu, out byte[] data)
        {
            bool isOK = NoErrorsCheck(pdu);
            if (isOK)
            {
                byte[] buf = new byte[2];
                buf[0] = pdu[pdu.Length - 2];
                buf[1] = pdu[pdu.Length - 1];
                data = buf;
            }
            else
            {
                data = null;
            }
            return isOK;
        }
        public bool GetDataFunc15(byte[] pdu, out ushort firstCoilAdr,out ushort coilsWriten)
        {
            bool isOK = NoErrorsCheck(pdu);
            if (isOK)
            {
                byte[] buf = new byte[2];
                buf[0] = pdu[pdu.Length - 3];
                buf[1] = pdu[pdu.Length - 4];
                firstCoilAdr = BitConverter.ToUInt16(buf);
                buf[0] = pdu[pdu.Length - 1];
                buf[1] = pdu[pdu.Length - 2];
                coilsWriten = BitConverter.ToUInt16(buf);
            }
            else
            {
                firstCoilAdr = 0xFF;
                coilsWriten = 0xFF;
            }
            return isOK;
        }
        public bool GetDataFunc16(byte[] pdu, out ushort firstWordAdr, out ushort wordsWriten)
        {
            return GetDataFunc15(pdu, out firstWordAdr, out wordsWriten);
        }
        public bool IsTheErrorReq(byte[] pdu,out string msg)
        {
            msg = "";
            bool isit = pdu[0] >= 129 && pdu[0]<= 134 || pdu[0] == 143 || pdu[0] == 144;
            if(isit)
            {
                switch(pdu[1])
                {
                    case 1:
                        {
                            msg = "Illegal Function";
                        }break;
                    case 2:
                        {
                            msg = "Illegal Data Address";
                        }
                        break;
                    case 3:
                        {
                            msg = "Illegal Data Value";
                        }
                        break;
                    case 4:
                        {
                            msg = "Slave Device Failure";
                        }
                        break;
                    case 5:
                        {
                            msg = "Acknowledge";
                        }
                        break;
                    case 6:
                        {
                            msg = "Slave Device Busy";
                        }
                        break;
                    case 7:
                        {
                            msg = "Negative Acknowledge";
                        }
                        break;
                    case 8:
                        {
                            msg = "Memory Parity Error";
                        }
                        break;
                    case 10:
                        {
                            msg = "Gateway Path Unavailable";
                        }
                        break;
                    case 11:
                        {
                            msg = "Gateway Path Unavailable";
                        }
                        break;
                }
            }
            return isit;
        }
    }
}
