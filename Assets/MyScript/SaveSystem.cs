using System;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    private static string basePath = Application.persistentDataPath;

    public static Progress LoadProgress(int index)
    {
        string path = basePath + "/progress_" + index + ".bin";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            
            Progress data = formatter.Deserialize(stream) as Progress;
            
            stream.Close();
            
            return data;
        }
        else
        {
            return new Progress(index, DateTime.Now.Date.ToString("d"));
        }
    }

    public static void SetProgress(int index)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = basePath + "/progress_" + index +".bin";
        FileStream stream = new FileStream(path, FileMode.Create);
        
        formatter.Serialize(stream, GameManager.Instance.presentProgress);
        stream.Close();
    }

}
