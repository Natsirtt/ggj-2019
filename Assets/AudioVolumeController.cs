using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioVolumeController : MonoBehaviour
{
    public AudioSource ObjectAudioSource;

    public float FadeSpeed = 1f;

    public float CameraXYMaxDist = 350.0f;
    public float CameraMaxHeight = 300.0f;
    public float CameraMinHeight = 80.0f;

    float CurrentFade;
    float FadeTarget;

    float CachedVolume;

    void Start()
    {
        ObjectAudioSource = GetComponent<AudioSource>();
        CachedVolume = ObjectAudioSource.volume;
        CurrentFade = 0.0f;
        FadeTarget = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        float newFade = Mathf.MoveTowards(CurrentFade, FadeTarget, FadeSpeed * Time.deltaTime);
        float cameraHeight = 1f - Mathf.Clamp((Camera.main.orthographicSize - CameraMinHeight) / (CameraMaxHeight - CameraMinHeight), 0f, 1f);
        Vector3 vectorToCamera = transform.position - Camera.main.transform.position;
        vectorToCamera.z = 0.0f;
        float cameraXY = 1f - Mathf.Clamp(vectorToCamera.magnitude / CameraXYMaxDist, 0.0f, 1.0f);

        if(ObjectAudioSource)
        {
            ObjectAudioSource.volume = CachedVolume * newFade * cameraHeight * cameraXY;
        }

        CurrentFade = newFade;
    }
}
