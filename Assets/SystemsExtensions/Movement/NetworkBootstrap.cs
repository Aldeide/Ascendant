using Unity.Netcode;
using UnityEngine;

namespace Ascendant.SystemsExtensions.Movement
{
    public class NetworkBootstrap : MonoBehaviour
    {
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                StartButtons();
            }
            else
            {
                StatusLabels();
            }
            GUILayout.EndArea();
        }

        private void StartButtons()
        {
            if (GUILayout.Button("Host (Server + Client)", GUILayout.Height(40)))
            {
                NetworkManager.Singleton.StartHost();
            }
            if (GUILayout.Button("Server", GUILayout.Height(40)))
            {
                NetworkManager.Singleton.StartServer();
            }
            if (GUILayout.Button("Client", GUILayout.Height(40)))
            {
                NetworkManager.Singleton.StartClient();
            }
        }

        private void StatusLabels()
        {
            var mode = NetworkManager.Singleton.IsHost ? "Host" : 
                       NetworkManager.Singleton.IsServer ? "Server" : "Client";

            GUILayout.Label($"Transport: {NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name}");
            GUILayout.Label($"Mode: {mode}");
            
            if (NetworkManager.Singleton.IsServer)
            {
                GUILayout.Label($"Connections: {NetworkManager.Singleton.ConnectedClientsList.Count}");
            }
            else
            {
                GUILayout.Label($"Local Client ID: {NetworkManager.Singleton.LocalClientId}");
            }

            if (GUILayout.Button("Disconnect", GUILayout.Height(30)))
            {
                NetworkManager.Singleton.Shutdown();
            }
        }
    }
}
