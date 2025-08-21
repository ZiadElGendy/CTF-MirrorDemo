using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace CTF
{
    public class CTFGameManager : NetworkBehaviour
    {
        public static CTFGameManager Instance { get; private set; }
        public List<Base> bases = new List<Base>();
        private Dictionary<int, GamePlayer> activePlayers = new Dictionary<int, GamePlayer>();
        public Dictionary<GamePlayer, int> playerScores = new Dictionary<GamePlayer, int>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        #region Getters and Setters

        public Transform GetPlayerSpawnPoint(int playerId)
        {
            Base playerBase = GetBase(playerId);
            Debug.Log($"GetPlayerSpawnPoint: Player {playerId} base is {playerBase?.name}");
            return playerBase?.transform;
        }

        public Base GetBase(int playerId)
        {
            if (playerId >= 0 && playerId < bases.Count)
                return bases[playerId];
            return null;
        }

        #endregion

        #region Network Hooks

        [Server]
        public void OnPlayerConnected(GamePlayer player)
        {
            // Make sure player is not rotated to ensure movement works correctly
            player.transform.rotation = Quaternion.identity;
            // Assign random color
            player.playerColor =
                new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
            // Update game state
            Base assignedBase = GetBase(player.playerId);
            if (assignedBase != null)
            {
                assignedBase.owner = player;
                activePlayers[player.playerId] = player;
            }

            Debug.Log($"GameManager: Player {player.playerId} connected and assigned to base");
        }

        [Server]
        public void OnPlayerDisconnected(GamePlayer player)
        {
            // Update game state
            Base assignedBase = GetBase(player.playerId);
            if (assignedBase != null)
            {
                assignedBase.owner = null;
            }

            activePlayers.Remove(player.playerId);
            Debug.Log($"GameManager: Player {player.playerId} disconnected, base freed");


        }


        #endregion

        #region Game Logic
        [Server]
        public void HandleFlagCapture(GamePlayer player, Base capturedBase)
        {
            if (!capturedBase.hasFlag) return;

            capturedBase.hasFlag = false;
            player.hasFlag = true;
            player.stolenBase = capturedBase;
        }
        [Server]
        public void HandleFlagDeposit(GamePlayer player, Base homeBase)
        {
            if (!player.hasFlag || homeBase.owner != player) return;

            player.hasFlag = false;
            player.stolenBase.hasFlag = true;
            player.stolenBase = null;

            AddScore(player, 100);
        }

        [Server]
        public void AddScore(GamePlayer player, int score)
        {
            if (!playerScores.ContainsKey(player))
            {
                playerScores[player] = 0;
            }
            playerScores[player] += score;
            //print scoreboard
            foreach (var kvp in playerScores)
            {
                Debug.Log($"Player {kvp.Key.playerName} (ID: {kvp.Key.playerId}) Score: {kvp.Value}");
            }
        }

        #endregion
    }
}