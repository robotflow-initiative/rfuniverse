using System;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace RFUniverse
{

    public class RFUniverseCommunicatorTCP : RFUniverseCommunicatorBase
    {
        TcpClient client;
        NetworkStream stream;

        public override bool Connected => client != null ? client.Connected : false;

        public RFUniverseCommunicatorTCP(string host = "localhost", int port = 5004, int clientTime = 30, Action onConnected = null)
        {
            Debug.Log($"Connecting to server on port: {port}");

            client = new TcpClient();
            client.SendTimeout = 0;
            client.ReceiveTimeout = 0;
            client.NoDelay = true;

            link = new Thread((Link) =>
            {
                int connectCount = 0;
                while (!Connected && connectCount < clientTime)
                {
                    connectCount++;
                    try
                    {
                        client.Connect(host, port);
                    }
                    catch
                    {
                        Debug.Log($"Try Connection failed {connectCount}.");
                        Thread.Sleep(1000);
                    }
                }
                if (Connected)
                {
                    stream = client.GetStream();
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        onConnected?.Invoke();
                    });
                }
                else
                {
                    Debug.Log("Connection timeout.");
                    client.Close();
                    client.Dispose();
                    client = null;
                }
            });
            link.Start();
        }

        protected override byte[] ReceiveBytes()
        {
            byte[] lengthBuffer = ReceiveBytesTCP(4);
            uint length = BitConverter.ToUInt32(lengthBuffer);
            return ReceiveBytesTCP((int)length);
        }

        byte[] ReceiveBytesTCP(int length)
        {
            byte[] buffer = new byte[length];
            try
            {
                int lengthOffset = 0;
                while (lengthOffset < buffer.Length && Connected)
                {
                    int readLength = stream.Read(buffer, lengthOffset, buffer.Length - lengthOffset);
                    if (readLength == 0)
                        throw new Exception("Disconnected from server.");
                    lengthOffset += readLength;
                }
                return buffer;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                OnDisconnect?.Invoke();
            }
            return buffer;
        }
        protected override void SendBytes(byte[] bytes)
        {
            try
            {
                byte[] length = BitConverter.GetBytes(bytes.Length);
                stream.Write(length, 0, length.Length);
                stream.Write(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                OnDisconnect?.Invoke();
            }
        }
        public override void Dispose()
        {
            client?.Close();
            client?.Dispose();
            client = null;
            base.Dispose();
        }
    }
}
