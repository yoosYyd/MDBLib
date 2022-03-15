using MDBLib.HEADERS;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MDBLib
{
    internal class SerialComs
    {
        private SerialPort port = new SerialPort();
        private SerialSettings settings;
        private int transmissionEndIntervalMS;
        private long lastRead = 0;
        private List<byte> readBuffer = new List<byte>();
        public string portID { get; } = "";
        private void Port_DataReceiver(object sender, SerialDataReceivedEventArgs e)
        {
            int toRead = port.BytesToRead;
            byte[] buff = new byte[toRead];
            port.Read(buff, 0, toRead);
            readBuffer.AddRange(buff);
            lastRead = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            //Console.WriteLine("readBuffer.Count.ToString() " + readBuffer.Count.ToString());
        }
        private void ErrorHandler(object sender, SerialErrorReceivedEventArgs e)
        {
            Console.WriteLine("comm error: "+e.ToString());
        }
        public void InitPort()
        {
            if (port.IsOpen)
            {
                Console.WriteLine("[1]port.BreakState: " + port.BreakState.ToString());
                port.Close();
                Thread.Sleep(1000);
            }
            port = new SerialPort();
            port.PortName = settings.PortName;
            port.BaudRate = settings.BaudRate;
            port.DataBits = settings.DataBits;
            port.Parity = settings.Parity;
            port.StopBits = settings.stopBit;
            port.ReadTimeout = settings.ReadTimeout;
            port.WriteTimeout = settings.WriteTimeout;
            port.ReadBufferSize = 4096;

            port.DataReceived += new SerialDataReceivedEventHandler(Port_DataReceiver);
            port.ErrorReceived += new SerialErrorReceivedEventHandler(ErrorHandler);
            try
            {
                port.Open();
                if (port.IsOpen)
                {
                    Console.WriteLine(port.PortName + " OPEN");
                    Console.WriteLine("[1]port.BreakState: " + port.BreakState.ToString());
                }
            }
            catch (Exception ex)
            {
                ErrorsBuffer.Instance.AddError(ex.Message);
                Console.WriteLine(ex.Message);
            }
        }
        public SerialComs(SerialSettings settings, int transmissionEndIntervalMS=100)
        {
            this.transmissionEndIntervalMS = transmissionEndIntervalMS;
            this.settings = settings;
            portID = settings.PortName;
            InitPort();
        }
        public bool WriteDataRTU(byte[] data)
        {
            readBuffer.Clear();
            try
            {
                port.Write(data, 0, data.Length);
                return true;
            }
            catch (Exception ex)
            {
                ErrorsBuffer.Instance.AddError(ex.Message);
                Console.WriteLine(ex.Message);
            }
            return false;
        }
        public byte[] ReadDataRTU()
        {
            Thread.Sleep(15);
            lastRead = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            while ((DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastRead) < transmissionEndIntervalMS)
            {
                Console.WriteLine(DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastRead);
                Thread.Sleep(15);
            }
            return readBuffer.ToArray();
        }
    }
    internal class TCPComs
    {
        private TcpClient client = new TcpClient();
        private int timeout;
        private int port;
        private string server;

        protected void StartConn()
        {
            if (!client.Connected)
            {
                try
                {
                    client.Close();
                    client = new TcpClient();
                    client.ConnectAsync(server, port).Wait(timeout);
                    //client.Connect(server, port);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    ErrorsBuffer.Instance.AddError(ex.Message);
                }
            }
        }
        public TCPComs(string server, int port, int timeout)
        {
            this.timeout = timeout;
            this.port = port;
            this.server = server;
        }
        public void WriteDataTCP(byte[] data)
        {
            StartConn();
            try
            {
                NetworkStream stream = client.GetStream();
                stream.WriteTimeout = timeout;
                stream.Write(data, 0, data.Length);
                //stream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("WriteDataTCP: " + ex.Message);
                ErrorsBuffer.Instance.AddError(ex.Message);
            }
        }
        public byte[] ReadDataTCP()
        {
            int bytesRead = 0;
            List<byte> ret = new List<byte>();
            try
            {
                byte[] buffer = new byte[4096];
                NetworkStream stream = client.GetStream();
                stream.ReadTimeout = timeout;
                ushort baseLen = 6;
                ushort stopLen = 1024;
                while (bytesRead != 0 || bytesRead != stopLen)
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    Array.Resize<byte>(ref buffer, bytesRead);
                    ret.AddRange(buffer);
                    Array.Resize<byte>(ref buffer, 4096);
                    if (ret.Count >= baseLen && stopLen == 1024)
                    {
                        byte[] lenArr = new byte[2];
                        lenArr[0] = ret[5];
                        lenArr[1] = ret[4];
                        stopLen = (ushort)(BitConverter.ToUInt16(lenArr) + baseLen);
                        if (ret.Count == stopLen)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (bytesRead == 0)
                {
                    Console.WriteLine("ReadDataTCP: " + ex.Message);
                    ErrorsBuffer.Instance.AddError(ex.Message);
                }
            }
            return ret.ToArray();
        }
    }
    public class Commutation
    {
        private SerialComs serial = null;
        private TCPComs tcp = null;
        private bool isTCPSerialConvertor = false;
        private PDUComposer composer = new PDUComposer();
        public Commutation(SerialSettings serialSettings)
        {
            serial = new SerialComs(serialSettings, serialSettings.ReadTimeout);
        }
        public Commutation(string server, int port, int timeout, bool isTCPSerialConvertor = false)
        {
            this.isTCPSerialConvertor = isTCPSerialConvertor;
            tcp = new TCPComs(server, port, timeout);
        }
        public void WriteData(byte[] data, byte dev)
        {
            if (serial != null)
            {
                serial.WriteDataRTU(composer.ComposeRTUreq(dev, data));
            }
            if (tcp != null)
            {
                tcp.WriteDataTCP(composer.ComposeTCPreq(data));
            }
        }
        public byte[] ReadData()
        {
            byte[] ret = null;
            byte[] data = null;
            if (serial != null)
            {
                byte dev = 0;
                data = serial.ReadDataRTU();
                if (data.Length > 2)
                {
                    if(!composer.RestorePDUFromRTUreq(data, out dev, out ret))
                    {
                        ret = null;
                        Console.WriteLine("CRC ERROR !");
                        ErrorsBuffer.Instance.AddError("CRC ERROR: dev adr: "+dev.ToString()+" port: "+serial.portID);
                    }
                }
                else
                {
                    serial.InitPort();
                }
            }
            if (tcp != null)
            {
                data = tcp.ReadDataTCP();
                if (data.Length > 2)
                {
                    ret = composer.RestorePDUFromTCPreq(data);
                }
            }
            return ret;
        }
    }
}
