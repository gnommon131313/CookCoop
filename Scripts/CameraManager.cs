using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(CinemachineVirtualCamera))]
internal sealed class CameraManager : MonoBehaviour
{
    private readonly CompositeDisposable _disposable = new();

    [SerializeField] private CinemachineTargetGroup _cameraTargetGroup;
    private CinemachineBasicMultiChannelPerlin _cinemachineBasicMultiChannelPerlin;

    internal CinemachineVirtualCamera VirtualCamera {  get; private set; }

    private float _shakeTimer;
    private float _shakeTimerBase;
    private float _intensityBase;

    private CameraManager() { }

    private void Awake()
    {
        VirtualCamera = GetComponent<CinemachineVirtualCamera>();
        _cinemachineBasicMultiChannelPerlin = VirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void LateUpdate()
    {
        void Shake()
        {
            if (_shakeTimer <= 0)
                return;

            _shakeTimer -= Time.deltaTime;

            _cinemachineBasicMultiChannelPerlin.m_AmplitudeGain =
                Mathf.Lerp(0, _intensityBase, _shakeTimer / _shakeTimerBase);
        }

        Shake();
    }

    internal void ApplyShake(float intensity, float time)
    {
        _cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intensity;

        _intensityBase = intensity;
        _shakeTimer = time;
        _shakeTimerBase = time;
    }

    internal void SetMemberForCameraTargetGroup(List<Transform> targets, float radius)
    {
        var members = _cameraTargetGroup.m_Targets;

        for (int i = members.Length - 1; i >= 0; i--)
            _cameraTargetGroup.RemoveMember(members[i].target);

        foreach (Transform target in targets)
            _cameraTargetGroup.AddMember(target, 1, radius);
    }
}
