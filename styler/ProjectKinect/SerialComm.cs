using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Text;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

namespace ProjectKinect
{
    public class SerialComm
    {
        public delegate void DataReceivedHandlerFunc(byte[] receiveData);
        public DataReceivedHandlerFunc DataReceivedHandler;
        public delegate void DisconnectedHandlerFunc();
        public SerialPort serialPort;
        public static string strBuffer;

        public bool IsOpen
        {
            get
            {
                if (serialPort != null) return serialPort.IsOpen;
                return false;
            }
        }

        public bool OpenComm(string portName, int baudrate, int databits, StopBits stopbits, Parity parity, Handshake handshake)
        {
            try
            {
                serialPort = new SerialPort();

                serialPort.PortName = portName;
                serialPort.BaudRate = baudrate;
                serialPort.DataBits = databits;
                serialPort.StopBits = stopbits;
                serialPort.Parity = parity;
                serialPort.Handshake = handshake;

                serialPort.Encoding = new System.Text.ASCIIEncoding();
                serialPort.NewLine = "\r\n";
                serialPort.DataReceived += serialPort_DataReceived;

                serialPort.Open();

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }
        }

        public void CloseComm()
        {
            try
            {
                if (serialPort != null)
                {
                    serialPort.Close();
                    serialPort = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        public byte[] ReadSerialByteData()
        {
            serialPort.ReadTimeout = 100;
            byte[] bytesBuffer = new byte[serialPort.BytesToRead];
            int bufferOffset = 0;
            int bytesToRead = serialPort.BytesToRead;

            while (bytesToRead > 0)
            {
                try
                {
                    //버퍼를 읽고 해당 오프셋에 쓴다.
                    int readBytes = serialPort.Read(bytesBuffer, bufferOffset, bytesToRead - bufferOffset);

                    bytesToRead -= readBytes;
                    bufferOffset += readBytes;
                }
                catch (TimeoutException ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }

            return bytesBuffer;
        }

        public void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                byte[] bytesBuffer = ReadSerialByteData();
                strBuffer = Encoding.ASCII.GetString(bytesBuffer);

                if (DataReceivedHandler != null)
                    DataReceivedHandler(bytesBuffer);

                Debug.WriteLine("recceived(" + strBuffer.Length + ") : " + strBuffer);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                Console.WriteLine("예외발생");
            }
        }
    }
}

