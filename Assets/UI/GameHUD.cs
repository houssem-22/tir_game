using Cipher.Clues;
using Cipher.Gameplay.Player;
using Cipher.Gameplay.Weapons;
using UnityEngine;

namespace Cipher.UI
{
    public sealed class GameHUD : MonoBehaviour
    {
        static readonly Color Panel = new Color(0.04f, 0.06f, 0.07f, 0.82f);
        static readonly Color Amber = new Color(0.95f, 0.72f, 0.22f);
        static readonly Color Cyan = new Color(0.35f, 0.9f, 0.95f);
        static readonly Color Danger = new Color(0.95f, 0.28f, 0.22f);
        static readonly Color Ok = new Color(0.45f, 0.9f, 0.5f);

        MatchManager _match;
        PlayerHealth _health;
        WeaponController _weapons;
        PlayerController _player;
        GUIStyle _title;
        GUIStyle _body;
        GUIStyle _dim;
        GUIStyle _center;
        Texture2D _pixel;
        bool _stylesReady;

        public void Bind(MatchManager match, PlayerHealth health, WeaponController weapons)
        {
            _match = match;
            _health = health;
            _weapons = weapons;
            _player = health != null ? health.GetComponent<PlayerController>() : null;
        }

        void OnGUI()
        {
            if (_match == null) return;
            EnsureStyles();

            DrawTopBar();
            DrawVitals();
            DrawIntel();
            DrawCrosshair();
            DrawCompass();
            DrawInteractPrompt();

            if (_match.Phase == MatchPhase.Victory) DrawBanner("ACCESS GRANTED", "Victoire par objectif — pas par éliminations", Ok);
            else if (_match.Phase == MatchPhase.Defeat) DrawBanner("MISSION FAILED", "Temps écoulé", Danger);
            else if (_match.Phase == MatchPhase.FailedGeneration) DrawBanner("GENERATION FAIL", _match.StatusMessage, Danger);
        }

        void EnsureStyles()
        {
            if (_stylesReady) return;
            _pixel = Texture2D.whiteTexture;
            _title = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft
            };
            _body = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft
            };
            _dim = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.UpperLeft
            };
            _center = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _stylesReady = true;
        }

        void DrawTopBar()
        {
            Fill(new Rect(16f, 16f, 540f, 78f), Panel);
            Accent(new Rect(16f, 16f, 4f, 78f), Amber);

            int minutes = Mathf.FloorToInt(_match.TimeRemaining / 60f);
            int seconds = Mathf.FloorToInt(_match.TimeRemaining % 60f);
            bool low = _match.TimeRemaining <= 60f && _match.Phase == MatchPhase.Playing;
            _title.normal.textColor = low ? Danger : Amber;
            GUI.Label(new Rect(32f, 22f, 220f, 24f), $"T+ {minutes:00}:{seconds:00}", _title);

            _dim.normal.textColor = new Color(0.7f, 0.75f, 0.72f);
            GUI.Label(new Rect(210f, 24f, 330f, 20f), _match.StatusMessage, _dim);

            _body.normal.textColor = Color.white;
            GUI.Label(new Rect(32f, 46f, 508f, 40f), _match.ObjectiveText, _body);
        }

        void DrawVitals()
        {
            float hp = _health != null ? _health.CurrentHealth : 0f;
            float max = _health != null ? _health.MaxHealth : 100f;
            string ammo = "—";
            string weapon = "—";
            if (_weapons != null && _weapons.WeaponCount > 0 && _weapons.Current != null)
            {
                weapon = _weapons.Current.displayName;
                ammo = _weapons.IsReloading
                    ? "RECHARGEMENT…"
                    : $"{_weapons.Magazine} / {_weapons.Reserve}";
            }

            var box = new Rect(16f, Screen.height - 102f, 300f, 86f);
            Fill(box, Panel);
            Accent(new Rect(box.x, box.y, 4f, box.height), Danger);

            _dim.normal.textColor = new Color(0.75f, 0.78f, 0.8f);
            GUI.Label(new Rect(box.x + 16f, box.y + 8f, 260f, 18f), "VITAUX", _dim);

            float ratio = max > 0f ? Mathf.Clamp01(hp / max) : 0f;
            var bar = new Rect(box.x + 16f, box.y + 30f, 268f, 10f);
            Fill(bar, new Color(0.12f, 0.12f, 0.12f, 0.9f));
            Fill(new Rect(bar.x, bar.y, bar.width * ratio, bar.height), Color.Lerp(Danger, Ok, ratio));

            _body.normal.textColor = Color.white;
            GUI.Label(new Rect(box.x + 16f, box.y + 44f, 260f, 18f), $"{weapon}    {ammo}", _body);
            GUI.Label(new Rect(box.x + 16f, box.y + 64f, 260f, 18f), $"HP  {hp:0}", _dim);
        }

        void DrawIntel()
        {
            float x = Screen.width - 372f;
            var box = new Rect(x, 16f, 356f, 154f);
            Fill(box, Panel);
            Accent(new Rect(box.x, box.y, 4f, box.height), Cyan);

            _title.normal.textColor = Cyan;
            GUI.Label(new Rect(x + 16f, 22f, 320f, 22f), "INTEL CHECKLIST", _title);

            for (int i = 0; i < 3; i++)
            {
                bool found = _match.IsClueFound(i);
                bool current = i == _match.CluesFound && _match.Phase == MatchPhase.Playing;
                _body.normal.textColor = found ? Ok : (current ? Amber : new Color(0.55f, 0.58f, 0.55f));
                GUI.Label(new Rect(x + 16f, 50f + i * 28f, 324f, 26f), _match.GetIntelLine(i), _body);
            }

            if (_match.Mission != null && _match.CluesFound >= 3)
            {
                _dim.normal.textColor = Amber;
                GUI.Label(new Rect(x + 16f, 138f, 324f, 22f), $"FINAL  {_match.Mission.finalNodeId}", _dim);
            }
        }

        void DrawCrosshair()
        {
            if (_match.Phase != MatchPhase.Playing) return;
            float cx = Screen.width * 0.5f;
            float cy = Screen.height * 0.5f;
            Fill(new Rect(cx - 8f, cy - 1f, 16f, 2f), new Color(1f, 1f, 1f, 0.75f));
            Fill(new Rect(cx - 1f, cy - 8f, 2f, 16f), new Color(1f, 1f, 1f, 0.75f));
        }

        void DrawCompass()
        {
            if (_match.Phase != MatchPhase.Playing || _health == null) return;
            if (!_match.TryGetObjectivePosition(out Vector3 target)) return;

            Vector3 from = _health.transform.position;
            Vector3 dir = target - from;
            dir.y = 0f;
            float dist = dir.magnitude;
            if (dist < 0.2f) return;

            float relative = Mathf.DeltaAngle(_health.transform.eulerAngles.y, Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg);
            float cx = Screen.width * 0.5f + relative * 1.6f;
            cx = Mathf.Clamp(cx, Screen.width * 0.5f - 140f, Screen.width * 0.5f + 140f);
            Fill(new Rect(cx - 6f, 102f, 12f, 12f), Amber);
            _dim.normal.textColor = Amber;
            GUI.Label(new Rect(Screen.width * 0.5f - 90f, 116f, 180f, 20f), $"{dist:0} m  → objectif", _dim);
        }

        void DrawInteractPrompt()
        {
            if (_player == null) return;
            string prompt = _player.InteractPrompt;
            if (string.IsNullOrEmpty(prompt)) return;

            var rect = new Rect(Screen.width * 0.5f - 180f, Screen.height * 0.62f, 360f, 32f);
            Fill(rect, new Color(0.02f, 0.04f, 0.05f, 0.7f));
            _center.fontSize = 14;
            _center.normal.textColor = Amber;
            GUI.Label(rect, prompt, _center);
            _center.fontSize = 22;
        }

        void DrawBanner(string title, string subtitle, Color color)
        {
            var rect = new Rect(Screen.width * 0.5f - 240f, Screen.height * 0.5f - 70f, 480f, 140f);
            Fill(rect, new Color(0.02f, 0.03f, 0.04f, 0.92f));
            Accent(new Rect(rect.x, rect.y, rect.width, 4f), color);
            _center.normal.textColor = color;
            GUI.Label(new Rect(rect.x, rect.y + 28f, rect.width, 40f), title, _center);
            _body.normal.textColor = Color.white;
            _body.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(rect.x + 20f, rect.y + 78f, rect.width - 40f, 36f), subtitle, _body);
            _body.alignment = TextAnchor.UpperLeft;
        }

        void Fill(Rect rect, Color color)
        {
            var prev = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, _pixel);
            GUI.color = prev;
        }

        void Accent(Rect rect, Color color) => Fill(rect, color);
    }
}
