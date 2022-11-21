using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
/// <summary>
/// XML序列化帮助类
/// </summary>
public static class XMLHelper
{
    /// <summary>
    /// 序列化到文件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data">对象</param>
    /// <param name="path">完整路径</param>
    public static void SaveToFile<T>(T data, string path, bool compress = false, bool encrypt = false)
    {
        if (!Directory.Exists(Path.GetDirectoryName(path)))//路径不存在或未创建
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }
        if (Path.GetFileName(path) == null) return;//路径未包含文件名
        SaveString(ObjectToXML(data, compress, encrypt), path);
    }
    /// <summary>
    /// 从文件反序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public static T LoadFromFile<T>(string path, bool compress = false, bool encrypt = false)
    {
        if (!File.Exists(path)) return default;//文件不存在
        return XMLToObject<T>(LoadString(path), compress, encrypt);
    }
    /// <summary>
    /// 读取文件内容
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string LoadString(string path)
    {
        if (!File.Exists(path)) return default;//文件不存在
        using (StreamReader sr = new StreamReader(path))
        {
            return sr.ReadToEnd();
        }
    }
    /// <summary>
    /// 保存内容到文件
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static void SaveString(string content, string path)
    {
        if (!Directory.Exists(Path.GetDirectoryName(path))) return;//路径不存在或未创建
        if (Path.GetFileName(path) == null) return;//路径未包含文件名
        using (StreamWriter sw = new StreamWriter(path))
        {
            sw.WriteLine(content);
        }
    }
    /// <summary>
    /// 序列化到string
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string ObjectToXML<T>(T data, bool compress = false, bool encrypt = false)
    {
        if (data == null) return null;
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        MemoryStream stream = new MemoryStream();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Encoding = Encoding.UTF8;
        settings.Indent = true;
        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            serializer.Serialize(writer, data);
        }
        string s = Encoding.UTF8.GetString(stream.ToArray());
        if (compress)
            s = Compress(s);
        if (encrypt)
            s = Encrypt(s);
        return s;
    }
    /// <summary>
    /// 从string反序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="xml"></param>
    /// <returns></returns>
    public static T XMLToObject<T>(string xml, bool compress = false, bool encrypt = false)
    {
        if (string.IsNullOrEmpty(xml)) return default;
        if (encrypt)
            xml = Decrypt(xml);
        if (compress)
            xml = Decompress(xml);
        T t;
        using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
        {
            using (StreamReader sr = new StreamReader(ms, true))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                try
                {
                    t = (T)serializer.Deserialize(sr);
                }
                catch
                {
                    t = default;
                }
            }
        }
        return t;
    }


    /// AES加密
    /// </summary>
    /// <param name="encryptStr">明文</param>
    /// <param name="key">密钥</param>
    /// <returns></returns>
    public static string Encrypt(string encryptStr, string key = "1234567812345678")
    {
        if (string.IsNullOrEmpty(encryptStr)) return "";
        byte[] keyArray = Encoding.UTF8.GetBytes(key);
        byte[] toEncryptArray = Encoding.UTF8.GetBytes(encryptStr);
        RijndaelManaged rDel = new RijndaelManaged();
        rDel.Key = keyArray;
        rDel.Mode = CipherMode.ECB;
        rDel.Padding = PaddingMode.PKCS7;
        ICryptoTransform cTransform = rDel.CreateEncryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
        return Convert.ToBase64String(resultArray, 0, resultArray.Length);
    }
    public static byte[] Encrypt(byte[] encryptByte, string key = "1234567812345678")
    {
        byte[] keyArray = Encoding.UTF8.GetBytes(key);
        RijndaelManaged rDel = new RijndaelManaged();
        rDel.Key = keyArray;
        rDel.Mode = CipherMode.ECB;
        rDel.Padding = PaddingMode.PKCS7;
        ICryptoTransform cTransform = rDel.CreateEncryptor();
        return cTransform.TransformFinalBlock(encryptByte, 0, encryptByte.Length);
    }
    /// AES解密
    /// </summary>
    /// <param name="decryptStr">密文</param>
    /// <param name="key">密钥</param>
    /// <returns></returns>
    public static string Decrypt(string decryptStr, string key = "1234567812345678")
    {
        if (string.IsNullOrEmpty(decryptStr)) return "";
        byte[] keyArray = Encoding.UTF8.GetBytes(key);
        byte[] toEncryptArray = Convert.FromBase64String(decryptStr);
        RijndaelManaged rDel = new RijndaelManaged();
        rDel.Key = keyArray;
        rDel.Mode = CipherMode.ECB;
        rDel.Padding = PaddingMode.PKCS7;
        ICryptoTransform cTransform = rDel.CreateDecryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
        return UTF8Encoding.UTF8.GetString(resultArray);
    }
    public static byte[] Decrypt(byte[] decryptByte, string key = "1234567812345678")
    {
        byte[] keyArray = Encoding.UTF8.GetBytes(key);
        RijndaelManaged rDel = new RijndaelManaged();
        rDel.Key = keyArray;
        rDel.Mode = CipherMode.ECB;
        rDel.Padding = PaddingMode.PKCS7;
        ICryptoTransform cTransform = rDel.CreateDecryptor();
        return cTransform.TransformFinalBlock(decryptByte, 0, decryptByte.Length);
    }
    public static string Compress(string str)
    {
        if (string.IsNullOrEmpty(str)) return "";
        var compressBeforeByte = Encoding.GetEncoding("UTF-8").GetBytes(str);
        var compressAfterByte = Compress(compressBeforeByte);
        string compressString = Convert.ToBase64String(compressAfterByte);
        return compressString;
    }

    public static string Decompress(string str)
    {
        if (string.IsNullOrEmpty(str)) return "";
        var compressBeforeByte = Convert.FromBase64String(str);
        var compressAfterByte = Decompress(compressBeforeByte);
        string compressString = Encoding.GetEncoding("UTF-8").GetString(compressAfterByte);
        return compressString;
    }

    /// <summary>
    /// Compress
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private static byte[] Compress(byte[] data)
    {
        try
        {
            var ms = new MemoryStream();
            var zip = new GZipStream(ms, CompressionMode.Compress, true);
            zip.Write(data, 0, data.Length);
            zip.Close();
            var buffer = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(buffer, 0, buffer.Length);
            ms.Close();
            return buffer;

        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// Decompress
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private static byte[] Decompress(byte[] data)
    {
        try
        {
            var ms = new MemoryStream(data);
            var zip = new GZipStream(ms, CompressionMode.Decompress, true);
            var msreader = new MemoryStream();
            var buffer = new byte[0x1000];
            while (true)
            {
                var reader = zip.Read(buffer, 0, buffer.Length);
                if (reader <= 0)
                {
                    break;
                }
                msreader.Write(buffer, 0, reader);
            }
            zip.Close();
            ms.Close();
            msreader.Position = 0;
            buffer = msreader.ToArray();
            msreader.Close();
            return buffer;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}
