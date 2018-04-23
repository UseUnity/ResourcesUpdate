using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ExportVersion : MonoBehaviour {
    [MenuItem("Tools/ExportVersion")]
    public static void ExportVersionAndMD5()
    {
        string resourcePath = Application.streamingAssetsPath + "/" + BuildScript.GetPlatformName(Application.platform) + "/";
        StringBuilder versions = new StringBuilder();
        string[] files = Directory.GetFiles(resourcePath, "*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            string filePath = files[i];
            if (!filePath.Contains("."))
            {
                string relativePath = filePath.Replace(resourcePath, "").Replace("\\", "/");
                string md5 = MD5File(relativePath);
                versions.Append(relativePath).Append(",").Append(md5).Append("\n");
                continue;
            }
            string extension = filePath.Substring(filePath.LastIndexOf("."));
            if (extension == ".ab")
            {
                string relativePath = filePath.Replace(resourcePath, "").Replace("\\", "/");
                string md5 = MD5File(relativePath);
                versions.Append(relativePath).Append(",").Append(md5).Append("\n");
            }
        }
        FileStream stream = new FileStream(resourcePath + "version.txt", FileMode.Create);
        byte[] data = Encoding.UTF8.GetBytes(versions.ToString());
        stream.Write(data, 0, data.Length);
        stream.Flush();
        stream.Close();
    }

    public static string MD5File(string file)
    {
        try
        {
            FileStream stream = new FileStream(file, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retValue = md5.ComputeHash(stream);
            stream.Close();
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < retValue.Length; i++)
            {
                str.Append(retValue[i].ToString("x2"));
            }
            return str.ToString();
        }
        catch (System.Exception e)
        {
            throw new System.Exception("md5file() fail, error:" + e.Message);
        }
    }

}
