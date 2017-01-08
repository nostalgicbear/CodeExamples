namespace VRTK
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    /// <summary>
    /// This is placed on the cylinders that will be put in the phonograph. This inherits from VRKTH_InteractableObject, which is one of the main scripts of the VRTK package. 
    /// </summary>
    public class PhonographCylinder : VRTK_InteractableObject
    {

        [SerializeField]
        private AudioClip musicClip;

        private Vector3 positionInBox; //Position at the start of the game. 
        [SerializeField]
        private Vector3 startPositionOverride; //Lets you manually override the startPos;
        private Quaternion rotationInBox;
        [SerializeField]
        private Transform rotationOverride;

        [SerializeField]
        private Renderer cylinderOutline; //The placeholder cylinder (with the outline) in the phonograph. We turn it on and off when a cylinder is picked up.

        [SerializeField]
        private PhonographManager phonographManager; //Reference to the script on the phonograph. This script controls everything that happens in relation to the phonograph.
        private bool insidePhonographTrigger; //This is a bool that is set to true when this is the cylinder in the phonograph. 

        private bool rotateCylinder = false;
        [SerializeField]
        private float rotateCylinderSpeed = 30f;

        // Use this for initialization
         void Start()
        {
            StoreStartInformation();
        }

        private void Update()
        {
            if (rotateCylinder)
            {
                transform.Rotate(Vector3.left * Time.deltaTime * rotateCylinderSpeed);
            }
        }


        /// <summary>
        /// Sets the bool that controls whether or not to rotate the cylinder
        /// </summary>
        /// <param name="rotateState"></param>
        public void RotateCylinderControl(bool rotateState)
        {
            rotateCylinder = rotateState;
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
            else { //No manual override given , so take the cylinders normal start pos
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
            Debug.Log("Rest to  : " + positionInBox);
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
            phonographManager.IncreasePickedUpCylinders();
            ActivateCylinderOutline();
        }

        /// <summary>
        /// Override method for the VRTK_InteractableObject version of "void Grabbed". I override to add extra functionality. This is called once a cylinder is dropped. The cylinder's position is reset and 
        /// the outline for where the cylinder should be placed in the phonograph is turned off.
        /// </summary>
        /// <param name="previousGrabbingObject"></param>
        public override void Ungrabbed(GameObject previousGrabbingObject)
        {
            base.Ungrabbed(previousGrabbingObject);
            phonographManager.DecreasePickedUpCylinders();
            ResetCylinderPosition();

            if (phonographManager.ReturnIsCylinderInPhonograph() == false) //Only proceed if there is not a cylinder in the phonograph. 
            {
                if (phonographManager.ReturnPickedUpCylinderCount() == 0) //Only deactivate if the player is not holding another cylinder. 
                {
                    DeactivateCylinderOutline();
                }

            }
        }

        /// <summary>
        /// Move cylinder back to its starting position (Should be inside the box). Called when a cylinder is dropped (called from Ungrabbed method)
        /// </summary>
        public void ResetCylinderPosition()
        {
            transform.position = GetPositionInBox();
            transform.rotation = GetRotationInBox();
        }

        /// <summary>
        /// Turns on Cylinder outline when a music cylinder is picked up
        /// </summary>
        public void ActivateCylinderOutline()
        {
          
            if (phonographManager.ReturnIsCylinderInPhonograph() == false)
            {
                cylinderOutline.enabled = true;
            }
        }

        /// <summary>
        /// Turns off the cylinder outline when a cylinder is placed in the phonograph
        /// </summary>
        private void DeactivateCylinderOutline()
        {
            cylinderOutline.enabled = false;
        }

        /// <summary>
        /// Returns the insidePhonographTrigger variable. 
        /// </summary>
        /// <param name="value"></param>
        public bool GetInsidePhonographTrigger()
        {
            return insidePhonographTrigger;
        }

        /// <summary>
        /// Sets the insidePhonographTrigger variable. This is set to true when the cylinder is the one currently inside the phonograph
        /// </summary>
        /// <param name="value"></param>
        public void SetInsidePhonographTrigger(bool value)
        {
            insidePhonographTrigger = value;
        }

        /// <summary>
        /// Retuns the length of the cylinders audioclip. Used to calculate the speed at which to move the moving part of the phonograph
        /// </summary>
        /// <returns></returns>
        public float ReturnAudioClipLength()
        {
            return musicClip.length;
        }

        public AudioClip ReturnAudioClip()
        {
            return musicClip;
        }

        
    }
}
