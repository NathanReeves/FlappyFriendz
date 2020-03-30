# iOS Realtime Multiplayer Plugin

A complete Unity project for the iOS game Flappy Friendz, a realtime online co-op version of Flappy Bird. 

If you just need the network code without the game, import just the Plugins folder into your project ( Make sure it looks like `Assets/Plugins/iOS/GameKitP/...` )

You'll need an Apple Developer account to make apps for the App Store that use GameCenter. Before you start with code, add your game to App Store Connect for the multiplayer services to work: https://help.apple.com/app-store-connect/#/dev2cd126805

# How-to

## Subscribing to Events

First, subscribe to the call backs you need from the iOS plugin. Like GameKit.Instance, do this on an object that persists between scenes by calling DontDestroyOnLoad

How the PlayerAuthEvent is handled in Flappy Friendz in the GameState class:

```
void Init()
{
    //Subscribe to auth event 
    GameKit.Instance.PlayerAuthEvent += new iOSEventHandler(playerAuthenticated);
    //Authenticate player
    GameKit.Instance.authPlayer();
    //Persist gameobject
    DontDestroyOnLoad(gameObject);
}
public long PlayerID;
void playerAuthenticated(string status)
{
    if (status == "true")
    {
        //On success, save playerID as number only
        string numberOnly = Regex.Replace(GameKit.Instance.GameCenterPlayerID().Substring(2), "[^0-9.]", "");
        long.TryParse(numberOnly, out PlayerID);
    }
    else
    {
        //If player cant authenticate, show GameCenter login prompt
        GameKit.Instance.showAuthView();
    }
}
```

iOSEvents you can subscribe to all return a status string:
```
PlayerAuthEvent         //returns "true" on success else "false"
FoundMatchEvent         //returns other players GameCenter ID unless match is an accepted invite, returns "i"+ other players GameCenter ID
CancelMatchmakingEvent  //returns "1" when matchmaking view is canceled
EnemyLeftMatchEvent     //returns "" when enemy player leave match
LockButtonPressedEvent  //returns "" when app is resumed from a lock screen action
ReceivedDataEvent       //returns the data received from other player
InAppPurchasedEvent     //returns "complete" on success else "failed" 
```


## Authenticating GameCenter Player

Before using a GameCenter service you'll need to authenticate the local GameCenter player:

`GameKit.Instance.authPlayer();`

You can check if a player is authenticated, if not, display a GameCenter login prompt:
```
if(GameKit.Instance.isAuthenticated())
{
    //Do GameCenter stuff
}
else
{
    GameKit.Instance.showAuthView();
}
```
Once authenticated, you can get a player's GameCenter ID as a string:

`GameKit.Instance.GameCenterPlayerID();`

Some IDs contain letters, you may want to parse a number only for comparing player IDs:
```
long PlayerID;
string numberOnly = Regex.Replace(GameKit.Instance.GameCenterPlayerID().Substring(2), "[^0-9.]", "");
long.TryParse(numberOnly, out PlayerID);
```

## Matchmaking
First, subscribe to these events for matchmaking
```
FoundMatchEvent         //returns other players GameCenter ID unless match is an accepted invite, returns "i"+ other players GameCenter ID
CancelMatchmakingEvent  //returns "1" when matchmaking view is canceled
EnemyLeftMatchEvent     //returns "" when enemy player leave match
ReceivedDataEvent       //returns the data received from other player
```

Starting the Matchmaking View Controller:

`GameKit.Instance.findMatch(maxPlayers, minPlayers);`

(This plugin currently only supports min 2, max 2 players)

If a player wants to quit the match but is in the process of connecting, use cancel match:

`GameKit.Instance.cancelMatch();`

You can get the number of players recently seaching for a match:

`GameKit.Instance.PlayersInMatchmaking();`

Quitting a match:

`GameKit.Instance.LeaveMatch();`


## Sending data to players

Players handle realtime multiplayer by sending string messages to each other. 
If needed, a host can be determined by comparing players GameCenter IDs.

Send a message to all players in the game:

`GameKit.Instance.sendData("hi");`

In Flappy Friendz, a gamestate object is serialized into a string before sending:
```
[System.Serializable]
public class SerializeGameState
{
    [SerializeField]
    public Vector2 position;
    public bool isAlive = false;
    public int score = 0;
    public float zRotation = 0;

}

SerializeGameState gameState = new SerializeGameState();
void Send()
{
    //Set position
    gameState.position = transform.position;
    //Serialize
    string state = JsonUtility.ToJson(gameState);
    //Send
    GameKit.Instance.sendData(state);
}
```
Everytime a message is received, the string is deserialized into a gamestate object with the friend's position, rotation, score, and alive status:
```
SerializeGameState friendGameState;
void GotData(string data)
{
    friendGameState = JsonUtility.FromJson<SerializeGameState>(data);
    //Do stuff with friend's gamestate
}

```

## GameCenter Leaderboard

Set up your leaderboard first in App Store Connect under Features > GameCenter > Leaderboards

Show GameCenter View Controller of all leaderboards in the game:

`GameKit.Instance.ShowAllLeaderboards();`

Show GameCenter View Controller of a specific leaderboard in the game:

`GameKit.Instance.ShowLeaderboard("yourLeaderboardID");`

Post a leaderboard score (value can accept `int` or `float`):

`GameKit.Instance.PostLeaderboard("yourLeaderboardID", value);`


## GameCenter Achievements

Set up your achievement first in App Store Connect under Features > GameCenter > Achievements

Post progress for an achievement (takes a double as progress between 0.0-1.0):

`GameKit.Instance.PostAchievementProgress("yourAchievementID", (double)progress/required);`

Reset all achievement progress

`GameKit.Instance.ResetAchievementProgress();`

Show all achievements in a GameCenter view

`GameKit.Instance.ShowAchievements();`


## In-App Purchases
Set up your IAP first in App Store Connect under Features > In-App Purchases

Subscribe to this event to know when the puchase is complete:

`InAppPurchasedEvent     //returns "complete" on success else "failed" `

You should always check to see if the device allows In-App Purchases, if not handle the UI accordingly:

```
if(GameKit.Instance.IAPCanPurchase())
{
    //show IAP
}
else
{
    //hide IAP
}
```

Buying a product:

`GameKit.Instance.PurchaseInApp("yourProductID");`

## Showing a review prompt
You can also prompt users to review your app (use this carefully):

`GameKit.Instance.ShowRating();`



