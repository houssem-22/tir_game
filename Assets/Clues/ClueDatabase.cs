using System.Collections.Generic;
using UnityEngine;

namespace Cipher.Clues
{
    /// <summary>
    /// Map truth database for MVP1 Blacksite slice.
    /// Generator may only pick nodes that exist here — never invent objects.
    /// </summary>
    [CreateAssetMenu(menuName = "CIPHER/Clue Database", fileName = "ClueDatabase_Blacksite")]
    public sealed class ClueDatabase : ScriptableObject
    {
        public string mapId = "blacksite_mvp1";
        public List<MapNode> nodes = new List<MapNode>();

        static ClueDatabase _runtimeFallback;

        public MapNode Find(string id)
        {
            if (string.IsNullOrEmpty(id) || nodes == null) return null;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] != null && nodes[i].id == id) return nodes[i];
            }
            return null;
        }

        public List<MapNode> GetCluePoints()
        {
            var list = new List<MapNode>();
            if (nodes == null) return list;
            for (int i = 0; i < nodes.Count; i++)
            {
                var n = nodes[i];
                if (n != null && n.isCluePoint && n.interactable) list.Add(n);
            }
            return list;
        }

        public List<MapNode> GetFinalPoints()
        {
            var list = new List<MapNode>();
            if (nodes == null) return list;
            for (int i = 0; i < nodes.Count; i++)
            {
                var n = nodes[i];
                if (n != null && n.isFinalPoint && n.interactable) list.Add(n);
            }
            return list;
        }

        public bool Contains(string id) => Find(id) != null;

        public static ClueDatabase CreateBlacksiteRuntime()
        {
            if (_runtimeFallback != null) return _runtimeFallback;

            var db = CreateInstance<ClueDatabase>();
            db.mapId = "blacksite_mvp1";
            db.nodes = new List<MapNode>
            {
                Node("Zone_Hospital", MapNodeType.Zone, "Hospital Sector", "Zone_Hospital", null, null, false, false, new Vector3(-28f, 0f, 8f)),
                Node("Zone_Industrial", MapNodeType.Zone, "Industrial Yard", "Zone_Industrial", null, null, false, false, new Vector3(0f, 0f, 18f)),
                Node("Zone_Bunker", MapNodeType.Zone, "Bunker Approach", "Zone_Bunker", null, null, false, false, new Vector3(30f, 0f, -6f)),

                Node("Building_04", MapNodeType.Building, "Building 04", "Zone_Industrial", "Building_04", null, true, false, new Vector3(2f, 0f, 20f), related: "Terminal_C"),
                Node("Room_17", MapNodeType.Room, "Room 17", "Zone_Industrial", "Building_04", "Room_17", false, false, new Vector3(2f, 0f, 22f)),

                Node("Camera_12", MapNodeType.Camera, "Camera 12", "Zone_Hospital", "Hospital_Wing", "Corridor_A", true, false, new Vector3(-30f, 2.2f, 10f), faces: "East", related: "Building_04"),
                Node("Terminal_C", MapNodeType.Terminal, "Terminal C", "Zone_Industrial", "Building_04", "Room_17", true, false, new Vector3(3.5f, 1f, 22.5f), related: "Bunker_07"),

                Node("Bunker_07", MapNodeType.Bunker, "Bunker 07", "Zone_Bunker", "Bunker_07", "Entry", false, true, new Vector3(32f, 0f, -8f)),
                Node("Bunker_Terminal", MapNodeType.Terminal, "Bunker Code Pad", "Zone_Bunker", "Bunker_07", "Entry", false, true, new Vector3(32f, 1.1f, -6.5f), related: "Bunker_07"),

                // Extra truth nodes so seed variation can pick alternate chains later
                Node("Camera_03", MapNodeType.Camera, "Camera 03", "Zone_Hospital", "Hospital_Wing", "Lobby", true, false, new Vector3(-24f, 2.2f, 4f), faces: "South", related: "Building_04"),
                Node("Computer_A1", MapNodeType.Computer, "Computer A1", "Zone_Hospital", "Hospital_Wing", "Records", true, false, new Vector3(-32f, 1f, 6f), related: "Building_04"),
            };

            _runtimeFallback = db;
            return db;
        }

        static MapNode Node(
            string id,
            MapNodeType type,
            string displayName,
            string zone,
            string building,
            string room,
            bool cluePoint,
            bool finalPoint,
            Vector3 pos,
            string faces = null,
            string related = null)
        {
            return new MapNode
            {
                id = id,
                type = type,
                displayName = displayName,
                zoneId = zone,
                buildingId = building,
                roomId = room,
                facesDirection = faces,
                relatedTargetId = related,
                isCluePoint = cluePoint,
                isFinalPoint = finalPoint,
                interactable = true,
                worldPosition = pos
            };
        }
    }
}
