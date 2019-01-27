using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientAudio : MonoBehaviour
{
    public List<AudioClip> LoopingClips = new List<AudioClip>();
    public AudioSource PlayAudioSource;
    private AudioClip CurrentClip;

    void Start()
    {
        ChangeAudioClip();
    }

    void ChangeAudioClip()
    {
        if (LoopingClips.Count > 0)
        {
            CurrentClip = LoopingClips[UnityEngine.Random.Range(0, LoopingClips.Count)];
            if (PlayAudioSource != null)
            {
                PlayAudioSource.clip = CurrentClip;
                PlayAudioSource.Play();
                PlayAudioSource.loop = false;
                Invoke("ChangeAudioClip", CurrentClip.length);
            }
        }
    }
}
