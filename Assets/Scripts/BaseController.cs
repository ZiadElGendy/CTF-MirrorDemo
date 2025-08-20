using Mirror;
using UnityEngine;

namespace CTF
{
    public class Base : NetworkBehaviour
    {
        [SyncVar]
        public int baseId;
        [SyncVar]
        public GamePlayer owner;
        [SyncVar]
        public bool hasFlag = true;

        [SerializeField] private Renderer flagRenderer;
        [SerializeField] private Renderer baseRenderer;
        public Color baseColor = Color.white;

        private void Start()
        {
            if (flagRenderer != null)
            {
                flagRenderer.material.color = baseColor;
            }
            if (baseRenderer != null)
            {
                baseRenderer.material.color = baseColor;
            }
        }


        public void SetOwner(GamePlayer newOwner)
        {
            if (newOwner == null)
            {
                Debug.LogWarning("Attempted to set owner to null.");
                return;
            }

            owner = newOwner;
            changeFlagColor(newOwner.PlayerColor);
            Debug.Log($"Base {baseId} is now owned by {newOwner.PlayerName}");
        }

        private void changeFlagColor(Color newColor)
        {
            if (flagRenderer != null)
            {
                flagRenderer.material.color = newColor;
            }
        }
    }
}