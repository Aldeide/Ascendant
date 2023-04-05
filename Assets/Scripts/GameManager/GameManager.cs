using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Client;

namespace Ascendant
{


    public class GameManager : MonoBehaviour
    {
        public GameObject playerPrefab;
        public Dictionary<ushort, GameObject> connectedPlayers = new Dictionary<ushort, GameObject>();
        public ushort localPlayerId;
        public static GameManager Instance;
        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this);
        }
        // Start is called before the first frame update
        void Start()
        {
            localPlayerId = ConnectionManager.Instance.Client.ID;
            Debug.Log("Local Player ID set to: " + localPlayerId);
            ConnectionManager.Instance.SpawnPlayerOnServerRequest();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SpawnLocalPlayer(SpawnLocalPlayerResponseData data)
        {
            if (!connectedPlayers.ContainsKey(data.ID))
            {
                Instantiate(playerPrefab, playerPrefab.transform.position, Quaternion.identity);
            }
        }
    }
}
