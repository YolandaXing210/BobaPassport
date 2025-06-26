using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation.Samples;
using UnityEngine.XR.ARFoundation;

public class HDRLightEstimationSetter : MonoBehaviour
{
    [SerializeField] private HDRLightEstimation hdrEstimationLight;

    private void Awake()
    {
        hdrEstimationLight.cameraManager = Camera.main.GetComponent<ARCameraManager>();
    }
}
