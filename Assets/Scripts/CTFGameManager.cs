using System;
using System.Collections.Generic;
using Mirror;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace CTF
{
    public class CTFGameManager : NetworkedSingleton<CTFGameManager>
    {
        public List<Base> bases = new List<Base>();
        public TextMeshProUGUI scoreboardText;
        private Dictionary<int, GamePlayer> _activePlayers = new Dictionary<int, GamePlayer>();
        public Dictionary<GamePlayer, int> PlayerScores = new Dictionary<GamePlayer, int>();


        #region Getters and Setters

        public Transform GetPlayerSpawnPoint(int playerId)
        {
            Base playerBase = GetBase(playerId);
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
                _activePlayers[player.playerId] = player;
            }

            // Initialize player score and update scoreboard
            PlayerScores.TryAdd(player, 0);
            Debug.Log($"Players in dictionary: {PlayerScores.Count}");
            Debug.Log($"Contains this player: {PlayerScores.ContainsKey(player)}");
            Invoke(nameof(ServerUpdateScoreboard), 1f); // Delay to ensure client is ready

        }

        [Server]
        public void OnPlayerDisconnected(GamePlayer player)
        {
            // Update game state
            Base assignedBase = GetBase(player.playerId);
            if (assignedBase != null)
            {
                assignedBase.ResetBase();
            }
            Base stolenBase = player.stolenBase;
            if (stolenBase != null)
            {
                stolenBase.ResetBase();
            }

            _activePlayers.Remove(player.playerId);

            // Remove player from scores and update scoreboard
            if (PlayerScores.ContainsKey(player))
            {
                PlayerScores.Remove(player);
                ServerUpdateScoreboard();
            }

            //  Reset stolen base if player had one
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
            if (!player.hasFlag || homeBase.owner != player) return;
            player.hasFlag = false;
            player.stolenBase.hasFlag = true;
            player.SetStolenBase(null);

            AddScore(player, 100);
        }

        [Server]
        public void AddScore(GamePlayer player, int score)
        {
            PlayerScores.TryAdd(player, 0);
            PlayerScores[player] += score;

            ServerUpdateScoreboard();
        }

        [Server]
        public void ServerUpdateScoreboard()
        {
            string scoreboard = "";
            foreach (var kvp in PlayerScores)
            {
                Debug.Log($"{kvp.Key.playerName}: {kvp.Value}");
                scoreboard += $"{kvp.Key.playerName}: {kvp.Value}\n";
            }
            Debug.Log(scoreboard);
            RpcUpdateScoreboard(scoreboard);
        }
        [ClientRpc]
        public void RpcUpdateScoreboard(string scoreboardTextString)
        {
            Debug.Log($"UpdateScoreboard RPC received on client. IsClient: {isClient}");
            scoreboardText.text = scoreboardTextString;
        }

        #endregion


    }
}