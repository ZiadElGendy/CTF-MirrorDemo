using System;
using System.Collections.Generic;
using Mirror;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace CTF
{
    public class CTFGameManager : NetworkBehaviour
    {
        public static CTFGameManager Instance { get; private set; }
        public List<Base> bases = new List<Base>();
        public TextMeshProUGUI scoreboardText;
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

        }


        #endregion

        #region Game Logic
        [Server]
        public void HandleFlagCapture(GamePlayer player, Base capturedBase)
        {
            if (!capturedBase.hasFlag) return;

            capturedBase.hasFlag = false;
            player.SetStolenBase(capturedBase);
            player.hasFlag = true;

        }

        [Server]
        public void HandleFlagDeposit(GamePlayer player, Base homeBase)
        {
            //Debug.Log("1");
            if (!player.hasFlag || homeBase.owner != player) return;
            //Debug.Log("2");
            player.hasFlag = false;
            //Debug.Log("3");
            player.stolenBase.hasFlag = true;
            //Debug.Log("4");
            player.SetStolenBase(null);

            AddScore(player, 100);
        }

        [Server]
        public void AddScore(GamePlayer player, int score)
        {
            Debug.Log($"AddScore called on server: {isServer}. Player: {player.playerName}, Score: {score}");

            if (!playerScores.ContainsKey(player))
            {
                playerScores[player] = 0;
            }
            playerScores[player] += score;
            Debug.Log($"Player {player.playerName} scored {score} points. Total: {playerScores[player]}");

            RpcUpdateScoreboard();
            Debug.Log("UpdateScoreboard RPC called");
        }

        //FIXME: Why does this not get called?

        [ClientRpc]
        public void RpcUpdateScoreboard()
        {
            Debug.Log($"UpdateScoreboard RPC received on client. IsClient: {isClient}");

            string scoreboard = "";
            foreach (var kvp in playerScores)
            {
                scoreboard += $"{kvp.Key.playerName} (ID: {kvp.Key.playerId}) Score: {kvp.Value}\n";
            }
            Debug.Log($"Scoreboard updated:\n{scoreboard}");

            if (scoreboardText != null)
            {
                scoreboardText.text = scoreboard;
            }
            else
            {
                Debug.LogError("scoreboardText is null!");
            }
        }

        #endregion


    }
}