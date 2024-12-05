using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge : MonoBehaviour
{
    [SerializeField]
    private Transform Target;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.transform.position = Target.position;
            Invoke("TakeDamage", 0.1f);;
        }
    }
    private void TakeDamage()
    {
        MainControl.Instance.TakeDamage(1f, this.transform.position);
    }
}
