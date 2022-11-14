using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGameManager : MonoBehaviour
{

    [SerializeField]
    PlayerController player1;
    [SerializeField]
    PlayerController player2;
    [SerializeField]
    GameObject clashEffect;
    List<GameObject> clashEffectObjs = new List<GameObject>();
    [SerializeField]
    GameObject hitEffect;
    GameObject hitEffectObj;

    public bool isOnlinePlay;

    Coroutine countImpactFrameCo;

    [HideInInspector]
    public int frameSinceImpact;

    public bool isPressL;
    public bool isPressH;

    public enum GameState
    {
        PreStart,
        Fighting,
        GameEnd
    }

    [HideInInspector]
    public GameState _gameState;

    public static MainGameManager Instance { get; private set; }
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

    private void Start()
    {
        Application.targetFrameRate = 60;

        _gameState = GameState.PreStart;

        for (int i = 0; i < 3; i++)//pool for clash FX
        {
            clashEffectObjs.Add(Instantiate(clashEffect));
            clashEffectObjs[i].SetActive(false);
        }

        hitEffectObj = Instantiate(hitEffect);
        hitEffectObj.SetActive(false);

        MainUIController.Instance.GetReady();
    }

    public void StartFighting()
    {
        _gameState = GameState.Fighting;
    }

    public void ShowHitEffect(Vector3 effectPos)
    {
        hitEffectObj.transform.position = effectPos;
        hitEffectObj.SetActive(true);
    }

    public void CountImpactFrame(Vector3 clashPos)
    {
        SoundManager.Instance.PlaySound(SoundFxEnum.fireballClash);

        foreach(var fx in clashEffectObjs)//show clash FX
        {
            if(!fx.activeSelf)
            {
                fx.transform.position = clashPos;
                fx.SetActive(true);
                break;
            }
        }

        if(countImpactFrameCo != null)//Start counting frame for just frame
        {
            StopCoroutine(countImpactFrameCo);
        }

        countImpactFrameCo = StartCoroutine(StartCountImpactFrame());
    }

    public IEnumerator StartCountImpactFrame()
    {
        frameSinceImpact = 0;

        while (true)
        {
            yield return new WaitForEndOfFrame();

            frameSinceImpact++;
        }
    }


    public void PressL()
    {
        isPressL = true;
    }
    public void PressH()
    {
        isPressH = true;
    }
    public void ClearPressButtons()
    {
        isPressL = false;
        isPressH = false;
    }
}
