using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraFollowType
{
    Lerp,
    hard,
}

public class EasyCamera : MonoBehaviour
{
    //获取目标
    public Transform target;

    //相机跟随速度
    [SerializeField]
    private float smoothSpeed = 1;

    //相机位置偏移
    [SerializeField]
    private Vector3 offset = new Vector3(0, 0, -10);

    [SerializeField]
    private CameraFollowType cameraFollowType = CameraFollowType.hard;


    private void Start()
    {
        if (target != null)
        {
            MonoManager.Instance.AddLateUpdateListener(Follow);
        }
        else
        {
            Debug.LogError("Camera target is null");
        }
    }

    private void Follow()
    {
        switch (cameraFollowType)
        {
            case CameraFollowType.Lerp:
                this.transform.position = Vector3.Lerp(this.transform.position, target.position + offset, Time.unscaledDeltaTime * smoothSpeed);
                break;
            case CameraFollowType.hard:
                this.transform.position = target.position + offset;
                break;
        }
            
    }

    private void OnDestroy()
    {
        MonoManager.Instance.RemoveLateUpdateListener(Follow);
    }
}
