using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
/// <summary>
/// This script controls the playing of audioClips from a list. Audio will only be played if a collision occurs baed on the triggerLayer. Once all audio clips have been played, the list re-shuffles. No duplicate audio or
/// empty values can be added. 
/// </summary>
public class AudioClipManager : MonoBehaviour
{
    [Tooltip ("The layer that objects must be on to trigger the audio to play")]
    public LayerMask triggerLayer; //Anything on this layer will trigger the sounds. If player is on "PlayerLayer" set this to "PlayerLayer"

    public List<AudioClip> audioClips = new List<AudioClip>();
    private int clipToPlay = 0; //index for audioClips list
    AudioSource audioSource;

    // Use this for initialization
    void Start()
    {
        CheckForCollider(); 
        CheckForAudioSource(); //Make sure there is an audiosource
        GenerateInitialArray(); //Generates the first array to use. 
    }

    /// <summary>
    /// This generates the initial array that will be used by the game object. Called from Start
    /// </summary>
    void GenerateInitialArray()
    {
        CheckForEmptyArray(); //Check at the beginning to make sure array is not null
        audioClips = RemoveNullEntries(audioClips); //First remove any null entries. No point in having a list with blanks in it
        audioClips = RemoveDuplicateEntries(audioClips); //Second, remove any duplicates from the list. There is no need to have any duplicates in the audioclip array.
        CheckForEmptyArray(); //Check after modifications to make sure array isnt null. 
        audioClips = ShuffleAudioClips(audioClips); // Now randomize the array and store the result. This is our starting audioClips list
    }

    /// <summary>
    /// Returns an error if the audioClips array is null
    /// </summary>
    void CheckForEmptyArray()
    {
        if (audioClips.Count == 0)
        {
            Debug.LogError("The Audioclip array on " + gameObject.name + " is empty. Please add audio clips to the array");
        }
    }

    /// <summary>
    /// Removes any duplicate entries from the array. There is no need to have any sound there more than once.
    /// </summary>
    /// <param name="list"></param>
    List<AudioClip> RemoveDuplicateEntries(List<AudioClip> list)
    {
        list = list.Distinct().ToList();

        return list;
    }

    /// <summary>
    /// Removed any null entries from the audio list array
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    List<AudioClip> RemoveNullEntries(List<AudioClip> list)
    {
        for (int i = list.Count - 1; i > -1; i--)
        {
            if (list[i] == null)
            {
                list.RemoveAt(i);
            }
        }

        return list;
    }

    /// <summary>
    /// Resets the variable that determines what clip to play from the audioClips array. Placed in its own function simply for neatness
    /// </summary>
    private void ResetClipToPlay()
    {
        clipToPlay = 0;
    }

    /// <summary>
    /// Shuffles the audioClips. Does a check to make sure that the last element in the array is not placed at the beginning of the shuffled array. Will re-shuffle if it is. 
    /// </summary>
    /// <param name="audioClipsToShuffle"></param>
    /// <returns></returns>
    List<AudioClip> ShuffleAudioClips(List<AudioClip> audioClipsToShuffle)
    {
        ResetClipToPlay();
        if (audioClipsToShuffle.Count <= 1) //If there is only one entry it cant be shufled, so just return.
        {
            return audioClipsToShuffle;
        }
        List<AudioClip> tempList = new List<AudioClip>(); //Create a temp list to store the list we want to shuffle. Will be used for comparison later

        for (int i = 0; i < audioClipsToShuffle.Count; i++)
        {
            tempList.Add(audioClipsToShuffle[i]);
        }

        for (int i = 0; i < audioClipsToShuffle.Count; i++) //Shuffle the list passed in to the function
        {
            AudioClip temp = audioClipsToShuffle[i];
            int randomIndex = Random.Range(i, audioClipsToShuffle.Count);
            audioClipsToShuffle[i] = audioClipsToShuffle[randomIndex];
            audioClipsToShuffle[randomIndex] = temp;
        }

        //Check if the last element if the original list is the first in the new list. If it is, generate a new random list
        if (tempList[tempList.Count - 1] == audioClipsToShuffle[0])
        {
            ShuffleAudioClips(tempList); //Pass back in the original list and shuffle again
        }

        return audioClipsToShuffle;
    }

    /// <summary>
    /// Takes in two lists and compares the last . 
    /// </summary>
    /// <param name="unShuffled">The list before it was shuffled</param>
    /// <param name="shuffled">The list after it was shuffled</param>
    /// <returns></returns>
    int CompareLists(List<AudioClip> unShuffled, List<AudioClip> shuffled)
    {
        int similarities = 0;

        if (unShuffled.Count != shuffled.Count)
        {
            Debug.LogError("Lists must be the same length to compare");
        }

        for (int i = 0; i < unShuffled.Count; i++)
        {
            for (int j = 0; j < shuffled.Count; j++)
            {
                if (string.Compare(unShuffled[i].name, shuffled[j].name) == 0)
                {
                    similarities += 1;
                }
            }
        }
        return similarities;
    }

    /// <summary>
    /// Plays a sound from the audioclips array. If it reaches the end of the array, it will shuffle again
    /// </summary>
    private void PlaySoundFromArray()
    {
        if (!audioSource.isPlaying) //Only play if a sound isnt already playing. Othwerwise sounds can get cut off
        {
            audioSource.PlayOneShot(audioClips[clipToPlay]);
            if (clipToPlay + 1 < audioClips.Count) //Check to see if we are at the last element in the array. 
            {
                clipToPlay += 1;
            }
            else
            { //Last audio clip has been played so re-shuffle
                audioClips = ShuffleAudioClips(audioClips);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        int otherBit = 1 << other.gameObject.layer;
        if (otherBit == triggerLayer.value) //Object colliding with trigger is on the same layer as the selected triggerLayer
        {
            PlaySoundFromArray();
        }
    }

    /// <summary>
    /// If there is no collider on the object, we will add a sphere collider and set it to be a trigger.
    /// IMPORTANT : This method should never really need to exist. The colliders should carefully positioned by somone when desiging the level. Adding them like this is a bad idea. It exists just incase someone forgot.
    /// </summary>
    void CheckForCollider()
    {
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<SphereCollider>();
            gameObject.GetComponent<SphereCollider>().isTrigger = true;
        }
    }

    /// <summary>
    /// Checks for an audiosource. Will add one if there is not one on the object.
    /// </summary>
    void CheckForAudioSource()
    {
        if (GetComponent<AudioSource>() == null)
        {
            gameObject.AddComponent<AudioSource>();
        }

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }
}
