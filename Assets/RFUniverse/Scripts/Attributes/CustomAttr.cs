using Robotflow.RFUniverse.SideChannels;

namespace RFUniverse.Attributes
{
    public class CustomAttr : BaseAttr
    {
        string data = "";
        //重写Type值
        //该值需要和pyrfuniverse中custom_attr下划线前半段相同 不区分大小写
        public override string Type
        {
            get { return "Custom"; }
        }
        //数据发送示例
        public override void CollectData(OutgoingMessage msg)
        {
            //先完成所继承的基类的数据写入
            base.CollectData(msg);
            //按顺序写入数据
            //此处写入顺序对应pyrfuniverse的custom_attr脚本parse_message函数中的读取顺序
            msg.WriteString(data);
        }
        //新增接口示例
        public override void AnalysisMsg(IncomingMessage msg, string type)
        {
            //根据头字符串分支
            switch (type)
            {
                case "CustomMessage":
                    CustomMessage(msg);
                    return;
            }
            base.AnalysisMsg(msg, type);
        }
        //接口实现
        void CustomMessage(IncomingMessage msg)
        {
            //按顺序读取数据
            //此处写入读取对应pyrfuniverse的custom_attr脚本CustomMessage函数中的写入顺序
            data = msg.ReadString();
        }
    }

}