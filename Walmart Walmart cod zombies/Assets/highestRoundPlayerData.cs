using UnityEngine;
using System.IO;
using Unity.VisualScripting;
public class highestRoundData : MonoBehaviour
{   
    public static highestRoundData instance;
    public class PlayerData
    {
        public int highestRound = 0;
    }

    [System.Serializable]
    public class AllData
    {
        public PlayerData playerData;
    }

    private string filePath = Application.dataPath + "/JSONData.json";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void SaveData()
    {
        string JSONOutput = JsonUtility.ToJson(GetData());
        File.WriteAllText(filePath, JSONOutput);
    }

    public AllData LoadData()
    {
        string readJSON;
        if (!File.Exists(filePath))
        {
            DefaultJSON();
        }
        readJSON = File.ReadAllText(filePath);
        AllData data = JsonUtility.FromJson<AllData>(readJSON);
        return data;
    }

    public AllData GetData()
    {
        AllData data = new AllData();
        data.playerData.highestRound = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().highestRound;
        return data;
    }

    void DefaultJSON()
    {
        AllData data = new AllData();

        data.playerData = new PlayerData();

        string outputJSON = JsonUtility.ToJson(data);
        File.WriteAllText(filePath, outputJSON);
    }
}
