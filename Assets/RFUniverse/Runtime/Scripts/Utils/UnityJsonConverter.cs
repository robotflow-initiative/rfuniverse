using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class UnityJsonConverter : JsonConverter
{
    public override bool CanConvert(System.Type objectType)
    {
        return objectType == typeof(Vector2) ||
               objectType == typeof(Vector2Int) ||
               objectType == typeof(Vector3) ||
               objectType == typeof(Vector3Int) ||
               objectType == typeof(Quaternion) ||
               objectType == typeof(Matrix4x4) ||
               objectType == typeof(Color);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        JObject obj = new JObject();

        if (value is Vector2 vector2)
        {
            obj.Add("x", vector2.x);
            obj.Add("y", vector2.y);
        }
        else if (value is Vector2Int vector2Int)
        {
            obj.Add("x", vector2Int.x);
            obj.Add("y", vector2Int.y);
        }
        else if (value is Vector3 vector3)
        {
            obj.Add("x", vector3.x);
            obj.Add("y", vector3.y);
            obj.Add("z", vector3.z);
        }
        else if (value is Vector3Int vector3Int)
        {
            obj.Add("x", vector3Int.x);
            obj.Add("y", vector3Int.y);
            obj.Add("z", vector3Int.z);
        }
        else if (value is Quaternion quaternion)
        {
            obj.Add("x", quaternion.x);
            obj.Add("y", quaternion.y);
            obj.Add("z", quaternion.z);
            obj.Add("w", quaternion.w);
        }
        else if (value is Matrix4x4 matrix4x4)
        {
            for (int i = 0; i < 16; i++)
            {
                obj.Add($"m{i / 4}{i % 4}", matrix4x4[i / 4, i % 4]);
            }
        }
        else if (value is Color color)
        {
            obj.Add("r", color.r);
            obj.Add("g", color.g);
            obj.Add("b", color.b);
            obj.Add("a", color.a);
        }

        obj.WriteTo(writer);
    }

    public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);

        if (objectType == typeof(Vector2))
        {
            return new Vector2((float)obj["x"], (float)obj["y"]);
        }
        else if (objectType == typeof(Vector2Int))
        {
            return new Vector2Int((int)obj["x"], (int)obj["y"]);
        }
        else if (objectType == typeof(Vector3))
        {
            return new Vector3((float)obj["x"], (float)obj["y"], (float)obj["z"]);
        }
        else if (objectType == typeof(Vector3Int))
        {
            return new Vector3Int((int)obj["x"], (int)obj["y"], (int)obj["z"]);
        }
        else if (objectType == typeof(Quaternion))
        {
            return new Quaternion((float)obj["x"], (float)obj["y"], (float)obj["z"], (float)obj["w"]);
        }
        else if (objectType == typeof(Matrix4x4))
        {
            Matrix4x4 matrix4x4 = new Matrix4x4();
            for (int i = 0; i < 16; i++)
            {
                matrix4x4[i / 4, i % 4] = (float)obj[$"m{i / 4}{i % 4}"];
            }
            return matrix4x4;
        }
        else if (objectType == typeof(Color))
        {
            return new Color((float)obj["r"], (float)obj["g"], (float)obj["b"], (float)obj["a"]);
        }

        return null;
    }
}