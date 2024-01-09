using System.Collections.Generic;

namespace RFUniverse
{
    public interface ICollectData
    {
        Dictionary<string, object> CollectData()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            AddPermanentData(data);
            AddTemporaryData(data);
            return data;
        }
        void AddPermanentData(Dictionary<string, object> data);
        void AddTemporaryData(Dictionary<string, object> data)
        {
            if (TemporaryData == null)
                TemporaryData = new Dictionary<string, object>();
            foreach (var item in TemporaryData)
            {
                data[item.Key] = item.Value;
            }
            TemporaryData.Clear();
        }
        Dictionary<string, object> TemporaryData { get; set; }
        void AddDataNextStep(string name, object data)
        {
            if (TemporaryData == null)
                TemporaryData = new Dictionary<string, object>();
            TemporaryData[name] = data;
        }
    }
}
