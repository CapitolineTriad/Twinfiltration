using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Twinfiltration
{
    public class OnTriggerEnterWinScreen : MonoBehaviour
    {
        [SerializeField] GameObject WinScreenParent;
        int amtInside = 0;
        [SerializeField] AudioSource WinAudio;

        private void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.GetComponent<PlayerController>() != null)
            {
                amtInside++;
                if (amtInside >= 2)
                {
                    WinScreenParent.SetActive(true);
                    WinAudio.Play();
                    PlayerController p1 = null;
                    p1 = GameObject.FindGameObjectWithTag("Player1")?.GetComponent<PlayerController>();
                    PlayerController p2 = null;
                    p2 = GameObject.FindGameObjectWithTag("Player2")?.GetComponent<PlayerController>();
                    p1.TriggerWin();
                    p2.TriggerWin();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.GetComponent<PlayerController>() != null)
            {
                amtInside--;
            }
        }
    }
}
