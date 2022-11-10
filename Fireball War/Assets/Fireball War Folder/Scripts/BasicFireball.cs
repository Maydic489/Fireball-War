using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicFireball : MonoBehaviour
{
    [HideInInspector]
    public PlayerController owner;
    [HideInInspector]
    public float travelSpeed;
    [HideInInspector]
    public int bonusSpeedJustFrame = 1;
    [HideInInspector]
    public float bonusSpeedStreak = 1;

    [SerializeField]
    BoxCollider fbCollider;
    Vector3 originalColSide;

    private void Start()
    {
    }

    public void SetupFireball()
    {
        originalColSide = fbCollider.size;
    }


    private void Update()
    {
        if(owner.isPlayer1Side)
        {
            transform.Translate((Vector3.right * travelSpeed * bonusSpeedJustFrame * bonusSpeedStreak) * Time.deltaTime);
        }
        else
        {
            transform.Translate((Vector3.left * travelSpeed * bonusSpeedJustFrame * bonusSpeedStreak) * Time.deltaTime);
        }
    }

    private void OnDisable()
    {
        bonusSpeedJustFrame = 1;
    }

    public void ActiveJustFrameBonus()
    {
        bonusSpeedJustFrame = 3;
        fbCollider.size = new Vector3(fbCollider.size.x * 3, fbCollider.size.y, fbCollider.size.z);
    }

    public void ResetFireBall()
    {
        if(fbCollider.size != originalColSide)
            fbCollider.size = originalColSide;

        owner.activeFireBall = null;
        this.gameObject.SetActive(false);
        if(owner.isPlayer1Side)
        {
            transform.position = owner.transform.position + new Vector3(0.2f,0.5f,0);
        }
        else
        {
            transform.position = owner.transform.position + new Vector3(-0.2f, 0.5f, 0);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((!other.CompareTag("Player") && !other.CompareTag("Projectile")) || other.GetComponent<PlayerController>() == owner)
            return;

        if (other.CompareTag("Projectile") && owner.isPlayer1Side)
        {
            MainGameManager.Instance.CountImpactFrame(this.transform.position + (Vector3.right * (GetComponent<BoxCollider>().size.x / 2)));
            ResetFireBall();
            return;
        }
        else if(other.CompareTag("Projectile") && !owner.isPlayer1Side)
        {
            ResetFireBall();
            return;
        }

        if (other.CompareTag("Player") && other.GetComponent<PlayerController>() != owner)
        {
            other.GetComponent<PlayerController>().GetHit(this.transform.position);
            ResetFireBall();
        }

    }
}
