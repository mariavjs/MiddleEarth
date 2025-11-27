using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public CinemachineCamera GroundCamera;
    public CinemachineCamera HellCamera;

    private void Start()
    {
        GroundCamera.Priority = 10;

        PitManager.Instance.OnPlayerGoToHell += OnPlayerGoToHell;
        PitManager.Instance.OnPlayerBackFromHell += OnPlayerBackFromHell;
    }

    private void OnPlayerGoToHell()
    {
        HellCamera.Priority = 10;
        GroundCamera.Priority = 0;
    }

    private void OnPlayerBackFromHell()
    {
        HellCamera.Priority = 0;
        GroundCamera.Priority = 10;
    }
}
