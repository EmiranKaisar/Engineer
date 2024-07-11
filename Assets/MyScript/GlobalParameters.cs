using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GlobalParameters : MonoBehaviour
{
    public int presentLevelID = 0;
    
    public float rotateToolSpeed = 2;

    public float moveToolSpeed = 1;

    public bool editMode = false;
    
    //the info of level can be saved in local file
    //that can be created and shared
    public class LevelInfo
    {
        public int levelID;
        public List<PropTool> scenePropTools;
        public int levelType;
    }
    
    
    
    private string filePath;
    private string textToWrite;
    
    public class TestData
    {
        public string test;
        public int test1;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void WriteFile()
    {
        filePath = Application.persistentDataPath + "/test.txt";
        // ����Ҫд����ı�
        textToWrite = "Hello, World!";

        // ���ı�д���ļ�
        File.WriteAllText(filePath, textToWrite);
    }

    public void ReadFile()
    {
        string filePath = Application.persistentDataPath + "/playerData.json";
        string json = File.ReadAllText(filePath);

        // �����л�JSON�ַ���Ϊ����
        TestData playerData = JsonUtility.FromJson<TestData>(json);

        Debug.Log(playerData.test);
    }
}
