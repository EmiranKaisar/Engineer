using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

public static class SaveSystem
{
    private static string basePath = Application.persistentDataPath;
    public static void SaveLevel(LevelInfo level)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = basePath + "/level_" +level.levelID +".level";
        FileStream stream = new FileStream(path, FileMode.Create);
        
        formatter.Serialize(stream, level);
        stream.Close();
    }

    public static LevelInfo LoadLevel(int levelIndex)
    {
        string path = basePath + "/level_" + levelIndex + ".level";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            
            LevelInfo data = formatter.Deserialize(stream) as LevelInfo;

            return data;
        }
        else
        {
            Debug.LogError("file not found in" + path);
            return null;
        }
    }

    public static List<string> FileList()
    {
        // string [] fileInfo = Directory.GetFiles(basePath, "*.level");
        // List<string> fileList = new List<string>();

        string tryPath = "";
        List<string> fileList = new List<string>();
        for (int i = 0; i < 100; i++)
        {
            tryPath = basePath + "/level_" + i + ".level";
            if (File.Exists(tryPath))
            {
                fileList.Add("level_" + i );
            }
            else
            {
                break;
            }
            
            Debug.Log("loop: " + i);
        }

        return fileList;
    }
}
