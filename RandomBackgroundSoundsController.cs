using UnityEngine;
using System.Collections;

public class RandomBackgroundSoundsController : MonoBehaviour
{
    [SerializeField]
    AudioSource[] audioSources;
    int sourcesLengthForRand;
    [SerializeField]
    AudioClip[] soundClips;
    int clipsLengthForRand;

    int previousClip = 0;
    int previousPlace = 0;
    [SerializeField]
    int from = 10;
    [SerializeField]
    int to = 25;

    // Use this for initialization
    void Start()
    {
        clipsLengthForRand = soundClips.Length ;
        sourcesLengthForRand = audioSources.Length ;
        StartCoroutine(PlayRandom(GetRandomTime()));
    }

    IEnumerator PlayRandom(float delay)
    {
        yield return new WaitForSeconds(delay);
        int randomAudioSource = GetRandomSource();
        int randomClip = GetRandomClip();
        if (soundClips[randomClip] != null)
        {
            audioSources[randomAudioSource].Stop();
            audioSources[randomAudioSource].clip = soundClips[randomClip];
            audioSources[randomAudioSource].Play();
            StartCoroutine(PlayRandom(GetRandomTime() + soundClips[randomClip].length));
        }
        else
        {
            StartCoroutine(PlayRandom(GetRandomTime()));
        }

    }


    int GetRandomClip()
    {
        int random = 0;
        while (random == previousClip)
        {
            random = UnityEngine.Random.Range(0, clipsLengthForRand);
        }
        return random;
    }


    int GetRandomSource()
    {
        int random = 0;
        while (random == previousPlace)
        {
            random = UnityEngine.Random.Range(0, sourcesLengthForRand);
        }
        return random;
    }

    float GetRandomTime()
    {
        return UnityEngine.Random.Range(from, to);
    }



}
