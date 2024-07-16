using Grpc.Core;
using System;
using System.Threading;
using UnityEngine;

namespace RFUniverse
{

    public class RFUniverseCommunicatorGRPC : RFUniverseCommunicatorBase
    {
        AsyncClientStreamingCall<BinaryMessage, Empty> grpcSendCall;
        AsyncServerStreamingCall<BinaryMessage> grpcReceiveCall;

        public override bool Connected => grpcConnected;
        bool grpcConnected = false;

        public RFUniverseCommunicatorGRPC(string host = "localhost", int port = 5004, int clientTime = 30, Action onConnected = null)
        {
            Debug.Log($"Connecting to server on port: {port}");

            link = new Thread((Link) =>
            {
                Channel channel = new Channel(host, port, ChannelCredentials.Insecure);
                //var channel = GrpcChannel.ForAddress($"https://{host}:{port}", new GrpcChannelOptions
                //{
                //    HttpHandler = new YetAnotherHttpHandler()
                //});
                GrpcService.GrpcServiceClient grpcClient = new GrpcService.GrpcServiceClient(channel);

                int connectCount = 0;
                while (!Connected && connectCount < clientTime)
                {
                    connectCount++;
                    try
                    {
                        var response = grpcClient.Link(new Empty());
                        grpcConnected = true;
                    }
                    catch
                    {
                        Debug.Log($"Try Connection failed {connectCount}.");
                        Thread.Sleep(1000);
                    }
                }
                if (Connected)
                {
                    grpcReceiveCall = grpcClient.PythonToCSharpStream(new Empty());
                    grpcSendCall = grpcClient.CSharpToPythonStream();
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        onConnected?.Invoke();
                    });
                }
                else
                {
                    Debug.Log("Connection timeout.");
                    channel.ShutdownAsync();
                }
            });
            link.Start();
        }

        protected override byte[] ReceiveBytes()
        {
            try
            {
                grpcReceiveCall.ResponseStream.MoveNext().GetAwaiter().GetResult();
                BinaryMessage response = grpcReceiveCall.ResponseStream.Current;
                return response.Data.ToByteArray();
            }
            catch (Exception e)
            {
                grpcConnected = false;
                OnDisconnect?.Invoke();
                return null;
            }
        }

        protected override void SendBytes(byte[] bytes)
        {
            try
            {
                grpcSendCall.RequestStream.WriteAsync(new BinaryMessage { Data = Google.Protobuf.ByteString.CopyFrom(bytes) }).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                grpcConnected = false;
                OnDisconnect?.Invoke();
            }
        }

        public override void Dispose()
        {
            grpcSendCall?.RequestStream.CompleteAsync().GetAwaiter().GetResult();
            link?.Abort();
            base.Dispose();
        }
    }
}
