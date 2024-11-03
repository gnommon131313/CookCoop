using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraUIAutoSize : MonoBehaviour
{
    private Camera _cameraUI;
    [SerializeField] private Camera _cameraMain;

    private void Awake() => _cameraUI = GetComponent<Camera>();

    private void LateUpdate() => _cameraUI.orthographicSize = _cameraMain.orthographicSize;
}
