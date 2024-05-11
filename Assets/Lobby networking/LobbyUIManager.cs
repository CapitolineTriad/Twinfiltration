using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Twinfiltration
{

    public class LobbyUIManager : MonoBehaviour
    {
        NetworkManager networkManager;
    
        private void Awake()
        {
            networkManager = GetComponent<NetworkManager>();
        }
    }

}