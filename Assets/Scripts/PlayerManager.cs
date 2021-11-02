using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Pun.Demo.PunBasics;

namespace HCI.UD.KinectSender
{
    public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        #region Private Fields 
        [Tooltip("The Beams GameObject to control")]
        [SerializeField]
        private GameObject beams;

        //True, when user is firing
        private bool IsFiring;
        #endregion

        #region MonoBehaviour Callbacks

        /// <summary>
        /// MonoBeviour method called on GameObject by Unity during Early initialization phase.
        /// </summary>
        /// 
        private void Awake()
        {
            if (beams == null)
            {
                Debug.LogError("<Color = Red> <a>Missing</a></Color Beams Reference.", this);
            }
            else
            {
                beams.SetActive(false);
            }
            //#Imporant 
            // used in GameManger.cs: we keep track of the localPlayer instance to prevent instantiation when levels are syncronized 

            if (photonView.IsMine)
            {
                PlayerManager.LocalPlayerInstance = this.gameObject;
            }

            // #Crtical 
            // we flag as dont destroy on load so that instance survives level synchronization, thus giving a seamless experience when leveling 
            //DontDestroyOnLoad(this.gameObject);
            this.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);

        }



        /// <summary>
        /// MonoBehaviour method called on gameObject by Unity during initialization phase. 
        /// </summary>
        /// 
         void Start()
        {

            CameraWork _cameraWork = this.gameObject.GetComponent<CameraWork>();
                if (_cameraWork != null)
                {
                    if (photonView.IsMine)
                    {
                        _cameraWork.OnStartFollowing();
                    }
                }
                else
                {
                    Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab", this);
                }
            #if UNITY_5_4_OR_NEWER
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

            #endif 
            if (PlayerUiPrefab != null)
            {
                GameObject _uiGo = Instantiate(PlayerUiPrefab);
                _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);

            }
            else
            {
                Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab.", this);
            }

        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame. 
        /// </summary>
        /// 
        private void Update()
        {
            if (photonView.IsMine)
            {
                ProcessInputs();
            }
            

            //trigger beams active state 
            if (beams != null && IsFiring != beams.activeInHierarchy)
            {
                beams.SetActive(IsFiring);
            }

            if (Health <= 0f)
            {
                GameManager.Instance.LeaveRoom();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if(!photonView.IsMine)
            {
                return;
            }
            if (!other.name.Contains("Beam"))
            {
                return; 
            }
            Health -= 0.1f;
        }

        private void OnTriggerStay(Collider other)
        {
            if (! photonView.IsMine )
            {
                return;
            }

            if (!other.name.Contains("Beam"))
            {
                return;
            }
            Health -= 0.1f * Time.deltaTime;
        }

#if !UNITY_5_4_OR_NEWER
/// <summary>See CalledOnLevelWasLoaded. Outdated in Unity 5.4.
/// 
/// </summary>
            void OnLevelWasLoaded(int level)
            {
                this.CalledOnLevelWasLoaded(level);
            }
#endif


        void CalledOnLevelWasLoaded(int level)
        {
            // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
            if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
            {
                transform.position = new Vector3(0f, 5f, 0f);
            }
            GameObject _uiGo = Instantiate(this.PlayerUiPrefab);
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
        }

#if UNITY_5_4_OR_NEWER
        public override void OnDisable()
        {
            // Always call the base to remove callbacks
            base.OnDisable();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
#endif
        #endregion

        #region Custom 

        /// <summary>
        /// Process the inputs, Maintain a flag representing when the user is pressing Fire. 
        /// </summary>
        /// 
        private void ProcessInputs()
        {
            if (photonView.IsMine)
            {
                if (Input.GetButtonDown("Fire1"))
                {
                    if (!IsFiring)
                    {
                        IsFiring = true;
                    }
                }
                if (Input.GetButtonUp("Fire1"))
                {
                    if (IsFiring)
                    {
                        IsFiring = false;
                    }
                }
            }
            
        }
        #endregion

        #region Public Fields

        [Tooltip("Current Health of our Player")]
        public float Health = 1f;

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        [Tooltip("The Player's UI GameObject Prefab")]
        [SerializeField]
        public GameObject PlayerUiPrefab;


        #endregion

        #region IPunObservable implementation
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if(stream.IsWriting)
            {
                //We own this player, send the others our data
                stream.SendNext(IsFiring);
                stream.SendNext(Health);
            }
            else
            {
                // Network player, recieve data
                this.IsFiring = (bool)stream.ReceiveNext();
                this.Health = (float)stream.ReceiveNext();
            }
        }
        #endregion

        #region Private Methods

        #if UNITY_5_4_OR_NEWER
        void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
        {
            this.CalledOnLevelWasLoaded(scene.buildIndex);
        }
#endif
        #endregion
    }
}
