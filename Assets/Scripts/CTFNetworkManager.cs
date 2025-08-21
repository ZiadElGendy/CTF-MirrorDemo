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
        public CTFGameManager gameManager;
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
            Debug.Log($"Adding player {gameId} for connection {conn.connectionId}");

            // Get spawn position from GameManager
            Transform startPos = CTFGameManager.Instance.GetPlayerSpawnPoint(gameId);

            GameObject player = startPos != null
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab);

            player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";

            // Set the playerId on the player object
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

        #region Client System Callbacks

        /// <summary>
        /// Called on the client when connected to a server.
        /// <para>The default implementation of this function sets the client as ready and adds a player. Override the function to dictate what happens when the client connects.</para>
        /// </summary>
        public override void OnClientConnect()
        {
            base.OnClientConnect();
        }

        /// <summary>
        /// Called on clients when disconnected from a server.
        /// <para>This is called on the client when it disconnects from the server. Override this function to decide what happens when the client disconnects.</para>
        /// </summary>
        public override void OnClientDisconnect()
        {
        }

        /// <summary>
        /// Called on clients when a servers tells the client it is no longer ready.
        /// <para>This is commonly used when switching scenes.</para>
        /// </summary>
        public override void OnClientNotReady()
        {
        }

        /// <summary>
        /// Called on client when transport raises an error.</summary>
        /// </summary>
        /// <param name="transportError">TransportError enum.</param>
        /// <param name="message">String message of the error.</param>
        public override void OnClientError(TransportError transportError, string message)
        {
        }

        /// <summary>
        /// Called on client when transport raises an exception.</summary>
        /// </summary>
        /// <param name="exception">Exception thrown from the Transport.</param>
        public override void OnClientTransportException(Exception exception)
        {
        }

        #endregion
        
        #region User Methods
        private int ActivatePlayer()
        {
            if (_inactivePlayers.Count == 0)
            {
                Debug.LogWarning("No inactive players available to activate.");
                return -1;
            }

            // Get and remove the first (lowest) inactive player
            int userId = _inactivePlayers.Min;
            _inactivePlayers.Remove(userId);
            _activePlayers.Add(userId);

            Debug.Log($"Activated player {userId}. Active players: {string.Join(", ", _activePlayers)}");
            return userId;
        }

        private int DeactivatePlayer(int userId)
        {
            if (!_activePlayers.Remove(userId))
            {
                Debug.LogWarning($"Player {userId} is not active.");
                return -1;
            }

            _inactivePlayers.Add(userId);
            Debug.Log($"Deactivated player {userId}. Active players: {string.Join(", ", _activePlayers)}");
            return userId;
        }
        #endregion
    }
}