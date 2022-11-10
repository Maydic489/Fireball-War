using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public bool isPlayer1Side;
    public SPUM_Prefabs _prefabs;
    public GameObject fireBall;
    [SerializeField]
    GameObject justFrameFX;
    [SerializeField]
    GameObject textOverHeadFX;
    List<GameObject> fireBallPools = new List<GameObject>();
    List<SpriteRenderer> charSprites = new List<SpriteRenderer>();
    List<Color> spriteOriginalColors = new List<Color>();
    [HideInInspector]
    public GameObject activeFireBall;
    PlayerState _state;
    int lifePoint = 2;

    Coroutine fireBallCo;
    Coroutine whiffCo;
    Coroutine getHitCo;

    public enum PlayerState
    {
        idle,
        attack,
        whiff,
        hit,
        death
    }

    private void Start()
    {
        _state = PlayerState.idle;

        for (int i = 0; i < 2; i++)//pool for fireball
        {
            fireBallPools.Add(Instantiate(fireBall, transform.position + Vector3.up * 0.5f, fireBall.transform.rotation));
            fireBallPools[i].GetComponent<BasicFireball>().owner = this;
            fireBallPools[i].GetComponent<BasicFireball>().SetupFireball();
            fireBallPools[i].SetActive(false);
        }

        charSprites.AddRange(transform.Find("UnitRoot").transform.Find("Root").GetComponentsInChildren<SpriteRenderer>());
        foreach(var sp in charSprites)
        {
            spriteOriginalColors.Add(sp.color);
        }

        fireBallPools[0].GetComponent<BasicFireball>().travelSpeed = 5;
        fireBallPools[1].GetComponent<BasicFireball>().travelSpeed = 7;
    }

    private void Update()
    {
        if (_state != PlayerState.idle || _state == PlayerState.hit || MainGameManager.Instance._gameState != MainGameManager.GameState.Fighting)
            return;

        if (isPlayer1Side)
        {
            if (!activeFireBall)
            {
                if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.Joystick1Button2))
                {
                    InputFireBall(0);
                }
                else if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Joystick1Button3))
                {
                    InputFireBall(1);
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Joystick1Button2) || Input.GetKeyDown(KeyCode.Joystick1Button3))
                {
                    whiffCo = StartCoroutine(WhiffAttack());
                }
            }
        }
        else
        {
            if (!activeFireBall)
            {
                if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Joystick2Button2))
                {
                    InputFireBall(0);
                }
                else if (Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.Joystick2Button3))
                {
                    InputFireBall(1);
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.Joystick2Button2) || Input.GetKeyDown(KeyCode.Joystick2Button3))
                {
                    whiffCo = StartCoroutine(WhiffAttack());
                }
            }
        }
    }

    void InputFireBall(int fbIndex)
    {
        _state = PlayerState.attack;
        _prefabs.PlayAnimation("attack_normal");

        if(MainGameManager.Instance.frameSinceImpact < 3 && MainGameManager.Instance.frameSinceImpact > 0 && fbIndex == 1)//just frame
        {
            justFrameFX.SetActive(true);
            SoundManager.Instance.PlaySound(SoundFxEnum.justFrame);
            fireBallPools[fbIndex].GetComponent<BasicFireball>().ActiveJustFrameBonus();
        }
        else
        {
            SoundManager.Instance.PlaySound(SoundFxEnum.shootFireball);
        }

        fireBallCo = StartCoroutine(UseFireBall(fbIndex));
    }

    IEnumerator UseFireBall(int fbIndex)
    {
        yield return new WaitForSeconds(0.016f * 15);

        fireBallPools[fbIndex].SetActive(true);
        activeFireBall = fireBallPools[fbIndex];

        yield return new WaitForSeconds(0.016f * 10);

        if(fireBallPools[1].GetComponent<BasicFireball>().bonusSpeedStreak < 2 && fbIndex == 1)
        fireBallPools[1].GetComponent<BasicFireball>().bonusSpeedStreak += 0.05f;

        _state = PlayerState.idle;
        _prefabs.PlayAnimation("idle");
    }

    IEnumerator WhiffAttack()
    {
        _state = PlayerState.whiff;
        textOverHeadFX.SetActive(true);
        SoundManager.Instance.PlaySound(SoundFxEnum.whiff);
        _prefabs.PlayAnimation("attack_bow");

        fireBallPools[1].GetComponent<BasicFireball>().bonusSpeedStreak = 1;

        yield return new WaitForSeconds(0.016f * 50);
        _state = PlayerState.idle;
        _prefabs.PlayAnimation("idle");
    }

    void StopActionCo()
    {
        if (fireBallCo != null)
            StopCoroutine(fireBallCo);
        if (whiffCo != null)
            StopCoroutine(whiffCo);
        if (getHitCo != null)
            StopCoroutine(getHitCo);
    }

    public void GetHit(Vector3 fireBallPos)
    {
        StopActionCo();

        foreach(var fb in fireBallPools)//reset speed bonus
        {
            var basicfb = fb.GetComponent<BasicFireball>();
            basicfb.ResetFireBall();
            basicfb.bonusSpeedStreak = 1;
        }

        MainGameManager.Instance.ShowHitEffect(fireBallPos);
        StartCoroutine(SpriteFlashing());
        SoundManager.Instance.PlaySound(SoundFxEnum.fireballHit);

        lifePoint--;
        if (lifePoint <= 0 && _state != PlayerState.death)
        {
            _state = PlayerState.death;
            _prefabs.PlayAnimation("death");

            Time.timeScale = 0.2f;
            StartCoroutine(EndRound());
        }
        else
        {
            _state = PlayerState.hit;
            _prefabs.PlayAnimation("stun");
            getHitCo = StartCoroutine(GetHitStun());
        }
    }

    IEnumerator GetHitStun()
    {
        yield return new WaitForSeconds(0.016f * 30);
        _state = PlayerState.idle;
        _prefabs.PlayAnimation("idle");
    }

    IEnumerator SpriteFlashing()
    {
        foreach (var sp in charSprites)
        {
            sp.color = Color.red;
        }

        yield return new WaitForSeconds(0.05f);

        for(int i=0; i<charSprites.Count;i++)
        {
            charSprites[i].color = spriteOriginalColors[i];
        }

        yield return new WaitForSeconds(0.05f);
    }

    IEnumerator EndRound()
    {
        MainGameManager.Instance._gameState = MainGameManager.GameState.GameEnd;

        while (Time.timeScale < 1)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            Time.timeScale += 0.05f;
        }

        yield return new WaitForSecondsRealtime(0.5f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
