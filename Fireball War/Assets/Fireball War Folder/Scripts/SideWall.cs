using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideWall : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("collide");

        if (!other.CompareTag("Projectile"))
            return;

        other.GetComponent<BasicFireball>().ResetFireBall();
    }
}
