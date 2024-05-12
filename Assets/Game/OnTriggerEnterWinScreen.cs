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
