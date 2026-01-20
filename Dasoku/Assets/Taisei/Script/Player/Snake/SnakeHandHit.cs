using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeHandHit : MonoBehaviour
{
    private bool isHit = false;

    /// <summary>
    /// Žè‚ª’n–Ê‚Æ“–‚½‚Á‚½‚©‚Ç‚¤‚©‚ð•Ô‚·
    /// </summary>
    /// <returns>false=“–‚½‚Á‚Ä‚È‚¢ / true=“–‚½‚Á‚½</returns>
    public bool ReturnIsHit() => isHit;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            isHit = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            isHit = false;
        }
    }
}
