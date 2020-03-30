using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
[System.Serializable]
public class SaveData
{
    public bool isDefault = true;
    public int highestScore;
    public int birdColor = 0;
    public bool unlockedRain = false;

    public SaveData()
    {

    }
    public void WriteToFile(string filePath)
    {
        isDefault = false;
        string json = JsonUtility.ToJson(this, true);
        File.WriteAllText(filePath, json);
    }
    public static SaveData ReadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return new SaveData();
        }
        else
        {
            string contents = File.ReadAllText(filePath);

           // Debug.LogFormat("ReadFromFile({0})\ncontents:\n{1}", filePath, contents);

            if (string.IsNullOrEmpty(contents))
            {
                Debug.LogErrorFormat("File: '{0}' is empty. Returning default SaveData");
                return new SaveData();
            }

            return JsonUtility.FromJson<SaveData>(contents);
        }
    }

}
