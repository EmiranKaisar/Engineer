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
            
            stream.Close();
            
            return data;
        }
        else
        {
            Debug.LogError("file not found in" + path);
            return null;
        }
    }

}
