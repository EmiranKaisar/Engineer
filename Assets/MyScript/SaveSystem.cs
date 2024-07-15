using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    private static string basePath = Application.persistentDataPath;
    public static void SaveLevel(LevelInfo level)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = basePath + "/level_1.level";
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
}
