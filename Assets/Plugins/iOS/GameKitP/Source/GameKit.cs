using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEngine.Events;
[System.Serializable]
public class iOSEvent : UnityEvent<string>
{
}
public delegate void iOSEventHandler(string status);
public class GameKit : MonoBehaviour
{

    //iOS functions
    [DllImport("__Internal")]
    private static extern void _authenticatePlayer();
    [DllImport("__Internal")]
    private static extern bool _isAuthenticated();
    [DllImport("__Internal")]
    private static extern void _showAuthView();
    [DllImport("__Internal")]
    private static extern string _playerID();

    [DllImport("__Internal")]
    private static extern void _findMatch(int maxPlayers, int minPlayers);
    [DllImport("__Internal")]
    private static extern void _cancelMatchFind();
    [DllImport("__Internal")]
    private static extern void _sendData(string d);
    [DllImport("__Internal")]
    private static extern void _leaveMatch();
    [DllImport("__Internal")]
    private static extern int _playersOnline();

    [DllImport("__Internal")]
    private static extern void _showAchievements();
    [DllImport("__Internal")]
    private static extern void _showLeaderboard(string id);
    [DllImport("__Internal")]
    private static extern void _showAllLeaderboards();
    [DllImport("__Internal")]
    private static extern void _postLeaderboardInt(string id, int value);
    [DllImport("__Internal")]
    private static extern void _postLeaderboardFloat(string id, float value);
    [DllImport("__Internal")]
    private static extern void _resetAchievements();
    [DllImport("__Internal")]
    private static extern void _postAchievement(string id, double progress);

    [DllImport("__Internal")]
    private static extern void _purchaseInApp(string id);
    [DllImport("__Internal")]
    private static extern bool _canPurchaseInApp();
    [DllImport("__Internal")]
    private static extern int _showRating();


       
    public void authPlayer()
    {
        //GameCenter Authentication - call this when app loads
        _authenticatePlayer();
    }
    public bool isAuthenticated()
    {
        return _isAuthenticated();
    }
    public string GameCenterPlayerID()
    {
        //Player IDs useful for determining host
        return _playerID();
    }
    public void showAuthView()
    {
        //if players are not authenticated on app load, show this view to ask them to login to GameCenter
        _showAuthView();
    }
    public void LeaveMatch()
    {
        _leaveMatch();
    }
    public void findMatch(int maxPlayers, int minPlayers)
    {
        //Show MatchmakingViewController - allows players to invite friends or seach for a random opponent 
        _findMatch(maxPlayers, minPlayers);
    }
    public void cancelMatch()
    {
        //if using a custom matchmaking UI
        _cancelMatchFind();
    }
    public int PlayersInMatchmaking()
    {
        //number of players recently seaching for a match
        return _playersOnline();
    }
    public bool IAPCanPurchase()
    {
        //This device allows in app purchases - if false, change UI to show this
        return _canPurchaseInApp();
    }
    public void PostAchievementProgress(string type, double prog)
    {
        _postAchievement(type, prog);
    }
    public void ResetAchievementProgress()
    {
        _resetAchievements();
    }
    public void ShowRating()
    {
        //Displays prompt for player to rate your app
        _showRating();
    }
    public void ShowLeaderboard(string leaderboardID)
    {
        //Shows GameCenter Leaderboard View Controller
        //set up your leaderboard in App Store Connect first under Features > GameCenter
        _showLeaderboard(leaderboardID);
    }
    public void ShowAllLeaderboards()
    {
        //Shows GameCenter View Controller of all Leaderboards in the game
        //set up your leaderboard in App Store Connect first under Features > GameCenter
        _showAllLeaderboards();
    }

    public void PurchaseInApp(string inAppID)
    {
        //In App Purchase - set this up in App Store Connect first
        _purchaseInApp(inAppID);
    }
    public void ShowAchievements()
    {
        //Show GameCenter Achievement View Controller
        _showAchievements();
    }
    public void PostLeaderboard(string leaderboardID, int value)
    {
        _postLeaderboardInt(leaderboardID, value);
    }
    public void PostLeaderboard(string leaderboardID, float value)
    {
        _postLeaderboardFloat(leaderboardID, value);
    }
    public void sendData(string d)
    {
        //Send a string to all players
        _sendData(d); 
    }

    //iOS Events
    public event iOSEventHandler PlayerAuthEvent, FoundMatchEvent, LockButtonPressedEvent,
    EnemyLeftMatchEvent, CancelMatchmakingEvent, ReceivedDataEvent, InAppPurchasedEvent;

    private static GameKit _instance;
    public static GameKit Instance
    {
        get
        {
            if (_instance == null)
            { 
                var obj = new GameObject("GameKit");
                _instance = obj.AddComponent<GameKit>();
                DontDestroyOnLoad(_instance);
            }

            return _instance;
        }
    }
    
    
    public void PlayerAuthenticated(string status)
    {
        PlayerAuthEvent?.Invoke(status);
    }
    public void FoundMatch(string status)
    {
        FoundMatchEvent?.Invoke(status);
    }
    public void LockButtonPressed(string status)
    {
        LockButtonPressedEvent?.Invoke(status);
    }
    public void EnemyLeftMatch(string status)
    {
        EnemyLeftMatchEvent?.Invoke(status);
    }
    public void CancelMatchmaking(string status)
    {
        CancelMatchmakingEvent?.Invoke(status);
    }
    public void ReceivedData(string status)
    {
        ReceivedDataEvent?.Invoke(status);
    }
    public void InAppPurchased(string status)
    {
       InAppPurchasedEvent?.Invoke(status);
    }

}
