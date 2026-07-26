using Unity.Cinemachine;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private CinemachineCamera vcam;

    public void SetFollowTarget(Transform newTarget)
    {
        if (vcam == null) return;
        else
        {
            vcam.Follow = newTarget;
        }
    }
}
