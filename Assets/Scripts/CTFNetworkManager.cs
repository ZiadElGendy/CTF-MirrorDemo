using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/
namespace CTF
{
    public class CTFNetworkManager : NetworkManager
    {
        public static new CTFNetworkManager singleton => (CTFNetworkManager)NetworkManager.singleton;
        private SortedSet<int> _activePlayers = new SortedSet<int>();
        private SortedSet<int> _inactivePlayers = new SortedSet<int>{0,1,2,3};

        #region Server System Callbacks

        /// <summary>
        /// Called on the server when a client adds a new player with ClientScene.AddPlayer.
        /// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            int gameId = ActivatePlayer();

            // Get spawn position from GameManager
            Transform startPos = CTFGameManager.Instance.GetPlayerSpawnPoint(gameId);

            GameObject player = startPos != null
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab);

            player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";

            // Set the player ID and name on the player object
            GamePlayer gamePlayer = player.GetComponent<GamePlayer>();
            gamePlayer.playerId = gameId;

            // Notify GameManager about new player
            CTFGameManager.Instance.OnPlayerConnected(gamePlayer);

            NetworkServer.AddPlayerForConnection(conn, player);
        }

        /// <summary>
        /// Called on the server when a client disconnects.
        /// <para>This is called on the Server when a Client disconnects from the Server. Use an override to decide what should happen when a disconnection is detected.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            if (conn.identity != null)
            {
                GamePlayer gamePlayer = conn.identity.GetComponent<GamePlayer>();
                if (gamePlayer != null)
                {
                    int gameId = gamePlayer.playerId;
                    CTFGameManager.Instance.OnPlayerDisconnected(gamePlayer);
                    DeactivatePlayer(gameId);
                }
            }

            base.OnServerDisconnect(conn);
        }

        #endregion

        #region User Methods
        private int ActivatePlayer()
        {
            if (_inactivePlayers.Count == 0)
            {
                return -1;
            }

            // Get and remove the first (lowest) inactive player
            int userId = _inactivePlayers.Min;
            _inactivePlayers.Remove(userId);
            _activePlayers.Add(userId);

            return userId;
        }

        private int DeactivatePlayer(int userId)
        {
            if (!_activePlayers.Remove(userId))
            {
                return -1;
            }

            _inactivePlayers.Add(userId);
            return userId;
        }
        #endregion
    }
}