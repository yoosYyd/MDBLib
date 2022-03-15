# MDBLib
Modbus RTU/TCP Library(.NET5)
Fully implemented and debugged next functions: 3,6,16.USE at own risk and responsibility.
Example of use:
1.Init TCP variant
>Commutation comms = new Commutation("127.0.0.1", 502, 500);
>Modbus mdb = new Modbus(comms);

2.Init Serial variant
>SerialSettings serialSettings = new SerialSettings();
>serialSettings.BaudRate = 19200;
>serialSettings.DataBits = 8;
>serialSettings.Parity = System.IO.Ports.Parity.None;
>serialSettings.PortName = "COM1";
>serialSettings.ReadTimeout = 100;
>serialSettings.stopBit = System.IO.Ports.StopBits.One;
>serialSettings.WriteTimeout = 100;
>Commutation comms = new Commutation(serialSettings);
>Modbus mdb = new Modbus(comms);

3.Data exchange with "slave"
>Console.WriteLine("SetINT32: " + mdb.SetINT32(5, -10000000).ToString());
>Console.Write("GetINT32 " + mdb.GetINT32(5, out s32val).ToString() + " ");
>Console.Write(s32val.ToString() + "\n");

where is:
bool Set*(register address,data value,optional parametr:"slave" address); return true if operation success 
bool Get*(register address,data value,optional parametr:"slave" address); return true if operation success

4.Retrieving errors buffer
>foreach(string errorMsg in mdb.GetErrorsList())
>{
>Console.WriteLine(errorMsg);
>}


