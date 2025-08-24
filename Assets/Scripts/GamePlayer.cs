using System;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

namespace CTF
{


    public class GamePlayer : NetworkBehaviour
    {
        [SyncVar] public string playerName = "Player";
        [SyncVar] public int playerId = 0;
        [SyncVar] public bool hasFlag = false;
        [SyncVar (hook =nameof (OnStolenBaseChanged))] public Base stolenBase = null;
        [SerializeField] private Renderer playerRenderer;
        [SerializeField] private Renderer flagIndicatorRenderer;
        [SyncVar] public Color playerColor = Color.red;

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            CmdSetPlayerName(PlayerPrefs.GetString("PlayerName"));
        }
        [Command]
        private void CmdSetPlayerName(string name)
        {
            playerName = name;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            playerRenderer.material.color = playerColor;
        }

        [Server]
        public void SetStolenBase(Base baseObject)
        {
            stolenBase = baseObject;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Base"))
            {
                Base baseObject = other.GetComponent<Base>();

                if (baseObject.owner == this && hasFlag)
                {
                    CmdRequestDepositFlag(baseObject);
                }
                else if (baseObject.owner != this && !hasFlag && baseObject.hasFlag)
                {
                    CmdRequestCaptureFlag(baseObject);
                }

            }
        }

        [Command]
        private void CmdRequestCaptureFlag(Base capturedBase)
        {
            CTFGameManager.Instance.HandleFlagCapture(this, capturedBase);
        }

        [Command]
        private void CmdRequestDepositFlag(Base homeBase)
        {
            // Just notify GameManager
            CTFGameManager.Instance.HandleFlagDeposit(this, homeBase);
        }
        
        private void OnStolenBaseChanged(Base oldBase, Base newBase)
        {
            if (newBase == null)
            {
                flagIndicatorRenderer.enabled = false;
            }
            else
            {
                flagIndicatorRenderer.enabled = true;
                flagIndicatorRenderer.material.color = stolenBase.baseColor;
            }
        }
    }
}
