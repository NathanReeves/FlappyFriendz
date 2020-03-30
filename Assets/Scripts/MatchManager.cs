using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviour
{
    public GameObject TitlePanel;
    public GameObject PlayingPanel;
    public GameObject DeathPanel;
    public GameObject RespawnPanel;
    public GameObject ConnectingPanel;
    public GameObject ConnectingFriendSprite;
    public GameObject PlayButtons;
    public GameObject FlappyPlayer;
    public Flappy flappy;
    public Friend friend;
    public SpawnerScript spawner;

    public CustomizeBird customizeBird;

    public SerializeGameState friendGameState = new SerializeGameState(Vector3.zero);
    //when we recieve data from other plater
    bool friendInMatch;
    //keeps player from respawning after gameover screen w/o reloading scene first
    bool canRespawn = true;

    Coroutine flyCoroutine;

    UnityAction<string> matchFoundListener;

    void Awake()
    {    
        Application.targetFrameRate = 60;
        if (GameStateManager.Instance.foundMatch) { }
        customizeBird = GameObject.Find("CustomBirdManager").GetComponent<CustomizeBird>();
        customizeBird.ChangeBirdColor(GameStateManager.Instance.SaveData.birdColor);
    }

    void Start()
    {
        
        flappy = FlappyPlayer.GetComponent<Flappy>();
        if (GameStateManager.GameState == GameState.Connecting)
        {
            switchToConnectingState();
        }
        else if(GameStateManager.GameState == GameState.Intro)
        {
            if (GameStateManager.Instance.foundMatch)
            {
                customizeBird.ChangeFriendColor(GameStateManager.Instance.friendColor);
                spawner.SpawnPipes(GameStateManager.Instance.Pipes, GameStateManager.Instance.Pipes.Count);
                friendInMatch = true;
                friend.gameObject.SetActive(true);
            }
            TitlePanel.SetActive(false);
            PlayingPanel.SetActive(true);
            FlappyPlayer.SetActive(true);
            flappy.enabled = true;
        }
        else if(GameStateManager.GameState == GameState.Title)
        {
            TitlePanel.SetActive(true);
            PlayingPanel.SetActive(false);
            flappy.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void FindMatch()
    {
        if (GameKit.Instance.isAuthenticated())
            GameKit.Instance.findMatch(2,2);//require 2 players in a match
        else
            GameKit.Instance.showAuthView();//GameCenter login prompt
        
    }
    void switchToConnectingState()
    {
        ConnectingPanel.SetActive(true);
        ConnectingFriendSprite.SetActive(true);
        TitlePanel.SetActive(false);

        friendInMatch = true;
        //friend.gameObject.SetActive(true);
    }
    public void FoundMatch(bool isInvited)
    {
        if (isInvited)
        {
            if (friendInMatch)
            {
                GameStateManager.Instance.QuitMatch();
                BackToMenuConnecting();
            }
        }
        GameStateManager.GameState = GameState.Connecting;
        switchToConnectingState();

    }
    public void CancelMatchFind()
    {
        friendInMatch = false;
        ConnectingPanel.SetActive(false);
        ConnectingFriendSprite.SetActive(false);
        TitlePanel.SetActive(true);
        PlayingPanel.SetActive(false);
        flappy.enabled = false;
    }
    //Pipe info received, ready to start match
    public void FriendConnected()
    {
        StartLevel();
    }
    public void GotData(string data)
    {

        friendInMatch = true;
        bool wasAlive = friendGameState.isAlive;
        friendGameState = JsonUtility.FromJson<SerializeGameState>(data);
        if (GameStateManager.GameState == GameState.Dead) 
        {
            if (!friendGameState.isAlive && wasAlive)
            {
                flappy.UpdateScore(friendGameState.score);
                canRespawn = false;
                DeathPanel.SetActive(true);
                RespawnPanel.SetActive(false);
            }
        }
        friend.recGameState(friendGameState.position, friendGameState.zRotation);
    }
    public void StartLevel()
    {
        GameStateManager.GameState = GameState.Intro;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void RestartLevel()
    {
        GameStateManager.GameState = GameState.Respawning;
        StartCoroutine(waitForFriend()); 
    }
    public void ShowLeaderboards()
    {
        if (GameKit.Instance.isAuthenticated())
            GameKit.Instance.ShowLeaderboard("score");
        
    }
    public void ShowDeath()
    {
        if(friendGameState.isAlive && friendGameState.score >= 1)
        {
            RespawnPanel.SetActive(true);
        }
        else
        {
            canRespawn = false;
            DeathPanel.SetActive(true);
        }
    }
    public void FlyToFriend()
    {
        FlappyPlayer.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        if (flyCoroutine != null)
        {
            StopCoroutine(flyCoroutine);
        }
        flyCoroutine = StartCoroutine(Fly(new Vector3(((float)friendGameState.score*1.5f) + 2.05f, 0, FlappyPlayer.transform.position.z)));
    }
    IEnumerator Fly(Vector3 destination)
    {
        int friendScore = friendGameState.score;
        float t = 0.0f;
        while (t < 1.0f)
        {
            //arrived at target location
            if(Mathf.Abs(FlappyPlayer.transform.position.y) < .035f)
            {
                break;
            }
            t += Time.deltaTime * (Time.timeScale / 5f);
            FlappyPlayer.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            FlappyPlayer.transform.position = Vector3.Lerp(FlappyPlayer.transform.position, destination, t);
            yield return null;
        }
        GameStateManager.GameState = GameState.Playing;
        flappy.RespawnOnFriend(friendScore);
        yield return null;

    }
    IEnumerator waitForFriend()
    {
        int timeout = 0;
        while(!friendInMatch && timeout < 30)
        {
            timeout++;
            yield return null;
        }
        //if friend isAlive - fly to friend
        if(friendGameState.isAlive)
        {
            if(canRespawn)
            {
                DeathPanel.SetActive(false);
                RespawnPanel.SetActive(false);
                FlyToFriend();
            }
            else
            {
                StartLevel();
            }
            
            
        }
        //if friend is dead - restart
        else
        {
            StartLevel();
        }
        yield return null;
    }

    

    public void BacktoMenu()
    {
        GameStateManager.Instance.QuitMatch();
        GameStateManager.GameState = GameState.Title;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void BackToMenuConnecting()
    {
        GameStateManager.GameState = GameState.Title;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
