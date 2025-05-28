using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackArea : MonoBehaviour
{
    private int damage = 3;

    private void OnTiggerEnter2D(Collider2D collision)
    {
        if (GetComponent<Collider>().GetComponent<Health>() != null)
        {
            Health health = GetComponent<Collider>().GetComponent<Health>();
            health.Damage(damage);
        }
    }
}
