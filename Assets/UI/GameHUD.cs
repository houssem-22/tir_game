using Cipher.Clues;
using Cipher.Gameplay.Player;
using Cipher.Gameplay.Weapons;
using UnityEngine;

namespace Cipher.UI
{
    public sealed class GameHUD : MonoBehaviour
    {
        MatchManager _match;
        PlayerHealth _health;
        WeaponController _weapons;

        public void Bind(MatchManager match, PlayerHealth health, WeaponController weapons)
        {
            _match = match;
            _health = health;
            _weapons = weapons;
        }

        void OnGUI()
        {
            if (_match == null) return;

            DrawTopBar();
            DrawVitals();
            DrawIntel();
            if (_match.Phase == MatchPhase.Victory)
            {
                DrawVictory();
            }
        }

        void DrawTopBar()
        {
            GUI.Box(new Rect(12f, 12f, 520f, 70f), GUIContent.none);
            int minutes = Mathf.FloorToInt(_match.TimeRemaining / 60f);
            int seconds = Mathf.FloorToInt(_match.TimeRemaining % 60f);
            GUI.Label(new Rect(24f, 18f, 200f, 22f), $"TEMPS  {minutes:00}:{seconds:00}");
            GUI.Label(new Rect(24f, 40f, 490f, 36f), $"OBJECTIF  {_match.ObjectiveText}");
        }

        void DrawVitals()
        {
            float hp = _health != null ? _health.CurrentHealth : 0f;
            float max = _health != null ? _health.MaxHealth : 100f;
            string ammo = "—";
            string weapon = "—";
            if (_weapons != null && _weapons.WeaponCount > 0)
            {
                weapon = _weapons.Current.displayName;
                ammo = _weapons.IsReloading
                    ? "RECHARGEMENT…"
                    : $"{_weapons.Magazine} / {_weapons.Reserve}";
            }

            GUI.Box(new Rect(12f, Screen.height - 90f, 280f, 78f), GUIContent.none);
            GUI.Label(new Rect(24f, Screen.height - 82f, 250f, 22f), $"HP  {hp:0}/{max:0}");
            GUI.Label(new Rect(24f, Screen.height - 60f, 250f, 22f), $"ARME  {weapon}");
            GUI.Label(new Rect(24f, Screen.height - 38f, 250f, 22f), $"MUN  {ammo}");
        }

        void DrawIntel()
        {
            float x = Screen.width - 360f;
            GUI.Box(new Rect(x, 12f, 348f, 140f), "INTEL CHECKLIST");
            for (int i = 0; i < 3; i++)
            {
                GUI.Label(new Rect(x + 12f, 40f + i * 28f, 324f, 26f), _match.GetIntelLine(i));
            }

            if (_match.Mission != null && _match.CluesFound >= 3)
            {
                GUI.Label(new Rect(x + 12f, 124f, 324f, 22f), $"FINAL  {_match.Mission.finalNodeId}");
            }
        }

        void DrawVictory()
        {
            var rect = new Rect(Screen.width * 0.5f - 220f, Screen.height * 0.5f - 60f, 440f, 120f);
            GUI.Box(rect, "CIPHER");
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(rect.x, rect.y + 36f, rect.width, 30f), "ACCESS GRANTED");
            GUI.Label(new Rect(rect.x, rect.y + 70f, rect.width, 30f), "Victoire par objectif — pas par éliminations");
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
        }
    }
}
