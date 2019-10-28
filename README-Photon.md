# Photon Networking Setup
In this guide we will take you through all the steps to install Photon and add multiplayer to our tank game.

**We recommend using Unity _2019.2.3f1_.**

## Step 1: 
Open Multiplayer-Base Photon as a Unity Project. This will be used to implement Photon Networking.

## Step 2:
Inside Unity navigate to the _Asset Store_ search for  _PUN 2 - FREE_. Download and import this asset.

Link to the correct version: https://assetstore.unity.com/packages/tools/network/pun-2-free-119922.

To save some space on your PC, you can uncheck the demo folders when importing PUN.

## Step 3:
Meanwhile PUN is being imported, navigate to: https://www.photonengine.com/pun and sign up for an account.

## Step 4:
When you're account is working, head over to https://dashboard.photonengine.com/en-US/publiccloud to see all your applications.

![Photon Create App](https://github.com/JurianB/Unity-Multiplayer/blob/master/Images/Photon/Photon_Setup1.png)

- Create a new app:
- **Be careful to choose _PHOTON PUN_ as a type. This will add Unity specific implementations.**
- Give it a name and create it

![Photon Create App](https://github.com/JurianB/Unity-Multiplayer/blob/master/Images/Photon/Photon_Setup2.png)

## Step 5:
Go to your created app by clicking on manage and find its App Id, this will be used to setup Networking with Unity. Go back to Unity and insert it in the settings.

![Setup Settings](https://github.com/JurianB/Unity-Multiplayer/blob/master/Images/Photon/Step1_Setup.png)

And add your id in App Id Realtime:

![Setup Settings](https://github.com/JurianB/Unity-Multiplayer/blob/master/Images/Photon/Step2_Setup.png)

## Step 6:
For testing purposes, head to the _Room_ scene. Drag the player prefab on the playground.

Now when running the game, you can move around by pressing your WASD-keys. Look around by moving your mouse and you can shoot by pressing your left mouse button.

## Step 7:
What we need now is a scene that lets the player join or create a room, so switch over to the _Lobby_ scene.

Now we're ready to setup a script that will cause Photon to host a game for us.

- In the Hierachy, head over to the empty gameobject called _Launch Manager_.
-Open the script, called _Launcher_. 

**Let the coding begin....**

In the Launcher.cs you see this class extends from MonoBehaviour (which Unity made for us). Photon uses it's own version of this, which adds networking features.

We'll swap MonoBehaviour with _MonoBehaviourPunCallbacks_ and import the next two things:

```C#
using Photon.Pun;
using Photon.Realtime;
```

## Step 8:
Replace the empty method Connect with the following:

```C#
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

## Step 9:
_MonoBehaviourPunCallbacks_ uses callbacks when using networking.
So when a client is connected to a server, Photon callbacks will be called to execute something.

We're going to implement these callbacks right now.
The following code will be called when Photon has been connected / disconnected.

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

These callbacks will also load a new level when joining or hosting a game.
To set up these loading scenes go to: _File > Build Settings_.

The scene order is:
(0): Lobby
(1): Room

To add these scenes you can drag and drop them into the scene build window.

![Build Settings](https://github.com/JurianB/Unity-Multiplayer/blob/master/Images/Photon/Build_Settings.png)

## Step 10:
NOWW, when playing the game, press join! 
A new scene is loaded and you can move around!

The thing is, the controls aren't made for Networking yet... So let's fix this!

## Step 11:
Photon uses a PhotonView to identify an object across the network (viewID) and configures how the controlling client updates remote instances.

So we first add a PhotonView component to the player prefab to make sure it will be configured for networking.

We're now going to edit the _PlayerTankController.cs_ to a new version, so it supports networking.

- To support the extended functions from Photon the class must add the following:
```C#
using Photon.Pun;
```

- Inside the the script add the following:
```C#
private PhotonView _photonView;

...
void Start()
{
    _photonView = GetComponent<PhotonView>();
}
```

- Inside the Update(), all controls are being called regardless who's the owner so we've got to fix this first.

- Above the Movement(), Shooting() and health check, we'll add the following statement:
```C#
if (!photonView.IsMine) return;

    Movement();
    Shooting();
    ....
``` 
This will cause the controls only to work on the owner's machine.
The only thing is, when shooting these bullets will be instantiated locally.

Photon has a different way to instantiate objects across a network.
So let's change our Instantiate() into this, in the Shooting method:

```C#
PhotonNetwork.Instantiate("Bullet", SpawnPoint.position, SpawnPoint.rotation);
```

Next up, we need to add damage to the other player when hit.
Now RPC comes along. RPC stands for Remote Procedure Calls. It will execute a function on a different machine.

We need to tell the player who's been hit, that he needs to add some damage from the BulletScript.

- We also need to change the Bullet script so it's being destroyed across the network. So there are some minor changes as well in there.

The new version of the _BulletScript_ of it is going to be:
```C#
using System.Collections;
using Photon.Pun;
using UnityEngine;

public class BulletScript : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private int _damage = 50;

    void Start()
    {
        if (photonView.IsMine)
        {
            StartCoroutine(DestroyBullet());
            GetComponent<Rigidbody>().AddForce(transform.forward * 450f);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        var hit = other.gameObject;
        if (hit.CompareTag("Player"))
        {
            //hit.GetComponent<PlayerTankController>().AddDamage(_damage);
            hit.GetComponent<PhotonView>().RPC("AddDamage", RpcTarget.All, _damage);

            //Now using PhotonNetwork.Destroy()...
            PhotonNetwork.Destroy(gameObject);
        }
    }

    IEnumerator DestroyBullet()
    {
        yield return new WaitForSeconds(2);

        //Now using PhotonNetwork.Destroy()...
        PhotonNetwork.Destroy(gameObject);
    }
}
```
By extending the class with _MonoBehaviourPunCallbacks_, we can use use the basic MonoBehaviour with an extension on Photon's own implementation features that adds more networking API's.

Instead of calling the AddDamage method. It now tells the server that he needs to send it across the network to other clients.
This will then say, hey, you, execute this function (locally).

On the PlayerTankControl.cs, we only need to add the following attribute above our AddDamage method and it will be available to be called by RPC calls.
```C#
[PunRPC]
```

The first parameter of PhotonNetwork.Instantiate is a string which defines the "prefab" to instantiate. Internally, PUN will fetch the GameObject from the PhotonNetwork.PrefabPool, set it up for the network and enable it. Because of this we need to add a folder named _Resources_ where all networked prefabs have to be.

Because a bullet will be instantiated across the network it also needs a Photon Rigidbody View Component. So add that one to the prefab.
- Drag the Photon Rigidbody View into the observed component attribute in the object's Photon View. 
This will cause Photon to sync the cube's position/rotation across the network.

![Bullet Setup](https://github.com/JurianB/Unity-Multiplayer/blob/master/Images/Photon/BulletSetup.png)


## Step 12:
Switch to your Room scene, where all players will be playing and we'll add new thing over here.

To control all players spawning, joining a room or leaving one, we have to use a game manager.
- Go to the empty GameObject named Game Manager and open the script called GameManager.

You have to edit the script and add the missing parts. These parts will add networking for you.

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

        PhotonNetwork.LeaveRoom();
    }


    #endregion

}
```

All these methods will handle things when a player joins a room or when leaving one.
Drag the player prefab into the PlayerPrefab field to make sure it's not empty when running the game.

## Step 13:
The final steps will contains a spawn button and a leave button. In this way a new player can spawn into the game and play along.

- Enable _Canvas_ and you're good to go

## Step 14:
- Go to your player prefab and remove it from the scene. 
- In the Project view, drag your player into the Resources folder since it will be instantiated across the network as well.

The last thing we need to do is to make sure our PhotonView attached to the player will oberserve the right things.
- Drag its Rigidbody into the player prefab. This will sync his position
- Drag its Transform into here as well. Will sync his rotation. Only check its rotation since we use transform rotation in the script.

## Step 15:
Run the game from the lobby and you're good to go.

**Tip: change player settings to enable resolution dialog. In this way you can easily run a small windowed version.**

Now build the game and see how awesome your new multiplayer game is!
