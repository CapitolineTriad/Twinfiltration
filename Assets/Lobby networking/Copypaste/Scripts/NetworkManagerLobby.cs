using DapperDino.Tutorials.Lobby;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DapperDino.Mirror.Tutorials.Lobby
{
    public class NetworkManagerLobby : NetworkRoomManager
    {
        //[SerializeField] new private int minPlayers = 2;
        [Scene] [SerializeField] private string menuScene = string.Empty;

        [Header("Maps")]
        [SerializeField] private int numberOfRounds = 1;
        [SerializeField] private MapSet mapSet = null;

        [Header("Game")]
        [SerializeField] private NetworkGamePlayerLobby gamePlayerPrefab = null;

        private MapHandler mapHandler;

        public static event Action OnClientConnected;
        public static event Action OnClientDisconnected;
        public static event Action<NetworkConnection> OnServerReadied;
        public static event Action OnServerStopped;

        public List<NetworkRoomPlayerLobby> RoomPlayers { get; } = new List<NetworkRoomPlayerLobby>();
        public List<NetworkGamePlayerLobby> GamePlayers { get; } = new List<NetworkGamePlayerLobby>();

        public override void OnStartServer() => spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();

        public override void OnStartClient()
        {
            base.OnStartClient();
            var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");

            foreach (var prefab in spawnablePrefabs)
            {
                //ClientScene.RegisterPrefab(prefab);
            }
        }

        public override void OnClientConnect()
        {
            base.OnClientConnect();
            Debug.Log("OnClientConnect");
            OnClientConnected?.Invoke();
        }

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            Debug.Log("OnClientDisconnect");
            OnClientDisconnected?.Invoke();
        }

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);
            Debug.Log("OnServerConnect");
            if (numPlayers >= maxConnections)
            {
                Debug.Log("numPlayers over max, disconnecting the new guy");
                conn.Disconnect();
                return;
            }

            if (SceneManager.GetActiveScene().path != menuScene)
            {
                Debug.Log(SceneManager.GetActiveScene().path + " != " + menuScene);
                conn.Disconnect();
                return;
            }
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            //base.OnServerAddPlayer(conn);
            Debug.Log("OnServerAddPlayer");
            if (SceneManager.GetActiveScene().path == menuScene)
            {
                bool isLeader = RoomPlayers.Count == 0;
                Debug.Log("IsLeader: " + isLeader);
                // FUCK IT JUST CAST 
                NetworkRoomPlayerLobby roomPlayerInstance = Instantiate(roomPlayerPrefab) as NetworkRoomPlayerLobby;
                Debug.Log(roomPlayerInstance.gameObject.name + " ping", roomPlayerInstance);
                roomPlayerInstance.IsLeader = isLeader;

                NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
                Debug.Log("OnServerAddPlayer success");
            }
            else
            {
                Debug.Log("OnServerAddPlayer failure");
            }
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);
            Debug.Log("OnServerDisconnect");
            if (conn.identity != null)
            {
                var player = conn.identity.GetComponent<NetworkRoomPlayerLobby>();

                RoomPlayers.Remove(player);

                NotifyPlayersOfReadyState();
            }

            base.OnServerDisconnect(conn);
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            OnServerStopped?.Invoke();

            RoomPlayers.Clear();
            GamePlayers.Clear();
        }

        public void NotifyPlayersOfReadyState()
        {
            Debug.Log("Notifying");
            foreach (var player in RoomPlayers)
            {
                player.HandleReadyToStart(IsReadyToStart());
            }
        }

        private bool IsReadyToStart() // TODO: Make this do role-checks too.
        {
            Debug.Log("IsReadyToStart: numplayers:minPlayers: " + numPlayers + " " + minPlayers);
            if (numPlayers < minPlayers) 
            {
                Debug.Log("IsReadyToStart - false, not enough players.");
                return false; 
            }

            foreach (var player in RoomPlayers)
            {
                if (!player.IsReady) 
                {
                    Debug.Log("IsReadyToStart - false, player not ready");

                    return false; 
                }
            }
            Debug.Log("IsReadyToStart - true");
            return true;
        }

        public void StartGame()
        {
            Debug.Log("StartGame");
            if (SceneManager.GetActiveScene().path == menuScene)
            {
                if (!IsReadyToStart()) { return; }

                mapHandler = new MapHandler(mapSet, numberOfRounds);

                ServerChangeScene(mapHandler.NextMap);
            }
        }

        public override void ServerChangeScene(string newSceneName)
        {
            Debug.Log("ServerChangeScene - name: " + newSceneName);
            // From menu to game

            if (SceneManager.GetActiveScene().path == menuScene)
            {
                for (int i = RoomPlayers.Count - 1; i >= 0; i--)
                {
                    var conn = RoomPlayers[i].connectionToClient;
                    //var gameplayerInstance = Instantiate(playerPrefab);
                    //gameplayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);

                    NetworkServer.Destroy(conn.identity.gameObject);

                    //NetworkServer.ReplacePlayerForConnection(conn, gameplayerInstance);
                }
            }

            BasedServerChangeScene(GameplayScene);
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            base.OnServerSceneChanged(sceneName);
        }

        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            base.OnServerReady(conn);

            OnServerReadied?.Invoke(conn);
        }
    }
}
