using Robotflow.RFUniverse.SideChannels;


namespace RFUniverse.Manager
{
    public abstract class BaseManager
    {
        public BaseManager(string channel_id)
        {
            channel = new InfoChannel(this, channel_id);
            SideChannelManager.RegisterSideChannel(channel);
        }
        protected InfoChannel channel;
        public virtual void ReceiveData(IncomingMessage msg) { }
    }
}
