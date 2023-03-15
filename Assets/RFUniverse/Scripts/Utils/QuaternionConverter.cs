using Newtonsoft.Json;
using System;
using UnityEngine;
/// <summary>
/// 使Json.Net可以正确序列化或反序列化Unity中的Quaternion数据
/// </summary>
public class QuaternionConverter : JsonConverter
{
    public override bool CanRead => true;
    public override bool CanWrite => true;
    public override bool CanConvert(Type objectType)
    {
        return typeof(Quaternion) == objectType;
    }
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return objectType switch
        {
            var t when t == typeof(Quaternion) => JsonConvert.DeserializeObject<Quaternion>(serializer.Deserialize(reader).ToString()),
            _ => throw new Exception("Unexpected Error Occurred"),
        };
    }
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        switch (value)
        {
            case Quaternion v:
                writer.WritePropertyName("x");
                writer.WriteValue(v.x);
                writer.WritePropertyName("y");
                writer.WriteValue(v.y);
                writer.WritePropertyName("z");
                writer.WriteValue(v.z);
                writer.WritePropertyName("w");
                writer.WriteValue(v.w);
                break;
            default:
                throw new Exception("Unexpected Error Occurred");
        }
        writer.WriteEndObject();
    }
}