using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using UnityEngine;

namespace RFUniverse
{
    public abstract class RFUniverseCommunicatorBase : IDisposable
    {
        protected Thread link;

        public Action<object[]> OnReceivedData;
        public Action OnDisconnect;

        public abstract bool Connected { get; }
        protected abstract byte[] ReceiveBytes();
        protected abstract void SendBytes(byte[] bytes);
        public virtual void Dispose()
        {
            link?.Abort();
        }

        public void SyncStepEnd()
        {
            if (!Connected) return;
            SendObject("StepEnd");
            Queue<object[]> syncReceiveObjectQueue = new Queue<object[]>();
            while (Connected)
            {
                byte[] bytes = ReceiveBytes();
                if (bytes == null) break;
                if (bytes.Length > 0)
                {
                    object[] data = ReceiveObjects(bytes);
                    if (data.Length > 0 && data[0] is string && data[0] as string == "StepStart")
                    {
                        break;
                    }
                    OnReceivedData?.Invoke(data);
                }
            }
        }

        int readOffset = 0;
        object[] ReceiveObjects(byte[] bytes)
        {
            readOffset = 0;
            List<object> data = new List<object>();
            int count = ReadInt(bytes);
            for (int i = 0; i < count; i++)
            {
                data.Add(ReadObject(bytes));
            }
            return data.ToArray();
        }

        object ReadObject(byte[] bytes)
        {
            string type = ReadString(bytes);
            switch (type)
            {
                case "none":
                case "null":
                    return null;
                case "int":
                    return ReadInt(bytes);
                case "float":
                    return ReadFloat(bytes);
                case "bool":
                    return ReadBool(bytes);
                case "string":
                    return ReadString(bytes);
                case "bytes":
                    return ReadBytes(bytes);
                case "list":
                    List<object> list = new List<object>();
                    int listCount = ReadInt(bytes);
                    for (int i = 0; i < listCount; i++)
                    {
                        list.Add(ReadObject(bytes));
                    }
                    return list;
                case "dict":
                    Dictionary<object, object> dict = new Dictionary<object, object>();
                    int length = ReadInt(bytes);
                    for (int i = 0; i < length; i++)
                    {
                        object key = ReadObject(bytes);
                        object value = ReadObject(bytes);
                        dict.Add(key, value);
                    }
                    return dict;
                case "array":
                    int rank = ReadInt(bytes);
                    int[] shape = new int[rank];
                    for (int i = 0; i < rank; i++)
                    {
                        shape[i] = ReadInt(bytes);
                    }
                    switch (rank)
                    {
                        case 1:
                            float[] array1d = new float[shape[0]];
                            for (int i = 0; i < shape[0]; i++)
                            {
                                array1d[i] = ReadFloat(bytes);
                            }
                            return array1d;
                        case 2:
                            float[,] array2d = new float[shape[0], shape[1]];
                            for (int i = 0; i < shape[0]; i++)
                            {
                                for (int j = 0; j < shape[0]; j++)
                                {
                                    array2d[i, j] = ReadFloat(bytes);
                                }
                            }
                            return array2d;
                        case 3:
                            float[,,] array3d = new float[shape[0], shape[1], shape[2]];
                            for (int i = 0; i < shape[0]; i++)
                            {
                                for (int j = 0; j < shape[0]; j++)
                                {
                                    for (int k = 0; k < shape[0]; k++)
                                    {
                                        array3d[i, j, k] = ReadFloat(bytes);
                                    }
                                }
                            }
                            return array3d;
                        default:
                            Debug.LogError($"array dont support rank: {rank}");
                            return null;
                    }
                case "tuple":
                    int tupleCount = ReadInt(bytes);
                    switch (tupleCount)
                    {
                        case 1:
                            return new Tuple<object>(ReadObject(bytes));
                        case 2:
                            return new Tuple<object, object>(ReadObject(bytes), ReadObject(bytes));
                        case 3:
                            return new Tuple<object, object, object>(ReadObject(bytes), ReadObject(bytes), ReadObject(bytes));
                        case 4:
                            return new Tuple<object, object, object, object>(ReadObject(bytes), ReadObject(bytes), ReadObject(bytes), ReadObject(bytes));
                        case 5:
                            return new Tuple<object, object, object, object, object>(ReadObject(bytes), ReadObject(bytes), ReadObject(bytes), ReadObject(bytes), ReadObject(bytes));
                        case 6:
                            return new Tuple<object, object, object, object, object, object>(ReadObject(bytes), ReadObject(bytes), ReadObject(bytes), ReadObject(bytes), ReadObject(bytes), ReadObject(bytes));
                        case 7:
                            return new Tuple<object, object, object, object, object, object, object>(ReadObject(bytes), ReadObject(bytes), ReadObject(bytes), ReadObject(bytes), ReadObject(bytes), ReadObject(bytes), ReadObject(bytes));
                        case 8:
                            return new Tuple<object, object, object, object, object, object, object, object>(ReadObject(bytes), ReadObject(bytes), ReadObject(bytes), ReadObject(bytes), ReadObject(bytes), ReadObject(bytes), ReadObject(bytes), ReadObject(bytes));
                        default:
                            Debug.LogError($"dont support tuple rank > 8");
                            return null;
                    }

                default:
                    Debug.LogError($"dont support this type: {type}");
                    return null;
            }
        }
        string ReadString(byte[] bytes)
        {
            int count = ReadInt(bytes);
            readOffset += count;
            return Encoding.UTF8.GetString(bytes.Skip(readOffset - count).Take(count).ToArray());
        }
        bool ReadBool(byte[] bytes)
        {
            readOffset += 1;
            return BitConverter.ToBoolean(bytes.Skip(readOffset - 1).Take(1).ToArray());
        }

        float ReadFloat(byte[] bytes)
        {
            readOffset += 4;
            return BitConverter.ToSingle(bytes.Skip(readOffset - 4).Take(4).ToArray());
        }
        int ReadInt(byte[] bytes)
        {
            readOffset += 4;
            return BitConverter.ToInt32(bytes.Skip(readOffset - 4).Take(4).ToArray());
        }

        byte[] ReadBytes(byte[] bytes)
        {
            int count = ReadInt(bytes);
            readOffset += count;
            return bytes.Skip(readOffset - count).Take(count).ToArray();
        }

        public void SendObject(params object[] datas)
        {
            if (!Connected) return;
            List<byte> bytes = new List<byte>();
            WriteInt(bytes, datas.Length);
            foreach (var data in datas)
            {
                WriteObject(bytes, data);
            }
            SendBytes(bytes.ToArray());
        }
        void WriteObject(List<byte> bytes, object data)
        {
            switch (data)
            {
                case null:
                    WriteString(bytes, "none");
                    break;
                case int i:
                    WriteString(bytes, "int");
                    WriteInt(bytes, i);
                    break;
                case float f:
                    WriteString(bytes, "float");
                    WriteFloat(bytes, f);
                    break;
                case bool b:
                    WriteString(bytes, "bool");
                    WriteBool(bytes, b);
                    break;
                case string s:
                    WriteString(bytes, "string");
                    WriteString(bytes, s);
                    break;
                case byte[] b:
                    WriteString(bytes, "bytes");
                    WriteBytes(bytes, b);
                    break;
                case Vector3 v3:
                    WriteString(bytes, "vector3");
                    WriteObject(bytes, new List<float>() { v3.x, v3.y, v3.z });
                    break;
                case Quaternion qua:
                    WriteString(bytes, "quaternion");
                    WriteObject(bytes, new List<float>() { qua.x, qua.y, qua.z, qua.w });
                    break;
                case Matrix4x4 mat:
                    WriteString(bytes, "matrix");
                    WriteObject(bytes, new float[4, 4] { { mat[0, 0], mat[0, 1], mat[0, 2], mat[0, 3] }, { mat[1, 0], mat[1, 1], mat[1, 2], mat[1, 3] }, { mat[2, 0], mat[2, 1], mat[2, 2], mat[2, 3] }, { mat[3, 0], mat[3, 1], mat[3, 2], mat[3, 3] } });
                    break;
                case Rect rect:
                    WriteString(bytes, "rect");
                    WriteFloat(bytes, rect.center.x);
                    WriteFloat(bytes, rect.center.y);
                    WriteFloat(bytes, rect.width);
                    WriteFloat(bytes, rect.height);
                    break;
                case Array arr:
                    WriteString(bytes, "array");
                    WriteInt(bytes, arr.Rank);
                    for (int i = 0; i < arr.Rank; i++)
                    {
                        WriteInt(bytes, arr.GetLength(i));
                    }
                    foreach (var item in arr)
                    {
                        WriteFloat(bytes, (float)item);
                    }
                    break;
                case IList lf:
                    WriteString(bytes, "list");
                    WriteInt(bytes, lf.Count);
                    foreach (var item in lf)
                    {
                        WriteObject(bytes, item);
                    }
                    break;
                case IDictionary dic:
                    WriteString(bytes, "dict");
                    WriteInt(bytes, dic.Count);
                    foreach (var item in dic.Keys)
                    {
                        WriteObject(bytes, item);
                        WriteObject(bytes, dic[item]);
                    }
                    break;
                case ITuple tup:
                    WriteString(bytes, "tuple");
                    WriteInt(bytes, tup.Length);
                    for (int i = 0; i < tup.Length; i++)
                    {
                        WriteObject(bytes, tup[i]);
                    }
                    break;
                default:
                    WriteString(bytes, "none");
                    Debug.LogError($"dont support this type: {data.GetType()}");
                    break;
            }
        }

        void WriteString(List<byte> bytes, string data)
        {
            byte[] bs = Encoding.UTF8.GetBytes(data);
            WriteInt(bytes, bs.Length);
            bytes.AddRange(bs);
        }
        void WriteBool(List<byte> bytes, bool data)
        {
            byte[] bs = BitConverter.GetBytes(data);
            bytes.AddRange(bs);
        }

        void WriteFloat(List<byte> bytes, float data)
        {
            byte[] bs = BitConverter.GetBytes(data);
            bytes.AddRange(bs);
        }
        void WriteInt(List<byte> bytes, int data)
        {
            byte[] bs = BitConverter.GetBytes(data);
            bytes.AddRange(bs);
        }

        void WriteBytes(List<byte> bytes, byte[] data)
        {
            WriteInt(bytes, data.Length);
            bytes.AddRange(data);
        }
    }
}
namespace Robotflow.RFUniverse.SideChannels
{
    public class IncomingMessage
    {
        byte[] buffer;
        int offset = 0;
        public IncomingMessage(byte[] bytes)
        {
            buffer = bytes;
        }
        public bool ReadBoolean()
        {
            offset += 1;
            return BitConverter.ToBoolean(buffer.Skip(offset - 1).Take(1).ToArray());
        }
        public int ReadInt32()
        {
            offset += 4;
            return BitConverter.ToInt32(buffer.Skip(offset - 4).Take(4).ToArray());
        }
        public float ReadFloat32()
        {
            offset += 4;
            return BitConverter.ToSingle(buffer.Skip(offset - 4).Take(4).ToArray());
        }
        public string ReadString()
        {
            int count = ReadInt32();
            offset += count;
            return Encoding.UTF8.GetString(buffer.Skip(offset - count).Take(count).ToArray());
        }
        public List<float> ReadFloatList()
        {
            List<float> list = new List<float>();
            int count = ReadInt32();
            for (int i = 0; i < count; i++)
            {
                list.Add(ReadFloat32());
            }
            return list;
        }
    }

    public class OutgoingMessage
    {
        public byte[] buffer => bytes.ToArray();
        public List<byte> bytes = new List<byte>();
        public void WriteBoolean(bool b)
        {
            byte[] bs = BitConverter.GetBytes(b);
            bytes.AddRange(bs);
        }
        public void WriteInt32(int i)
        {
            byte[] bs = BitConverter.GetBytes(i);
            bytes.AddRange(bs);
        }
        public void WriteFloat32(float f)
        {
            byte[] bs = BitConverter.GetBytes(f);
            bytes.AddRange(bs);
        }
        public void WriteString(string s)
        {
            byte[] bs = Encoding.UTF8.GetBytes(s);
            WriteInt32(bs.Length);
            bytes.AddRange(bs);
        }
        public void WriteFloatList(List<float> lf)
        {
            WriteInt32(lf.Count);
            foreach (var item in lf)
            {
                WriteFloat32(item);
            }
        }
    }
}
