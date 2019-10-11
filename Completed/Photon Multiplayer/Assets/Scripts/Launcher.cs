using Photon.Pun;
using Photon.Realtime;

using UnityEngine;

public class Launcher : MonoBehaviourPunCallbacks
{
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

    private void Start()
    {
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);
    }


    #region Public Methods

    /// <summary>
    /// Start the connection process.
    /// - If already connected, we attempt joining a random room
    /// - if not yet connected, Connect this application instance to Photon Cloud Network
    /// </summary>
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


    #endregion

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
}
