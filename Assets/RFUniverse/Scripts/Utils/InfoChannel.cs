using Robotflow.RFUniverse.SideChannels;
using RFUniverse.Manager;
namespace RFUniverse
{
    public class InfoChannel : SideChannel
    {
        BaseManager Manager = null;
        public InfoChannel(BaseManager manager, string uuid)
        {
            ChannelId = new System.Guid(uuid);
            Manager = manager;
        }
        protected override void OnMessageReceived(IncomingMessage msg)
        {
            Manager?.ReceiveData(msg);
        }
        public void SendMetaDataToPython(OutgoingMessage msg)
        {
            QueueMessageToSend(msg);
        }
    }
}
