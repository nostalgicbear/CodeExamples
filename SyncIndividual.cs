using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using RenderHeads.Media.AVProVideo;

namespace VRPanorama
{
    [DisallowMultipleComponent]
    public class SyncIndividual : MonoBehaviour
    {
        public RenderHeads.Media.AVProVideo.MediaPlayer videoPlayer;
        public float frameStep = 0; //How far through the video we currently are
        public float frameMS = 33.33333f; //How much to step through the video on each rendered frame
        public float duration; //Duration of this media player clip
        public bool execute = true;

        void Awake()
        {
            if (execute)
            {
                videoPlayer = GetComponent<MediaPlayer>();
                videoPlayer.Events.AddListener(OnVideoEvent);
            }
        }

        public void OnVideoEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
        {
            switch (et)
            {
                case MediaPlayerEvent.EventType.MetaDataReady: //Once the metadata becomes ready, we can get some info from the media player. 
                    duration = videoPlayer.Info.GetDurationMs(); //Length of the clip ()
                    //frameMS = 1000 / videoPlayer.Info.GetVideoFrameRate(); //Can uncomment this to get the frame rate from the clip directly, however for all clips it is 33.33333f
                    break;


                case MediaPlayerEvent.EventType.FirstFrameReady:
                    videoPlayer.Play();
                    videoPlayer.Pause();
                    StartCoroutine(SyncVideo());

                    break;
            }
        }
        void Start()
        {
            if (execute)
            {
                videoPlayer.OpenVideoFromFile(videoPlayer.m_VideoLocation, videoPlayer.m_VideoPath, false);
            }
        }

        IEnumerator SyncVideo()
        {
            while (true && frameStep <= duration) //While the clip has not yet reached the end 
            {
                yield return new WaitForEndOfFrame();
                videoPlayer.Control.WaitForNextFrame(Camera.main, (int)frameStep); //Doing a render with this line enabled contributes to a far more accurate render. You'll notice a significant drop in quality if this is not enabled
                videoPlayer.Control.Seek(frameStep);
                videoPlayer.Pause();


                frameStep += frameMS; //Add 33.33333f to the current framestep 
            }
        }

        public void GetFrameFromMS(int frame)
        {
            //implement this
        }
    }
}