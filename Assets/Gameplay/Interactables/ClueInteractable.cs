using Cipher.Clues;
using UnityEngine;

namespace Cipher.Gameplay.Interactables
{
    public sealed class ClueInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] string nodeId;
        [SerializeField] string label = "Objet indice";

        MatchManager _match;
        bool _collected;
        TextMesh _labelMesh;

        public string NodeId => nodeId;
        public string Prompt => _collected ? $"{label} (déjà scanné)" : $"Scanner {label} [E]";
        public bool CanInteract => !_collected;

        public void Setup(string id, string displayName, MatchManager match)
        {
            nodeId = id;
            label = displayName;
            _match = match;
            EnsureLabel();
        }

        void EnsureLabel()
        {
            if (_labelMesh != null) return;
            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(transform, false);
            labelGo.transform.localScale = new Vector3(
                1f / Mathf.Max(0.01f, transform.localScale.x),
                1f / Mathf.Max(0.01f, transform.localScale.y),
                1f / Mathf.Max(0.01f, transform.localScale.z));
            labelGo.transform.localPosition = new Vector3(0f, 0.65f + 0.35f / Mathf.Max(0.01f, transform.localScale.y), 0f);
            _labelMesh = labelGo.AddComponent<TextMesh>();
            _labelMesh.text = label;
            _labelMesh.characterSize = 0.06f;
            _labelMesh.fontSize = 42;
            _labelMesh.anchor = TextAnchor.MiddleCenter;
            _labelMesh.alignment = TextAlignment.Center;
            _labelMesh.color = new Color(0.95f, 0.85f, 0.4f);
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        void LateUpdate()
        {
            if (_labelMesh == null || Camera.main == null) return;
            _labelMesh.transform.rotation = Quaternion.LookRotation(
                _labelMesh.transform.position - Camera.main.transform.position);
        }

        public void Interact(GameObject actor)
        {
            if (_match == null) _match = FindFirstObjectByType<MatchManager>();
            if (_match == null || _collected) return;

            if (_match.TryCollectClue(nodeId))
            {
                _collected = true;
                var rend = GetComponentInChildren<Renderer>();
                if (rend != null) rend.material.color = new Color(0.35f, 0.7f, 0.4f);
                if (_labelMesh != null) _labelMesh.color = new Color(0.5f, 0.9f, 0.55f);
            }
        }
    }
}
