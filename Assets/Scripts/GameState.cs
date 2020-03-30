using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEngine.Events;

public enum GameState
{
    Title,
    Connecting,
    Intro,  
    Playing,
    Dead,
    Respawning
}

public class GameStateManager : MonoBehaviour
{
    public GameStateManager()
    {
        
    }

    private static GameStateManager _instance;
    public static GameState GameState { get; set; }

    public static GameStateManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameState = GameState.Title;
                GameObject gk = new GameObject("GameStateManager");
                _instance = gk.AddComponent<GameStateManager>();
                _instance.InitGameState();
            }

            return _instance;
        }
    }

    public long PlayerID;
    public bool isHost;

    //shared list of upcoming pipes
    public List<float> Pipes;
    //is in match with friend
    public bool foundMatch = false;

    //default friend color - red
    public int friendColor = 4;

    //friend has fully connnected and loaded their scene
    public bool friendReady;
    //match is invited rather than matchmade
    public bool isInvitedMatch;

    public MatchManager match;
    //connection timeout for sending game to background: 7s
    System.DateTime timeoutTimeStart;

    public void InitGameState()
    {
#if !UNITY_EDITOR
        GameKit.Instance.authPlayer();
#endif
        GameKit.Instance.PlayerAuthEvent += new iOSEventHandler(playerAuthenticated);
        GameKit.Instance.FoundMatchEvent += new iOSEventHandler(FoundMatch);
        GameKit.Instance.CancelMatchmakingEvent += new iOSEventHandler(matchmakingViewCanceled);
        GameKit.Instance.EnemyLeftMatchEvent += new iOSEventHandler(FriendLeftMatch);
        GameKit.Instance.LockButtonPressedEvent += new iOSEventHandler(ScreenLocked);
        GameKit.Instance.ReceivedDataEvent += new iOSEventHandler(GotData);
        GameKit.Instance.InAppPurchasedEvent += new iOSEventHandler(InAppPurchased);

        DontDestroyOnLoad(gameObject);
        if (SaveData == null)
            LoadSaveData();
    }

    void playerAuthenticated(string status)
    {
        if (status == "true")
        {
            //get playerID as number only
            string numberOnly = Regex.Replace(GameKit.Instance.GameCenterPlayerID().Substring(2), "[^0-9.]", "");
            long.TryParse(numberOnly, out PlayerID);
        }
        else
        {
            //if player cant authenticate, show GameCenter login prompt
            GameKit.Instance.showAuthView();
        }
    }

    void FoundMatch(string status)
    {
        string friendID;
        long fID;
        if(status[0] == 'i')
        {
            isInvitedMatch = true;
            friendID = status.Substring(1);
        }
        else
        {
            friendID = status;
        }
        friendID = Regex.Replace(friendID.Substring(2), "[^0-9.]", "");
        long.TryParse(friendID, out fID);
        if (fID > PlayerID)
        {
            isHost = false;
        }
        else
        {
            isHost = true;
        }
        foundMatch = true;
        StartCoroutine(waitForReady());
        if (match == null)
        {
            match = GameObject.Find("MatchManager").GetComponent<MatchManager>();
        }
        match.FoundMatch(isInvitedMatch);
    }

    void matchmakingViewCanceled(string status)
    {
        if (match == null)
        {
            match = GameObject.Find("MatchManager").GetComponent<MatchManager>();
        }
        match.CancelMatchFind();
    }

    void FriendLeftMatch(string status)
    {
        foundMatch = false;
        if (match == null)
        {
            match = GameObject.Find("MatchManager").GetComponent<MatchManager>();
        }
        match.BacktoMenu();
    }

    void ScreenLocked(string status)
    {
        foundMatch = false;
        if (match == null)
        {
            match = GameObject.Find("MatchManager").GetComponent<MatchManager>();
        }
        match.BacktoMenu();
    }

    void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            timeoutTimeStart = System.DateTime.Now.ToUniversalTime();
        }
        else
        {
            if (System.DateTime.Now.ToUniversalTime().AddSeconds(-7).CompareTo(timeoutTimeStart) > 0)
            {
                foundMatch = false;
                if (match == null)
                {
                    match = GameObject.Find("MatchManager").GetComponent<MatchManager>();
                }
                match.BacktoMenu();
            }

        }

    }

    public void GotData(string status)
    {
        //got pipe info
        if (status[0] == 'p')
        {
            SerializePipes p = JsonUtility.FromJson<SerializePipes>(status.Substring(1));
            Instance.Pipes = p.pipes;
            //Non Host can start after recieving pipe data
            if (match == null)
            {
                match = GameObject.Find("MatchManager").GetComponent<MatchManager>();
            }
            match.FriendConnected();

        }
        //friend is ready
        else if (status[0] == 'r')
        {
            //get friend color inside ready message
            int fc;
            if(int.TryParse(status.Substring(1), out fc))
            {
                friendColor = fc;
            }
            GameKit.Instance.sendData("r" + Instance.SaveData.birdColor.ToString());
            friendReady = true;
            foundMatch = true;
        }
        //friend gamestate
        else
        {
            if (match == null)
            {
                match = GameObject.Find("MatchManager").GetComponent<MatchManager>();
            }
            match.GotData(status);
        }
    }

    public void InAppPurchased(string status)
    {
        GameObject.Find("CustomBirdManager").GetComponent<CustomizeBird>().BoughtRainbowBird();
    }

    //players both send "r" to each other until they are both connected and ready
    public IEnumerator waitForReady()
    {
        while (!friendReady)
        {
            GameKit.Instance.sendData("r"+ Instance.SaveData.birdColor.ToString());
            yield return new WaitForSeconds(.2f);
        }
        StartCoroutine(sendPipes());
        yield return null;
    }
    //sends a serialized representation of 2000 pipes and their height
    IEnumerator sendPipes()
    {
        yield return new WaitForSeconds(.2f);
        if (isHost)
        {
            SerializePipes pipes = new SerializePipes(2000);
            Instance.Pipes = pipes.pipes;
            string sendPipes = 'p' + JsonUtility.ToJson(pipes);
            GameKit.Instance.sendData(sendPipes);
            //Host can start after sending pipe data
            if (match == null)
            {
                match = GameObject.Find("MatchManager").GetComponent<MatchManager>();
            }
            match.FriendConnected();
        }
        yield return null;
    }

    public void QuitMatch()
    {
        if (foundMatch)
        {
            GameKit.Instance.LeaveMatch();
        }

        friendColor = 0;
        isHost = false;
        friendReady = false;
        foundMatch = false;
        isInvitedMatch = false;
        Pipes = null;

    }

    //Save file for saving high score and bird colors
    public SaveData SaveData { get; set; }

    private const string fileName = "s";
    private const string fileExtension = ".txt";
    private string dataDirectory { get { return Application.persistentDataPath + "/"; } }

    public string GetSaveDataFilePath()
    {
        if (!Directory.Exists(dataDirectory))
            Directory.CreateDirectory(dataDirectory);

        return dataDirectory + fileName + fileExtension;
    }
    public void LoadSaveData()
    {
        if (File.Exists(GetSaveDataFilePath()))
        {
            SaveData = SaveData.ReadFromFile(GetSaveDataFilePath());
        }
        else
        {
            SaveData = new SaveData();
        }
    }

    public void WriteSaveData()
    {
        if (SaveData == null)
        {
            SaveData = new SaveData();
        }
        SaveData.WriteToFile(GetSaveDataFilePath());
    }

}

