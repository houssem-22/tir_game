using System;
using UnityEngine;

namespace Cipher.Clues
{
    public enum MapNodeType
    {
        Zone,
        Building,
        Room,
        Camera,
        Computer,
        Terminal,
        Bunker,
        Prop
    }

    [Serializable]
    public sealed class MapNode
    {
        public string id;
        public MapNodeType type;
        public string displayName;
        public string zoneId;
        public string buildingId;
        public string roomId;
        public string facesDirection;
        public string relatedTargetId;
        public bool isCluePoint;
        public bool isFinalPoint;
        public bool interactable = true;
        public Vector3 worldPosition;
    }

    [Serializable]
    public sealed class ClueStep
    {
        public int index;
        public string targetNodeId;
        public string text;
        public string nextHint;
    }

    [Serializable]
    public sealed class ClueMission
    {
        public int seed;
        public string finalNodeId;
        public string code;
        public ClueStep[] steps = Array.Empty<ClueStep>();
        public bool validated;
    }
}
