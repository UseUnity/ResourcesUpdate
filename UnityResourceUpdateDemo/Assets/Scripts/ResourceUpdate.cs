using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class BundleInfo
{
    public string Name;
    public string Hash;

    public BundleInfo(string name, string hash)
    {
        Name = name;
        Hash = hash;
    }
}

public class ResourceUpdate : MonoBehaviour {
    private string VERSION_FILE = "version.txt";
    private string SERVER_URL = "file:///E:/Resources/";
    private string LOCAL_URL = "file://" + Application.dataPath + "/Reources/";
    private string LOCAL_VERSION_PATH = "";

    private int totalCount = 0;
    private int curCount = 0;

    private Dictionary<string, BundleInfo> localFileDic;
    private Dictionary<string, BundleInfo> serverFileDic;
    private List<BundleInfo> needUpdateList;

    public delegate void FinishDel(WWW www);

	void Start () {
        LOCAL_VERSION_PATH = Application.dataPath + VERSION_FILE;

        StartCoroutine(Download(LOCAL_VERSION_PATH, delegate (WWW localVersion)
            {
                AnalysisVersion(localVersion.text, localFileDic);

                Download(SERVER_URL, delegate (WWW serverVersion)
                {
                    AnalysisVersion(serverVersion.text, serverFileDic);
                    CompareVersion();
                    DownloadResources();
                }
                );
            })
        );
	}
	
    void DownloadResources()
    {
        SetProgress();
        if (curCount == totalCount)
        {
            WriteVersionFile();
            return;
        }
        string fileName = needUpdateList[0].Name;

        StartCoroutine(Download(SERVER_URL + fileName, delegate(WWW www)
            {
                WriteFiles(fileName, www.bytes);
                DownloadResources();
            })
        );
    }

    void WriteFiles(string fileName, byte[] bytes)
    {
        string filePath = Application.persistentDataPath + "/" + GetPlatformName(Application.platform) + "/" + fileName;
        int index = filePath.LastIndexOf('/');
        string path = filePath.Substring(0, index);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        FileStream stream = new FileStream(filePath, FileMode.Create);
        stream.Write(bytes, 0, bytes.Length);
        stream.Flush();
        stream.Close();
    }

    void WriteVersionFile()
    {
        StringBuilder stringBuild = new StringBuilder();
        foreach (var item in serverFileDic)
        {
            stringBuild.Append(item.Key + "," + item.Value.Hash + "\n");
        }
        FileStream stream = new FileStream(LOCAL_VERSION_PATH, FileMode.Create);
        byte[] bytes = Encoding.UTF8.GetBytes(stringBuild.ToString());
        stream.Write(bytes, 0, bytes.Length);
        stream.Flush();
        stream.Close();
    }

    void SetProgress()
    {
        float progress = curCount / totalCount;
    }

    void CompareVersion()
    {
        Dictionary<string, BundleInfo>.Enumerator iter = serverFileDic.GetEnumerator();
        while (iter.MoveNext())
        {
            if (!localFileDic.ContainsKey(iter.Current.Key))
            {
                needUpdateList.Add(iter.Current.Value);
            }
            else
            {
                BundleInfo bundle = null;
                if (localFileDic.TryGetValue(iter.Current.Key, out bundle))
                {
                    if (bundle.Hash != iter.Current.Value.Hash)
                        needUpdateList.Add(iter.Current.Value);
                }
            }
        }
    }

    void AnalysisVersion(string text, Dictionary<string, BundleInfo> dict)
    {
        string[] lineInfo = text.Split('\r');
        for (int i = 0; i < lineInfo.Length; i++)
        {
            BundleInfo bundle = new BundleInfo(lineInfo[0], lineInfo[1]);
            dict.Add(bundle.Name, bundle);
        }
    }

    IEnumerator Download(string url, FinishDel finishFunc)
    {
        WWW www = new WWW(url);
        yield return www;
        if (www != null)
        {
            finishFunc(www);
        }
        www.Dispose();
    }

    string GetPlatformName(RuntimePlatform platform)
    {
        switch (platform)
        {
            case RuntimePlatform.Android:
                return "android";
            case RuntimePlatform.IPhonePlayer:
                return "ios";
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
                return "windows";
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer:
                return "osx";
            default:
                return "others";
        }
    }

}
