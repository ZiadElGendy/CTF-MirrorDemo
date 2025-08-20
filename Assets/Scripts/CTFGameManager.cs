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

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        [Server]
        public void AssignPlayerToBase(GamePlayer player, Base baseToAssign)
        {
            if (baseToAssign == null || player == null) return;

            if (baseToAssign.owner != null)
            {
                Debug.LogWarning($"Base {baseToAssign.baseId} is already owned by {baseToAssign.owner.PlayerName}");
                return;
            }

            baseToAssign.owner = player;
            Debug.Log($"{player.PlayerName} has captured base {baseToAssign.baseId}");
        }

        [Server]
        public Transform GetBase(int baseId)
        {
            try
            {
                return bases[baseId];
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Base with ID {baseId} not found. Exception: {e.Message}");
                return null;
            }
        }
    }
}