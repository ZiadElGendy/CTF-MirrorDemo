using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

namespace CTF
{
    public class Base : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnOwnerChanged))]
        public GamePlayer owner;

        [SyncVar(hook = nameof(OnHasFlagChanged))]
        public bool hasFlag = true;

        [SerializeField] private Renderer flagClothRenderer;
        [SerializeField] private GameObject flag;
        [SerializeField] private Renderer baseRenderer;

        [SyncVar]
        public Color baseColor = Color.white;

        private void Start()
        {
            UpdateBaseColors();
        }

        private void OnOwnerChanged(GamePlayer oldOwner, GamePlayer newOwner)
        {
            if (newOwner != null)
            {
                // Update color based on new owner's color
                if (newOwner.playerColor != baseColor)
                {
                    baseColor = newOwner.playerColor;
                    UpdateBaseColors();
                }
            }
            else
            {
                // Reset to default color if owner is null
                baseColor = Color.white;
            }
        }

        private void OnBaseColorChanged(Color oldColor, Color newColor)
        {
            UpdateBaseColors();
        }

        private void OnHasFlagChanged(bool oldValue, bool newValue)
        {
            flag.SetActive(newValue);
            if(newValue) flagClothRenderer.material.color = baseColor;
        }

        // Update visual colors on all clients
        private void UpdateBaseColors()
        {
            if (flagClothRenderer == null || baseRenderer == null)
            {
                Debug.LogError("Flag or Base Renderer is not assigned in the Base component.");
                return;
            }

            Debug.Log("Updating base colors");
            flagClothRenderer.material.color = baseColor;
            //change alpha so that base is transparent
            Color transparentColor = baseColor;
            transparentColor.a = 0.25f;
            baseRenderer.material.color = transparentColor;
        }

        [Server]
        public void SetOwner(GamePlayer newOwner)
        {
            if (newOwner == null)
            {
                Debug.LogWarning("Attempted to set owner to null.");
                owner = null;
                return;
            }

            owner = newOwner;
            Debug.Log($"Base {name} is now owned by {newOwner.playerName}");
        }
    }
}