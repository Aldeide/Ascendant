using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Client;
using Ascendant.Networking;
using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;

namespace Ascendant
{
    public sealed class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }

        public Player localPlayer;
        public GameObject ui;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void Start()
        {
        }

        private void Update()
        {
            if (!IsServer) return;
        }

        public void ActivateUI()
        {
            if (ui != null)
            {
                ui.SetActive(true);
            }
        }

    }
}
