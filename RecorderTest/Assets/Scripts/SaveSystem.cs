using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class SaveSystem
{
    public static readonly string SAVE_FOLDER = Application.persistentDataPath + "/Videos/";

    public static readonly string SAVE_TXT = SAVE_FOLDER + "SavedVideos.txt";

    // Start is called before the first frame update
    public static void Init()
    {
        if (!Directory.Exists(SAVE_FOLDER))
        {
            Directory.CreateDirectory(SAVE_FOLDER);
            Debug.Log("Created folder: " + SAVE_FOLDER);
        }
    }

    public static void Save(string name, string filePath)
    {
        List<VideoItem> saveListData = new List<VideoItem>();

        if (File.Exists(SAVE_TXT))
        {
            Debug.Log("Saving txt exists");
            string list = File.ReadAllText(SAVE_TXT);

            SaveObject listObject = JsonUtility.FromJson<SaveObject>(list);
            saveListData = listObject.videoItem;

            VideoItem newItem = new VideoItem
            {
                name = name,
                filePath = filePath
            };

            saveListData.Add(newItem);

            //Save file path to json
            SaveObject saveObject = new SaveObject
            {
                videoItem = saveListData
            };
            string json = JsonUtility.ToJson(saveObject);
            Debug.Log(json);

            File.WriteAllText(SAVE_TXT, json);
        }
        else
        {
            Debug.Log("Saving txt doesn't exist");
            VideoItem newItem = new VideoItem
            {
                name = name,
                filePath = filePath
            };

            saveListData.Add(newItem);

            //Save file path to json
            SaveObject saveObject = new SaveObject
            {
                videoItem = saveListData
            };
            string json = JsonUtility.ToJson(saveObject);
            Debug.Log(json);

            File.WriteAllText(SAVE_TXT, json);
        }

        
    }

    public static List<VideoItem> Load()
    {
        List<VideoItem> saveListData = new List<VideoItem>();

        if (File.Exists(SAVE_TXT))
        {
            string list = File.ReadAllText(SAVE_TXT);
            Debug.Log(list);

            SaveObject listObject = JsonUtility.FromJson<SaveObject>(list);
            saveListData = listObject.videoItem;
            return saveListData;
        }
        else
        {
            Debug.Log("Didn't find txt file");
            return null;
        }

        
    }
}

[System.Serializable]
public class SaveObject
{
    public List<VideoItem> videoItem;
}

[System.Serializable]
public class VideoItem
{
    public string name;
    public string filePath;
}