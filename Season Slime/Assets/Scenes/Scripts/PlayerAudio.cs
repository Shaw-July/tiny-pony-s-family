using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("一次性音效(PlayOneShot 用)")]
    public AudioSource oneShotSource;   // 手动拖:留空 clip 的那个 AudioSource
    public AudioClip jumpClip;
    public AudioClip transformClip;
    public AudioClip landClip;

    [Header("行走音效(勾 Loop 的那个)")]
    public AudioSource footstepSource;  //不用勾loop
    public AudioClip[] footstepClips;
    public float footstepInterval = 0.35f; // 行走音效播放间隔

    private float footstepTimer;

    public void HandleFootsteps(bool isWalking)
    {
        if (isWalking)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                PlayRandomFootstep();
                footstepTimer = footstepInterval; //重置计时，下一步
            }
        }
        else
        {
            footstepTimer = 0f; // Reset timer when not walking
        }
    }

    private void PlayRandomFootstep()
    {
        if (footstepClips.Length == 0) return;
        int Index = Random.Range(0, footstepClips.Length);//随机挑一个
        footstepSource.PlayOneShot(footstepClips[Index]);
    }
    public void PlayJump() { oneShotSource.PlayOneShot(jumpClip); }
    public void PlayTransform() { oneShotSource.PlayOneShot(transformClip); }
    public void PlayLand() { oneShotSource.PlayOneShot(landClip); }
}