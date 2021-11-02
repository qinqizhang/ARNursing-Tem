using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HCI.UD.KinectSender
{
    public class CameraWork : MonoBehaviour
    {
        #region Private Fields 

        [Tooltip("The disance in the local x-z plane to the target")]
        [SerializeField]
        private float distance = 7.0f;

        [Tooltip("The height we want the camera to be above the target")]
        [SerializeField]
        private float height = 3.0f;

        [Tooltip("The smooth time lag for the hieght of the camera.")]
        [SerializeField]
        private float heightSmoothLag = 0.3f;

        [Tooltip("Allow the camera to be offsetted vertically from the target, for example giving more view of the scenery and less ground")]
        [SerializeField]
        private Vector3 centerOffset = Vector3.zero;

        [Tooltip("Set this as false if a compoenent of a prefab being instanciated by proton network and manually call OnStartFollow when and if needed.")]
        [SerializeField]
        private bool followOnStart = false;

        // cached transform of the target 
        Transform cameraTransform;


        //maintain a flag internally ot reconnect if targt is lost or camera is switched 
        bool isFollowing;


        //represents the current velocity, this value is modified by SmoothDamp() every time you call it.
        private float heightVelocity;

        // Represents the porition we are trying to reach using SmoothDamp() 
        private float targetHeight = 100000.0f;

        #endregion

        #region Monobehaviour Callbacks

        /// <summary>
        /// Monobehaviour method called on GameObject by unity during initialization phase 
        /// </summary>
        /// 
        void Start()
        {
            //Start following the target if wanted.
            if (followOnStart)
            {
                OnStartFollowing();
            }
        }

        /// <summary>
        /// MonoBehaviour method called after all update funcitons have been called. This is useful to order script execution. 
        /// </summary>
        /// 
        private void LateUpdate()
        {
           if (cameraTransform == null && isFollowing)
            {
                OnStartFollowing(); 
            }

           // only follow is explicitly declared 
           if (isFollowing)
            {
                Apply();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Raises the start following event. 
        /// Use this when you dont know at the time of editing what to follow, typcially instances managed by the photon network.
        /// </summary>
        public void OnStartFollowing()
        {
            cameraTransform = Camera.main.transform;
            isFollowing = true;
            //we dont smooth anything, we go straight to the right camera shot.
            Cut();
        }
        #endregion

        #region Private Methods
        /// <summary>
        ///  Follow the target smoothly
        /// </summary>
        /// 
        void Apply()
        {
            Vector3 targetCenter = transform.position + centerOffset;
            // Calculate the current and target rotation angles
            float originalTargetAngle = transform.eulerAngles.y;
            float currentAngle = cameraTransform.eulerAngles.y;

            //Adjust real target angle when camera is locked 
            float targetAngle = originalTargetAngle;
            currentAngle = targetAngle;
            targetHeight = targetCenter.y + height;

            //Damp the height 
            float currentHeight = cameraTransform.position.y;
            currentHeight = Mathf.SmoothDamp(currentHeight, targetHeight, ref heightVelocity, heightSmoothLag);
            // Convert the angle into a rotation, by which we then reposition the camera
            Quaternion currentRotation = Quaternion.Euler(0, currentAngle, 0);
            // Set the position of the camera on the x-z plane to:
            // distance meters behind the target
            cameraTransform.position = targetCenter;
            cameraTransform.position += currentRotation * Vector3.back * distance;
            // Set the height of the camera
            cameraTransform.position = new Vector3(cameraTransform.position.x, currentHeight, cameraTransform.position.z);
            // Always look at the target
            SetUpRotation(targetCenter);
        }


        /// <summary>
        /// Directly position the camera to a the specified Target and center.
        /// </summary>
        void Cut()
        {
            float oldHeightSmooth = heightSmoothLag;
            heightSmoothLag = 0.001f;
            Apply();
            heightSmoothLag = oldHeightSmooth;
        }


        /// <summary>
        /// Sets up the rotation of the camera to always be behind the target
        /// </summary>
        /// <param name="centerPos">Center position.</param>
        void SetUpRotation(Vector3 centerPos)
        {
            Vector3 cameraPos = cameraTransform.position;
            Vector3 offsetToCenter = centerPos - cameraPos;
            // Generate base rotation only around y-axis
            Quaternion yRotation = Quaternion.LookRotation(new Vector3(offsetToCenter.x, 0, offsetToCenter.z));
            Vector3 relativeOffset = Vector3.forward * distance + Vector3.down * height;
            cameraTransform.rotation = yRotation * Quaternion.LookRotation(relativeOffset);
        }


#endregion
    }
}
  
