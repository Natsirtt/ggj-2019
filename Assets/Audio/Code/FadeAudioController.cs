using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeAudioController : MonoBehaviour
{
    public bool myDoUpdateVolumeAttenuation = true;
    public AudioSource myAudioSource;
    [Header("SETTINGS")]
    public Vector2 myRandomVolumeMul = new Vector2(1, 1);
    public Vector2 myPitchDiffRange = new Vector2(0, 0);
    public float myFullFadeoutRadius = 10;
    public float myFadeStrength = 0.5f;
    [Range(0, 1)]
    public float myMaxVolume = 0.75f;

    [Header("SPAWN SOUND SETTINGS")]
    public bool myDoUseSpawnSound = false;
    public bool myGoToLoopAfterSpawn = true;
    public AudioClip mySpawnAudioClip;
    [Range(0, 1)]
    public float mySpawnSoundVolume = 1.0f;

    [Header("CLIPS")]
    public List<AudioClip> myRandomAudioClips;


    void Start()
    {
        myAudioSource.pitch = myAudioSource.pitch + Random.Range(myPitchDiffRange.x, myPitchDiffRange.y);
        myAudioSource.volume = myMaxVolume * Random.Range(myRandomVolumeMul.x, myRandomVolumeMul.y); ;
        if(myDoUseSpawnSound)
        {
            PlaySpawnSound();
            if (myGoToLoopAfterSpawn)
            {
                StartLoopAfterSpawnTimer();
            }
        }

    }
    void Update()
    {
        if (myDoUpdateVolumeAttenuation)
        {
            UpdateVolumeAttenuation();
        }
    }

    public void UpdateVolumeAttenuation()
    {
        float dist = GetDistToCameraCenter(transform);
        float distPercent = dist / myFullFadeoutRadius;
        float maxVolume = Mathf.Min(1 - distPercent) * myMaxVolume;

        if (dist < myFullFadeoutRadius)
        {
            if (myAudioSource.volume + myFadeStrength * Time.deltaTime * distPercent <= maxVolume)
            {
                myAudioSource.volume = myAudioSource.volume + myFadeStrength * Time.deltaTime * distPercent; // Increases faster as you come closer
            }
            else
            {
                myAudioSource.volume = maxVolume;
            }
        }
        else
        {
            if (myAudioSource.volume > 0)
            {
                myAudioSource.volume = myAudioSource.volume - myFadeStrength * Time.deltaTime;
            }
        }
    }
    public void PlayLooping()
    {
        myAudioSource.volume = myMaxVolume * Random.Range(myRandomVolumeMul.x, myRandomVolumeMul.y);
        myAudioSource.pitch = myAudioSource.pitch + Random.Range(myPitchDiffRange.x, myPitchDiffRange.y);
        myAudioSource.Play();
        myAudioSource.loop = true;
    }
    public void PlayLooping(AudioClip anAudioClip)
    {
        myAudioSource.clip = anAudioClip;
        myAudioSource.volume = myMaxVolume * Random.Range(myRandomVolumeMul.x, myRandomVolumeMul.y);
        myAudioSource.pitch = myAudioSource.pitch + Random.Range(myPitchDiffRange.x, myPitchDiffRange.y);
        myAudioSource.Play();
        myAudioSource.loop = true;
    }
    public virtual void TryPlaySoundOneShot()
    {
        float dist = GetDistToCameraCenter(transform);
        if (dist < myFullFadeoutRadius)
        {
            PlaySoundOneShot();
        }
    }
    public virtual void PlaySoundOneShot()
    {
        if (myRandomAudioClips.Count > 1)
        {
            myAudioSource.volume = myMaxVolume * Random.Range(myRandomVolumeMul.x, myRandomVolumeMul.y);
            myAudioSource.pitch = myAudioSource.pitch + Random.Range(myPitchDiffRange.x, myPitchDiffRange.y);
            myAudioSource.PlayOneShot(myRandomAudioClips[Random.Range(0, myRandomAudioClips.Count - 1)]);
        }
        else if (myRandomAudioClips.Count == 1)
        {
            myAudioSource.volume = myMaxVolume * Random.Range(myRandomVolumeMul.x, myRandomVolumeMul.y);
            myAudioSource.pitch = myAudioSource.pitch + Random.Range(myPitchDiffRange.x, myPitchDiffRange.y);
            myAudioSource.PlayOneShot(myRandomAudioClips[0]);
        }
        else if (myAudioSource.clip != null)
        {
            myAudioSource.volume = myMaxVolume * Random.Range(myRandomVolumeMul.x, myRandomVolumeMul.y);
            myAudioSource.pitch = myAudioSource.pitch + Random.Range(myPitchDiffRange.x, myPitchDiffRange.y);
            myAudioSource.Play();
            myAudioSource.loop = false;
        }
    }  
    public void TryPlaySpawnSound()
    {
        float dist = GetDistToCameraCenter(transform);
        if (dist < myFullFadeoutRadius)
        {
            PlaySoundOneShot();
        }
    }
    public void PlaySpawnSound()
    {
        myAudioSource.volume = mySpawnSoundVolume * Random.Range(myRandomVolumeMul.x, myRandomVolumeMul.y);
        myAudioSource.pitch = myAudioSource.pitch + Random.Range(myPitchDiffRange.x, myPitchDiffRange.y);
        myAudioSource.PlayOneShot(mySpawnAudioClip);
    }
    public void StartLoopAfterSpawnTimer()
    {
        StartCoroutine(PlayLoopSoundTimer(mySpawnAudioClip.length));
    }
    IEnumerator PlayLoopSoundTimer(float aDuration)
    {
        yield return new WaitForSeconds(aDuration);
        if (myRandomAudioClips.Count > 1)
        {
            PlayLooping(myRandomAudioClips[Random.Range(0, myRandomAudioClips.Count)]);
        }
        else if(myRandomAudioClips.Count == 1)
        {
            PlayLooping(myRandomAudioClips[0]);
        }
    }

    private float GetDistToCameraCenter(Transform aCompareTransform)
    {
        Camera mainCam = Camera.main;
        Vector3 cameraCenterPoint = mainCam.ScreenToWorldPoint(new Vector3(mainCam.scaledPixelWidth / 2, mainCam.scaledPixelHeight / 2, 0));
        Vector3 playPoint = aCompareTransform.position;
        playPoint.z = cameraCenterPoint.z;

        return Vector3.Distance(cameraCenterPoint, playPoint);
    }
}
