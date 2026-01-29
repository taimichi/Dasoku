using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smoke : MonoBehaviour
{
    [SerializeField] private Animator anim;

    void Start()
    {
        
    }


    void Update()
    {
        if (this.gameObject.activeSelf)
        {
            if(anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
            {
                this.gameObject.SetActive(false);
            }
        }
    }
}
