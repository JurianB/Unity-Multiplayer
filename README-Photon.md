# Photon Networking Setup
In this guide we will take you through all the steps to install Photon and add multiplayer to our tank game.

**We recommend using Unity _2019.2.3f1_.**

# TODO: player name scripts hierin verwerken

## Step 1: 
Open Multiplayer-Base as a Unity Project. This will be used to implement Photon Networking.

## Step 2:
Inside Unity navigate to the _Asset Store_ search for  _PUN 2 - FREE_. Download and import this asset.

Link to the correct version: https://assetstore.unity.com/packages/tools/network/pun-2-free-119922.

To save some space on your PC, you can uncheck the demo folders when importing PUN.

## Step 3:
Meanwhile PUN is being imported, navigate to: https://www.photonengine.com/pun and sign up for an account.

## Step 4:
When you're account is working, head over to https://dashboard.photonengine.com/en-US/publiccloud to see all your applications.

- Create a new app:
- **Be careful to choose _PHOTON PUN_ as a type. This will add Unity specific implementations.**
- Give it a name and create it

## Step 5:
Go to your created app and find its App Id, this will be used to setup Networking with Unity. Go back to Unity and insert it in the settings.

## Step 6:
Now when running the game, you can move around by pressing your WASD-keys. Look around by moving your mouse and you can shoot by pressing your left mouse button.

## Step 7:
Now we're going to create a new scene that acts as a sort of begin screen. In here you can enter a Photon Nickname (which is required to create or join a game).

- Create a new scene called _Lobby_. In here we're going to create a new inputfield by going to: _GameObject > UI > Input Field_. This field be used to enter a playername.
You can place it where ever you want, as long as you can still use it.

- Next up, we want to create a new panel inside the Canvas. To create one, select your Canvas in the Hierachy and right mouse to: _UI > Button_. Call it "Join Button" and place it down below the input field. Give the button the text "Join". 

If you're doing it all correct, you would see something like this:
[IMAGE]

- Create a label inside the Canvas. _UI > Text_ and call it "Connect Text". This will show if the game is connecting to a room. So change the content text to "Connecting...". Place this Text above the input field and disable it in the scene.

## Step 8:
Now we're ready to setup a script that will cause Photon to host a game for us.

- In the Hierachy, create a new empty gameobject called _Launch Manager_.
- Create a new script called _Launcher_ and attach it to this GameObject. 

**Let the coding begin....**

In the Launcher.cs you see this class extends from MonoBehaviour (which Unity made for us). Photon uses it's own, which adds networking features.

We'll swap MonoBehaviour with _MonoBehaviourPunCallbacks_ and import the next two things:

```C#
using Photon.Pun;
using Photon.Realtime;
```

Add the following things above the Start() Method:

```C#
[Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
[SerializeField]
private byte maxPlayersPerRoom = 4;

[Tooltip("The UI Panel to let the user enter name, connect and play")]
[SerializeField]
private GameObject controlPanel;
[Tooltip("The UI Label to inform the user that the connection is in progress")]
[SerializeField]
private GameObject progressLabel;

string gameVersion = "1";

bool isConnecting;
```

In the inspector drag your "Connect Text" to your progressLabel field into your script. Drag your connect button into the controlPanel field.

## Step 9:
Replace start with the following and add a new method Connect:
```C#
private void Start()
{
    progressLabel.SetActive(false);
    controlPanel.SetActive(true);
}

// Will connect to a room or join the Photon network.
public void Connect()
{
    isConnecting = true;

    progressLabel.SetActive(true);
    controlPanel.SetActive(false);

    // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
    if (PhotonNetwork.IsConnected)
    {
        // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
        PhotonNetwork.JoinRandomRoom();
    }
    else
    {
        // #Critical, we must first and foremost connect to Photon Online Server.
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
    }
}
```

## Step 10:
_MonoBehaviourPunCallbacks Callbacks_ uses callback when using networking.
So when connected to a server Photon callbacks will be called.

We're going to implement these callbacks.

```C#
#region MonoBehaviourPunCallbacks Callbacks

public override void OnConnectedToMaster()
{
    Debug.Log("Launcher: OnConnectedToMaster() was called by PUN");

    // #Critical: The first we try to do is to join a potential existing room. 
    // If there is, good, else, we'll be called back with OnJoinRandomFailed()
    if(isConnecting)
    {
        PhotonNetwork.JoinRandomRoom();
    }
}


public override void OnDisconnected(DisconnectCause cause)
{
    Debug.LogWarningFormat("Launcher: OnDisconnected() was called by PUN with reason {0}", cause);

    progressLabel.SetActive(false);
    controlPanel.SetActive(true);
}

public override void OnJoinRandomFailed(short returnCode, string message)
{
    Debug.Log("Launcher() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

    // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
    PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
}

public override void OnJoinedRoom()
{
    Debug.Log("Launcher(): OnJoinedRoom() called by PUN. Now this client is in a room.");

    PhotonNetwork.LoadLevel(1);
}


#endregion
```

These callbacks will load a new level when joining or hosting a game.
To set up these loading scenes go to: _File > Build Settings_.

The scene order is:
(0): Lobby
(1): Room

## Step 11:
Now,  we're hooking up the Join button to call the Connect().
Navigate to your join button, add a new OnClick Listener.

Drag your Launch Manager GameObject into the field and make sure as an argument that the Launcher Script gets called to the Connect() method.

NOWW, when playing the game. Enter a name and press join! 
A new scene is loaded and you can move around!

The thing is, the controls aren't made for Networking yet. So let's fix this!

## Step 12:
Photon uses a PhotonView to 'claim' something that belongs to a user. So we first add a PhotonView component to the player prefab.

We're now going to edit the _PlayerTankController.cs_ to a new version, so it supports networking.

- To support the extended functions from Photon the class must add the following:
```C#
using Photon.Pun;

public class PlayerTankController : MonoBehaviourPunCallbacks
```

- Inside the Update(), all controls are being called regardless who's the owner so we've got to fix this first.

- We'll wrap around the Movement() and Shooting() inside the following statement:
```C#
if (photonView.IsMine)
{
    // Methods here
}
``` 
This will cause the controls only to work on the owner's machine.
The only thing is, when shooting these bullets will be instantiated locally.

Photon has a different way to instantiate object across a network.
So let's change our Instantiate().

```C#
PhotonNetwork.Instantiate("Bullet", SpawnPoint.position, SpawnPoint.rotation);
```

Photon requires a string for a prefab name. And we need to add a folder named _Resources_ where all networked prefabs have to be.
Because a bullet will be instantiated across the network it also needs a PhotonView Rigidbody Component. So add that one to the prefab.
- In this new PhotonView drag it's rigidbody to the observed component. This will cause Photon to sync the cube's position/rotation across the network.

- We also need to change the Bullet script so it's being destroy across the network.

```C#
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    [Tooltip("The prefab to use for representing the player")]
    public GameObject PlayerPrefab;

    void Start()
    {
        Instance = this;
    }

    #region Photon Callbacks


    /// <summary>
    /// Called when the local player left the room. We need to load the launcher scene.
    /// </summary>
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    public void SpawnPlayer()
    {
        PhotonNetwork.Instantiate(this.PlayerPrefab.name, new Vector3(0f, 0.5f, 0f), Quaternion.identity, 0);
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
        }
    }


    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects


        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom

            LoadArena();
        }
    }


    #endregion


    #region Public Methods


    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }


    #endregion

    #region Private Methods


    void LoadArena()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
        }
        Debug.Log("PhotonNetwork : Loading Launcher Level");
        PhotonNetwork.LoadLevel(0);
    }


    #endregion

}
```

## Step 13:
TODO: Adding UI en spawn player.