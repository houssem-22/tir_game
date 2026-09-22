using System.Collections.Generic;

namespace Cipher.Clues
{
    /// <summary>
    /// Controlled templates only. Placeholders are filled from map truth nodes.
    /// </summary>
    public static class ClueTemplates
    {
        public const string SearchObjectInZone = "Recherchez {OBJECT} dans {ZONE}.";
        public const string CameraPointsToBuilding = "{OBJECT} oriente vers {DIRECTION}. Destination : {TARGET}.";
        public const string BuildingLeadsToTerminal = "{OBJECT} — accédez à {ROOM}, cible {TARGET}.";
        public const string TerminalRevealsFinal = "{OBJECT} confirme la localisation finale : {FINAL}.";
        public const string FinalCodePrompt = "Point final {FINAL}. Saisissez le code d'accès.";

        public static string Format(string template, Dictionary<string, string> values)
        {
            if (string.IsNullOrEmpty(template)) return string.Empty;
            string result = template;
            if (values == null) return result;
            foreach (var kv in values)
            {
                result = result.Replace("{" + kv.Key + "}", kv.Value ?? string.Empty);
            }
            return result;
        }

        public static string ZoneDisplay(MapNode node)
        {
            if (node == null) return "ZONE INCONNUE";
            return string.IsNullOrEmpty(node.zoneId) ? node.displayName : node.zoneId.Replace('_', ' ');
        }
    }
}
