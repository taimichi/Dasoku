using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyScript : MonoBehaviour
{

    void Start()
    {
        Goal.Instance.AddKeyObject(this.gameObject);
    }


    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //ƒvƒŒƒCƒ„[‚ÆÚG‚µ‚½
        if (collision.CompareTag("Player"))
        {
            Goal.Instance.GetKey(this.gameObject);
        }
    }
}
