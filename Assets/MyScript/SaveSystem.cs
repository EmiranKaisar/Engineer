using System;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

public static class SaveSystem
{
    private static string basePath = Application.persistentDataPath;

    public static Progress LoadProgress(int index)
    {
        string path = basePath + "/progress_" + index + ".bin";
        Debug.Log(path);
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
            Debug.LogError("file not found in" + path + "it will be created then");
            return new Progress(index, DateTime.Now.Date.ToString("MM/dd/yyyy HH:mm"));
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
