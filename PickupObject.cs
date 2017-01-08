namespace VRTK
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    /// <summary>
    /// This is placed on the cylinders that will be put in the phonograph. This inherits from VRKTH_InteractableObject, which is one of the main scripts of the VRTK package. 
    /// </summary>
    public class PickupObject : VRTK_InteractableObject
    {

        private Vector3 positionInBox; //Position at the start of the game. 
        [SerializeField]
        private Vector3 startPositionOverride; //Lets you manually override the startPos;
        private Quaternion rotationInBox;
        [SerializeField]
        private Transform rotationOverride;

        [Header ("For collision with eyes")]
        [Tooltip("Level to load when object hits players eyes. Ignored if left blank")]
        public string levelToLoad;

        // Use this for initialization
        void Start()
        {
            StoreStartInformation();
        }


        /// <summary>
        /// Allows for the turning on and off of the cylinders gravity. Turned off when placed in the phonograph. Turned on when taken out.
        /// </summary>
        /// <param name="useGravityState"></param>
        public void UseGravityControl(bool useGravityState)
        {
            GetComponent<Rigidbody>().useGravity = useGravityState;
        }

        /// <summary>
        /// Stores the start position + rotation for when we need to return the cylinder to the box once its dropped.
        /// </summary>
        private void StoreStartInformation()
        {
            if (startPositionOverride != Vector3.zero)
            {
                positionInBox = startPositionOverride;
                rotationInBox = rotationOverride.rotation;

            }
            else
            { //No manual override given , so take the cylinders normal start pos
                positionInBox = transform.position;
                rotationInBox = transform.rotation;
            }

        }

        /// <summary>
        /// Returns start position of cylinder
        /// </summary>
        /// <returns></returns>
        public Vector3 GetPositionInBox()
        {
            return positionInBox;
        }

        /// <summary>
        /// Returns the start rotation of the cylinder when it is in the box.
        /// </summary>
        /// <returns></returns>
        public Quaternion GetRotationInBox()
        {
            return rotationInBox;
        }

        /// <summary>
        /// Override method for the VRTK_InteractableObject version of "void Grabbed". I override to add extra functionality. Once we grab our object, we also call a method to highlight the 
        /// target area in the phonograph where the player is meant to put the cylinder.
        /// </summary>
        /// <param name="currentGrabbingObject"></param>
        public override void Grabbed(GameObject currentGrabbingObject)
        {
            base.Grabbed(currentGrabbingObject);
        }

        /// <summary>
        /// Override method for the VRTK_InteractableObject version of "void Grabbed". I override to add extra functionality. This is called once a cylinder is dropped. The cylinder's position is reset and 
        /// the outline for where the cylinder should be placed in the phonograph is turned off.
        /// </summary>
        /// <param name="previousGrabbingObject"></param>
        public override void Ungrabbed(GameObject previousGrabbingObject)
        {
            base.Ungrabbed(previousGrabbingObject);
            ResetCylinderPosition();
        }

        /// <summary>
        /// Move cylinder back to its starting position (Should be inside the box). Called when a cylinder is dropped (called from Ungrabbed method)
        /// </summary>
        public void ResetCylinderPosition()
        {
            transform.position = GetPositionInBox();
            transform.rotation = GetRotationInBox();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name == "Camera (eye)") //Could check layer. Its all the same. 
            {
                if (levelToLoad.Length > 0)
                {
                    Debug.Log(gameObject.name + " just collided with head. Level to load is " + levelToLoad);
                }
            }
        }

    }
}
