using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime; 



namespace HCI.UD.KinectSender
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        #region Private Serializable Fields 

        #endregion
        /// <summary> 
        /// The maximum number of players per room. When it is full, it cant be joined by new players, and so a new room will be created. 
        /// </summary>
        /// 
        [Tooltip("The maximum number of players per room. When a room is full, it cant be joined by new players, and so a new room will be created.")]
        [SerializeField]
        private byte maxPlayersPerRoom = 4;

        #region Private Fields

        /// <summary>
        ///  This clients version number. Users are seperated by version number. 
        /// </summary>

        string gameVersion = "1";

        bool isConnecting;


        #endregion

        #region MonoBehavior CallBacks

        /// <summary>
        /// MonoBehavior method called on GameObject by Unity during early initialization phase.
        /// </summary>
        private void Awake()
        {
            //#Critical 
            //this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync thier level automatically 

            PhotonNetwork.AutomaticallySyncScene = true;
        }

        /// <summary>
        /// MonoBehavior method called on GameObject by Unity during initialization phase.
        /// </summary>


        // Start is called before the first frame update
        void Start()
        {
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
        }

        #endregion

        #region Public Fields 
        [Tooltip("The Ui Panel to let the user enter name, connect and play")]
        [SerializeField]
        private GameObject controlPanel;
        [Tooltip("The UI Label to inform the user that the connection is in progress")]
        [SerializeField]
        private GameObject progressLabel;

        #endregion

        #region Public Methods

        /// <summary>
        /// Start the connection process.
        /// If already connected, we attempt to join a random room. 
        /// If not yet connected, connect this application instance to Photon Cloud Network
        /// </summary>
        /// 


        public void Connect()
        {
            //Keep track of the will to join a room, because then when we come back from the game we will still get a callback that we are connected, so we can see what to do then.
            isConnecting = PhotonNetwork.ConnectUsingSettings();
            progressLabel.SetActive(true);
            controlPanel.SetActive(false);
            
         
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = gameVersion;
            }
        }

        #endregion

        #region MonoBehaviorPunCallbacks Callbacks

        public override void OnConnectedToMaster()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
            // #Critical: Try and join an existing room first. 

            // we dont want anything to do if we are not attempting to join a room.
            // this case where isConnecting is false is typically when you lost or quit the game, when the level is loaded, OnConnectedToMaster will be called, in that case
            // we dont want to do anything 
            if (isConnecting)
            {
                // #Critical: The first we try to do is join a potential existing room, if there is, good, else we'll be called back with OnJoinRandomFailed()
                PhotonNetwork.JoinRandomRoom();
                isConnecting = false;
            }
            
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
            Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by pun with reason {0}", cause);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("PUN basics tutorial/launcher: OnJoinRandomFailed() was called by pun. No random room available, so we create one. \nCalling: PhotonNetwork.CreateRoom");
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("PUN basics tutorial/launcher: OnJoinedRoom() was called by pun. Client is now in a room");
            // #Critical: We only load if we are the first player, else we rely on 'PhotonNetwork.AutomatiallySyncScene' to sync our instance scene.
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                Debug.Log("We load the 'Room for 1'");

                // #Critical 
                // Load the Room Level.
                PhotonNetwork.LoadLevel("Room for 1");
            }
        }

        #endregion

        // Update is called once per frame
        void Update()
        {

        }
    }
}
