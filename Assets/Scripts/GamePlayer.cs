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
        [SyncVar (hook = nameof(OnHasFlagChanged))] public bool hasFlag = false;
        [SyncVar] public Base stolenBase = null;
        [SerializeField] private Renderer playerRenderer;
        [SerializeField] private Renderer flagIndicatorRenderer;

        public Color playerColor = Color.red;

        public override void OnStartClient()
        {
            base.OnStartClient();
            playerRenderer.material.color = playerColor;
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

        private void OnHasFlagChanged(bool oldValue, bool newValue)
        {
            flagIndicatorRenderer.enabled = newValue;
            flagIndicatorRenderer.material.color = newValue ? stolenBase.baseColor : Color.clear;
        }
    }
}
