using System.Collections;
using System.Collections.Generic;
using FishNet;
using UnityEngine;
using UnityEngine.UI;

namespace Ascendant.Networking
{
    public sealed class MultiplayerMenu : MonoBehaviour
    {
        [SerializeField]
        private Button hostButton;
        [SerializeField]
        private Button connectButton;

        private void Start()
        {
            hostButton.onClick.AddListener(() =>
            {
                InstanceFinder.ServerManager.StartConnection();
                InstanceFinder.ClientManager.StartConnection("82.66.176.102", 7770);
            });
            connectButton.onClick.AddListener(() =>
            {
                InstanceFinder.ClientManager.StartConnection("82.66.176.102", 7770);
            });
        }
    }
}


