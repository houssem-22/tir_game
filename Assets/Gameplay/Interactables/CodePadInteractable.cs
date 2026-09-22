using Cipher.Clues;
using Cipher.Gameplay;
using UnityEngine;

namespace Cipher.Gameplay.Interactables
{
    public sealed class CodePadInteractable : MonoBehaviour, IInteractable
    {
        MatchManager _match;
        bool _open;
        string _input = string.Empty;
        bool _unlocked;
        GUIStyle _keyStyle;
        GUIStyle _titleStyle;

        public string Prompt => _unlocked ? "Porte ouverte" : "Saisir le code [E]";
        public bool CanInteract => !_unlocked;

        public void Setup(MatchManager match)
        {
            _match = match;
        }

        public void Interact(GameObject actor)
        {
            if (_unlocked) return;
            if (_match == null) _match = FindFirstObjectByType<MatchManager>();
            if (!_open)
            {
                _open = true;
                _input = string.Empty;
                GameplayUi.PushModal();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        void Update()
        {
            if (!_open) return;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClosePad();
                return;
            }

            for (int i = 0; i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
                {
                    Append(i.ToString());
                }
            }

            if (Input.GetKeyDown(KeyCode.Backspace) && _input.Length > 0)
            {
                _input = _input.Substring(0, _input.Length - 1);
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                Submit();
            }
        }

        void OnGUI()
        {
            if (!_open) return;
            EnsureStyles();

            float w = 340f;
            float h = 420f;
            var rect = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            GUI.Box(rect, GUIContent.none);
            GUI.Label(new Rect(rect.x, rect.y + 12f, w, 28f), "BUNKER ACCESS", _titleStyle);

            string display = _input.PadRight(4, '_');
            GUI.Box(new Rect(rect.x + 40f, rect.y + 48f, w - 80f, 44f), display);

            float kx = rect.x + 40f;
            float ky = rect.y + 110f;
            float kw = 72f;
            float kh = 48f;
            float gap = 12f;
            int n = 1;
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    if (GUI.Button(new Rect(kx + col * (kw + gap), ky + row * (kh + gap), kw, kh), n.ToString(), _keyStyle))
                    {
                        Append(n.ToString());
                    }
                    n++;
                }
            }

            if (GUI.Button(new Rect(kx, ky + 3 * (kh + gap), kw, kh), "CLR", _keyStyle))
            {
                _input = string.Empty;
            }

            if (GUI.Button(new Rect(kx + (kw + gap), ky + 3 * (kh + gap), kw, kh), "0", _keyStyle))
            {
                Append("0");
            }

            if (GUI.Button(new Rect(kx + 2 * (kw + gap), ky + 3 * (kh + gap), kw, kh), "OK", _keyStyle))
            {
                Submit();
            }

            if (_match != null)
            {
                GUI.Label(new Rect(rect.x + 24f, rect.y + h - 36f, w - 48f, 24f), _match.StatusMessage);
            }
        }

        void Append(string digit)
        {
            if (_input.Length >= 4) return;
            _input += digit;
        }

        void Submit()
        {
            if (_match == null) return;
            if (_match.TrySubmitCode(_input))
            {
                _unlocked = true;
                ClosePad();
                var rend = GetComponentInChildren<Renderer>();
                if (rend != null) rend.material.color = new Color(0.2f, 0.75f, 0.35f);
            }
            else
            {
                _input = string.Empty;
            }
        }

        void ClosePad()
        {
            if (!_open) return;
            _open = false;
            GameplayUi.PopModal();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void OnDestroy()
        {
            if (_open) GameplayUi.PopModal();
        }

        void EnsureStyles()
        {
            if (_keyStyle != null) return;
            _keyStyle = new GUIStyle(GUI.skin.button) { fontSize = 18, fontStyle = FontStyle.Bold };
            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }
    }
}
