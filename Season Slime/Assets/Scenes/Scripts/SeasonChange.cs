using UnityEngine;
using Unity.Cinemachine;

public class SeasonChange : MonoBehaviour
{
    //[SerializeField] private CinemachineCamera vcam;
    [SerializeField] private int season;
    [SerializeField] private GameObject SpringSlime;
    [SerializeField] private GameObject SummerSlime;
    [SerializeField] private GameObject AutumnSlime;
    [SerializeField] private GameObject WinterSlime;
    [SerializeField] private FixedUI fixedUI;   // Inspector 拖入 UI

    private Animator anim;
    private GameObject targetPlayer;
    private Vector3 playerPos;

    private System.Collections.IEnumerator DelayedCreate(GameObject slimePrefab)
    {
        anim.SetBool("ChangeState", true);
        yield return new WaitForSeconds(1f);
        playerPos = targetPlayer.transform.position;
        GameObject newSlime = Instantiate(slimePrefab, playerPos, Quaternion.identity);
        if (targetPlayer != null)
            Destroy(targetPlayer);
        //vcam.Follow = newSlime.transform;
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        anim = other.GetComponent<Animator>();
        if (other.CompareTag("Player"))
        {
            targetPlayer = other.gameObject;
            if (season == 1)
            {
                //winter to spring
                StartCoroutine(DelayedCreate(SpringSlime));
                print("changed to season 1");
            }
            else if (season == 2)
            {
                //spring to summer
                StartCoroutine(DelayedCreate(SummerSlime));
                print("changed to season 2");
            }
            else if (season == 3)
            {
                //summer to fall
                StartCoroutine(DelayedCreate(AutumnSlime));
                print("changed to season 3");
            }
            else if (season == 4)
            {
                //fall to winter
                StartCoroutine(DelayedCreate(WinterSlime));
                print("changed to season 4");
            }

            // 通知 UI 更新背景和数字
            if (fixedUI != null)
                fixedUI.SetSeason(season);
        }
    }
}