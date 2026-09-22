using Cipher.Clues;
using UnityEngine;

namespace Cipher.Gameplay.Interactables
{
    public sealed class CodePadInteractable : MonoBehaviour, IInteractable
    {
        MatchManager _match;
        bool _open;
        string _input = string.Empty;
        bool _unlocked;

        public string Prompt => _unlocked ? "Porte ouverte" : "ENTER CODE [E]";
        public bool CanInteract => !_unlocked;

        public void Setup(MatchManager match)
        {
            _match = match;
        }

        public void Interact(GameObject actor)
        {
            if (_unlocked) return;
            if (_match == null) _match = FindFirstObjectByType<MatchManager>();
            _open = true;
            _input = string.Empty;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void Update()
        {
            if (!_open) return;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClosePad();
            }
        }

        void OnGUI()
        {
            if (!_open) return;

            float w = 360f;
            float h = 220f;
            var rect = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            GUI.Box(rect, "BUNKER ACCESS — ENTER CODE");

            var codeRect = new Rect(rect.x + 40f, rect.y + 50f, w - 80f, 36f);
            GUI.Label(new Rect(codeRect.x, codeRect.y - 22f, codeRect.width, 20f), "CODE _ _ _ _");
            GUI.skin.textField.fontSize = 28;
            GUI.skin.textField.alignment = TextAnchor.MiddleCenter;
            _input = GUI.TextField(codeRect, _input, 4);
            _input = Sanitize(_input);

            if (GUI.Button(new Rect(rect.x + 40f, rect.y + 110f, 120f, 36f), "SUBMIT"))
            {
                Submit();
            }

            if (GUI.Button(new Rect(rect.x + 200f, rect.y + 110f, 120f, 36f), "CANCEL"))
            {
                ClosePad();
            }

            if (_match != null)
            {
                GUI.Label(new Rect(rect.x + 40f, rect.y + 160f, w - 80f, 40f), _match.StatusMessage);
            }
        }

        void Submit()
        {
            if (_match == null) return;
            if (_match.TrySubmitCode(_input))
            {
                _unlocked = true;
                _open = false;
                var rend = GetComponentInChildren<Renderer>();
                if (rend != null) rend.material.color = new Color(0.2f, 0.75f, 0.35f);
                RestoreCursor();
            }
        }

        void ClosePad()
        {
            _open = false;
            RestoreCursor();
        }

        void RestoreCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        static string Sanitize(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            var chars = new char[Mathf.Min(4, value.Length)];
            int n = 0;
            for (int i = 0; i < value.Length && n < 4; i++)
            {
                if (char.IsDigit(value[i])) chars[n++] = value[i];
            }
            return new string(chars, 0, n);
        }
    }
}
