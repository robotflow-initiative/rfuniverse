using UnityEngine;
using Robotflow.RFUniverse.SideChannels;

namespace RFUniverse.Manager
{
    public class AssetManagerExt
    {
        //You can extend AssetManager here
        public void AnalysisMsg(IncomingMessage msg, string type)
        {
            switch (type)
            {
                //Switch to implementation based on message type
                case "CustomMessage":
                    CustomMessage(msg);
                    return;
            }
        }

        void CustomMessage(IncomingMessage msg)
        {
            //Read the message from python in order.
            //Note that the reading order here should align with 
            //the writing order in env.CustomMessage() of asset_channel_ext.py.
            string str = msg.ReadString();
            Debug.Log(str);


            //Send the received data back to python
            //1. Define an out-going message
            OutgoingMessage sendMsg = new OutgoingMessage();
            //2. The first data written must be the function name.
            sendMsg.WriteString("CustomMessage");
            //3. Write in the data.
            sendMsg.WriteString("this is asset channel custom message");
            AssetManager.Instance.SendMessage(sendMsg);
        }

    }
}
