using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainUIController : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI readyText;

    public static MainUIController Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void GetReady()
    {
        StartCoroutine(ReadyAnnounce());
    }

    IEnumerator ReadyAnnounce()
    {
        yield return new WaitWhile(() => Time.timeScale == 0);

        Animation readyAnim = readyText.GetComponent<Animation>();

        readyText.GetComponent<Animation>().Play("ReadySlowExpand");
        SoundManager.Instance.PlaySound(SoundFxEnum.AnnounceReady);

        yield return new WaitForSeconds(readyAnim.GetClip("ReadySlowExpand").length + 0.5f);

        readyText.text = "SHOOT!";
        readyText.GetComponent<Animation>().Play("ReadyFastExpand");
        SoundManager.Instance.PlaySound(SoundFxEnum.AnnounceShoot);

        yield return new WaitForSeconds(readyAnim.GetClip("ReadyFastExpand").length);

        MainGameManager.Instance.StartFighting();
    }

    public void PressLButton()
    {
        MainGameManager.Instance.PressL();
    }
    public void PressHButton()
    {
        MainGameManager.Instance.PressH();
    }
}
