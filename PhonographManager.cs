using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This manager class is placed on the "CylinderPlaceholder" child of the phonograph object. It controls everything to do with the addition and removal of cylinder to the phonograpgh.
/// </summary>
public class PhonographManager : MonoBehaviour
{

    //private GameObject currentlyActiveCylinder; //Cylinder currently in the phonograph
    //For the variable below, I was using this to keep a reference to the script of the currently active cylinder. This would cut down on some long calls like cylinder.GetComponent<VRTK/PhonographCylinder>().someMethod. Feel free to change to use this.
  //  private VRTK.PhonographCylinder currentlyActiveCylinderScript 
    private bool cylinderAlreadyInPhonograph;
    [SerializeField]
    private Transform needle; //This is the large part of the gramophone that will move as the audio plays
    private Vector3 needleStartPos; //Pos it should be placed at when audio starts
    [SerializeField]
    private Transform needleEndPos; //This is transform rather than Vector3 for ease of moving it in the scene
    private bool moveNeedle = false; //Only set to true when a cylinder is in the phonograph
    private float resetMoveSpeed = 10; //How fast to move the needle when reseting the moving part of the phonopragh
    private bool resetNeedle = false; //true when cylinder taken from phonograph

    //Variables for calculating the speed at which to move the needled for a given song
    private float finishedTime; //The time it takes for the audio clip to finish. IE the length of the audioclip.
    private float needleSpeed; //How fast to move the moving part of the phonograph
    private float distanceToTravel; //How far the moving part of the phonograph needs to travel to reach the end

    private int pickedUpCylinders = 0; //Keeps track of how many cylinders the player is currently holding

    private AudioSource audioSource;

    private float minDistanceToDestination = 0.01f;

    // Use this for initialization
    void Start()
    {
        InitialSetup();
    }

    /// <summary>
    /// This method does some dirty setup at the start for some things that don't need their own function.
    /// </summary>
    private void InitialSetup()
    {
        GetComponentInChildren<Renderer>().enabled = false;
        needleStartPos = needle.localPosition;
        distanceToTravel = Vector3.Distance(needle.transform.localPosition, needleEndPos.transform.localPosition);
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// When a cylinder is placed in to the phonograph this is called and multiple checks are done.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<VRTK.PhonographCylinder>() != null)
        {
            if (!cylinderAlreadyInPhonograph) //No cylinder already in phonograph, so proceed...
            {
                if (other.GetComponent<VRTK.PhonographCylinder>().GetInsidePhonographTrigger() == false) //The cylinder that has entered the trigger isnt the one already in the phonograph
                {
                    other.GetComponent<VRTK.VRTK_InteractableObject>().ForceReleaseGrab(); //Force release of the cylinder so we are no longer holding it
                    PutCylinderInPhonograph(other.gameObject);
                }
            }
            else
            { //There IS already a cylinder in the phonograph so just make the player drop the cylinder they are holding (which will reset it). You cant add two cylinders to the phonograph. 
                other.GetComponent<VRTK.VRTK_InteractableObject>().ForceReleaseGrab();
            }

        } //Please feel free to put your own code here. I do not know how you want the phonograph to behave if you try to put any other objects in to the slot. Add else statement if you like.
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<VRTK.PhonographCylinder>() != null)
        {
            if (cylinderAlreadyInPhonograph) //thre is already a cylinder in the phonograph 
            {
                if (other.gameObject.GetComponent<VRTK.PhonographCylinder>().GetInsidePhonographTrigger() == true) //The object exiting the trigger is the one that was just in the phonograph(IE You are taking it out now)
                {
                    TakeCylinderFromPhonograph(other.gameObject);
                    other.gameObject.GetComponent<VRTK.PhonographCylinder>().SetInsidePhonographTrigger(false);
                }
                else
                {
                    other.GetComponent<VRTK.VRTK_InteractableObject>().ForceReleaseGrab();
                }
            }
        }
    }

    /// <summary>
    /// Places a cylinder successfully in the phonograpgh. Handles the actions that need to be taken once the cylinder is placed in the phonograph
    /// </summary>
    /// <param name="cylinder"></param>
    private void PutCylinderInPhonograph(GameObject cylinder)
    {
        VRTK.PhonographCylinder scriptOnCylinder = cylinder.GetComponent<VRTK.PhonographCylinder>(); //Local ref to avoid long GetComponent calls.

        //There is no cylinder currently in the phonograph and we are putting this one in it
        cylinder.transform.position = transform.position;
        cylinder.transform.rotation = transform.rotation;

        audioSource.clip = scriptOnCylinder.ReturnAudioClip();
        cylinderAlreadyInPhonograph = true;
        scriptOnCylinder.SetInsidePhonographTrigger(true); 
          
        finishedTime = scriptOnCylinder.ReturnAudioClipLength();

        scriptOnCylinder.RotateCylinderControl(true);
        scriptOnCylinder.UseGravityControl(false);

       // resetNeedle = false;  //UNCOMMENT THESE IF YOU DECIDE TO USE UPDATE
      //  moveNeedle = true;
        needleSpeed = distanceToTravel / finishedTime; //Calculate the speed at which to move the needle
        audioSource.PlayOneShot(audioSource.clip);
        HandleMoveNeedle(true);
    }

    /// <summary>
    /// Returns true if a cylinder is already in the phonograph.
    /// </summary>
    /// <returns></returns>
    public bool ReturnIsCylinderInPhonograph()
    {
        return cylinderAlreadyInPhonograph;
    }

    /// <summary>
    /// Called when you take a cylinder our of the phonograph. Sets the alreadyInPhonograph bool to false and does some reset information
    /// </summary>
    private void TakeCylinderFromPhonograph(GameObject cylinderTaken)
    {
        VRTK.PhonographCylinder removedCylinderScript = cylinderTaken.GetComponent<VRTK.PhonographCylinder>(); //local variable to prevent loads of GetComponent<> calls.
        cylinderAlreadyInPhonograph = false;

        //  moveNeedle = false; //UNCOMMENT THESE IF YOU DECIDE TO USE UPDATE
        // resetNeedle = true;

        //Reset phonograph needle
        HandleMoveNeedle(false);

        StopPlayingMusic();

        //Reset cylinder properties after it has been removed
        removedCylinderScript.RotateCylinderControl(false);
        removedCylinderScript.UseGravityControl(true);
        removedCylinderScript.ActivateCylinderOutline();
    }

    private void SetNeedleToStartPos()
    {
        needle.localPosition = needleStartPos;
    }

    private void StopPlayingMusic()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    
    /// <summary>
    /// This will replace the Update function. Needs to have LeanTween set up correctly.
    /// </summary>
    /// <param name="move"></param>
    void HandleMoveNeedle(bool move)
    {
        if (move)
        {
            if (Vector3.Distance(needle.localPosition, needleEndPos.localPosition) > minDistanceToDestination)
            {
                LeanTween.pause(needle.gameObject);
                LeanTween.move(needle.gameObject, needleEndPos, finishedTime).setEase(LeanTweenType.linear);
            }
            else
            {
                moveNeedle = false; //Already finished the track
            }
        }
        else { //move is false so reset the needle back to the start
            LeanTween.pause(needle.gameObject);
            LeanTween.moveLocal(needle.gameObject, needleStartPos, 0.5f).setEase(LeanTweenType.linear);
        }
    }

    /**
    void Update()
    {
        if (moveNeedle)
        {
            if (Vector3.Distance(needle.localPosition, needleEndPos.localPosition) >minDistanceToDestination)
            {
                needle.localPosition = Vector3.MoveTowards(needle.localPosition, needleEndPos.localPosition, (Time.deltaTime * needleSpeed));
            }
            else
            {
                moveNeedle = false; //Already finished the track
            }
        }

        if (resetNeedle)
        {
            if (Vector3.Distance(needle.localPosition, needleStartPos) > minDistanceToDestination)
            {
                needle.localPosition = Vector3.MoveTowards(needle.localPosition, needleStartPos, (Time.deltaTime * resetMoveSpeed));
            }
            else
            {
                resetNeedle = false; //Already finished the track
            }
        }
    }
    */

    /// <summary>
    /// Increases the int that tracks the number of cylinders currently being help by the player
    /// </summary>
    public void IncreasePickedUpCylinders()
    {
        pickedUpCylinders += 1;
    }

    public void DecreasePickedUpCylinders()
    {
        pickedUpCylinders -= 1;
    }

    /// <summary>
    /// Returns the number of cylinders currently held by the player. Used when determining when to deactivate the highlight area in the phonograph
    /// </summary>
    /// <returns></returns>
    public int ReturnPickedUpCylinderCount()
    {
        return pickedUpCylinders;
    }
}
