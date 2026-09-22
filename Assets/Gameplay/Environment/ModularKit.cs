using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cipher.Gameplay.Environment
{
    /// <summary>
    /// 4 m modular kit: floors, wall panels, door bays, columns. Shared edges are placed once.
    /// Floors/ceilings/trim are visual-only so the player walks on the world ground plane.
    /// </summary>
    public static class ModularKit
    {
        public const float Tile = 4f;
        public const float Height = 3.2f;
        public const float WallThickness = 0.22f;
        public const float DoorWidth = 2.4f;
        public const float DoorHeight = 2.45f;

        [Flags]
        public enum Opening
        {
            None = 0,
            North = 1,
            South = 2,
            East = 4,
            West = 8
        }

        static Transform _root;
        static readonly HashSet<string> OccupiedFloors = new HashSet<string>();
        static readonly HashSet<string> OccupiedEdges = new HashSet<string>();
        static readonly HashSet<string> OccupiedCorners = new HashSet<string>();

        public static void Begin()
        {
            OccupiedFloors.Clear();
            OccupiedEdges.Clear();
            OccupiedCorners.Clear();
            _root = new GameObject("ModularLevel").transform;
        }

        public static Vector3 TileCenter(int gx, int gz)
        {
            return new Vector3((gx + 0.5f) * Tile, 0f, (gz + 0.5f) * Tile);
        }

        public static void Fill(
            string name,
            int gx,
            int gz,
            int tilesX,
            int tilesZ,
            Material wall,
            Material floor,
            Material ceiling,
            Opening doors,
            Color lightColor)
        {
            var wing = new GameObject(name).transform;
            wing.SetParent(_root, false);

            for (int x = 0; x < tilesX; x++)
            {
                for (int z = 0; z < tilesZ; z++)
                {
                    PlaceFloor(wing, gx + x, gz + z, floor);
                    PlaceCeiling(wing, gx + x, gz + z, ceiling);
                }
            }

            for (int x = 0; x < tilesX; x++)
            {
                PlaceEdge(wing, gx + x, gz, vertical: false, wall, Has(doors, Opening.South) && IsDoorTile(x, tilesX));
                PlaceEdge(wing, gx + x, gz + tilesZ, vertical: false, wall, Has(doors, Opening.North) && IsDoorTile(x, tilesX));
            }

            for (int z = 0; z < tilesZ; z++)
            {
                PlaceEdge(wing, gx, gz + z, vertical: true, wall, Has(doors, Opening.West) && IsDoorTile(z, tilesZ));
                PlaceEdge(wing, gx + tilesX, gz + z, vertical: true, wall, Has(doors, Opening.East) && IsDoorTile(z, tilesZ));
            }

            for (int x = 0; x <= tilesX; x++)
            {
                for (int z = 0; z <= tilesZ; z++)
                {
                    PlaceColumn(wing, gx + x, gz + z);
                }
            }

            var lightGo = new GameObject(name + "_Light");
            lightGo.transform.SetParent(wing, false);
            lightGo.transform.position = new Vector3(
                (gx + tilesX * 0.5f) * Tile,
                Height * 0.72f,
                (gz + tilesZ * 0.5f) * Tile);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = Mathf.Max(tilesX, tilesZ) * Tile * 0.85f + 6f;
            light.intensity = 2.4f;
            light.color = lightColor;
            light.shadows = LightShadows.None;
        }

        public static GameObject CreateProp(string name, Vector3 worldPos, Vector3 scale, Material mat, bool collider = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = worldPos;
            go.transform.localScale = scale;
            Apply(go, mat);
            if (!collider)
            {
                UnityEngine.Object.Destroy(go.GetComponent<Collider>());
            }
            return go;
        }

        public static GameObject CreateCylinderProp(string name, Vector3 worldPos, Vector3 scale, Material mat, bool collider = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.position = worldPos;
            go.transform.localScale = scale;
            Apply(go, mat);
            if (!collider)
            {
                UnityEngine.Object.Destroy(go.GetComponent<Collider>());
            }
            return go;
        }

        static void PlaceFloor(Transform parent, int gx, int gz, Material mat)
        {
            string key = gx + ":" + gz;
            if (!OccupiedFloors.Add(key)) return;
            var pos = TileCenter(gx, gz) + new Vector3(0f, 0.02f, 0f);
            var go = Part(parent, "Floor_" + key, pos, new Vector3(Tile, 0.04f, Tile), mat, collider: false);
            go.isStatic = true;
        }

        static void PlaceCeiling(Transform parent, int gx, int gz, Material mat)
        {
            var pos = TileCenter(gx, gz) + new Vector3(0f, Height, 0f);
            Part(parent, "Ceil_" + gx + "_" + gz, pos, new Vector3(Tile, 0.08f, Tile), mat, collider: false);
        }

        static void PlaceColumn(Transform parent, int gx, int gz)
        {
            string key = gx + ":" + gz;
            if (!OccupiedCorners.Add(key)) return;
            var pos = new Vector3(gx * Tile, Height * 0.5f, gz * Tile);
            Part(parent, "Col_" + key, pos, new Vector3(0.28f, Height, 0.28f), RuntimeMaterials.Concrete, collider: true);
        }

        static void PlaceEdge(Transform parent, int ax, int az, bool vertical, Material wall, bool door)
        {
            string key = (vertical ? "V:" : "H:") + ax + ":" + az;
            if (!OccupiedEdges.Add(key)) return;

            Vector3 center;
            Vector3 solidScale;
            Vector3 wainscotScale;
            if (vertical)
            {
                center = new Vector3(ax * Tile, Height * 0.5f, (az + 0.5f) * Tile);
                solidScale = new Vector3(WallThickness, Height, Tile);
                wainscotScale = new Vector3(WallThickness + 0.04f, 1.05f, Tile);
            }
            else
            {
                center = new Vector3((ax + 0.5f) * Tile, Height * 0.5f, az * Tile);
                solidScale = new Vector3(Tile, Height, WallThickness);
                wainscotScale = new Vector3(Tile, 1.05f, WallThickness + 0.04f);
            }

            if (!door)
            {
                Part(parent, "Wall_" + key, center, solidScale, wall, collider: true);
                Part(parent, "Wainscot_" + key, new Vector3(center.x, 0.52f, center.z), wainscotScale, RuntimeMaterials.Metal, collider: false);
                return;
            }

            PlaceDoorBay(parent, key, center, vertical, wall);
        }

        static void PlaceDoorBay(Transform parent, string key, Vector3 center, bool vertical, Material wall)
        {
            float side = (Tile - DoorWidth) * 0.5f;
            float lintelH = Height - DoorHeight;
            if (vertical)
            {
                Part(parent, "DoorZ_" + key + "_A",
                    new Vector3(center.x, Height * 0.5f, center.z - (DoorWidth * 0.5f + side * 0.5f)),
                    new Vector3(WallThickness, Height, side), wall, true);
                Part(parent, "DoorZ_" + key + "_B",
                    new Vector3(center.x, Height * 0.5f, center.z + (DoorWidth * 0.5f + side * 0.5f)),
                    new Vector3(WallThickness, Height, side), wall, true);
                if (lintelH > 0.05f)
                {
                    Part(parent, "DoorZ_" + key + "_Lintel",
                        new Vector3(center.x, DoorHeight + lintelH * 0.5f, center.z),
                        new Vector3(WallThickness, lintelH, DoorWidth), wall, true);
                }
            }
            else
            {
                Part(parent, "DoorX_" + key + "_A",
                    new Vector3(center.x - (DoorWidth * 0.5f + side * 0.5f), Height * 0.5f, center.z),
                    new Vector3(side, Height, WallThickness), wall, true);
                Part(parent, "DoorX_" + key + "_B",
                    new Vector3(center.x + (DoorWidth * 0.5f + side * 0.5f), Height * 0.5f, center.z),
                    new Vector3(side, Height, WallThickness), wall, true);
                if (lintelH > 0.05f)
                {
                    Part(parent, "DoorX_" + key + "_Lintel",
                        new Vector3(center.x, DoorHeight + lintelH * 0.5f, center.z),
                        new Vector3(DoorWidth, lintelH, WallThickness), wall, true);
                }
            }

            var frame = RuntimeMaterials.Metal;
            float jam = 0.1f;
            if (vertical)
            {
                Part(parent, "Frame_" + key + "_A", new Vector3(center.x, DoorHeight * 0.5f, center.z - DoorWidth * 0.5f),
                    new Vector3(WallThickness + 0.05f, DoorHeight, jam), frame, false);
                Part(parent, "Frame_" + key + "_B", new Vector3(center.x, DoorHeight * 0.5f, center.z + DoorWidth * 0.5f),
                    new Vector3(WallThickness + 0.05f, DoorHeight, jam), frame, false);
            }
            else
            {
                Part(parent, "Frame_" + key + "_A", new Vector3(center.x - DoorWidth * 0.5f, DoorHeight * 0.5f, center.z),
                    new Vector3(jam, DoorHeight, WallThickness + 0.05f), frame, false);
                Part(parent, "Frame_" + key + "_B", new Vector3(center.x + DoorWidth * 0.5f, DoorHeight * 0.5f, center.z),
                    new Vector3(jam, DoorHeight, WallThickness + 0.05f), frame, false);
            }

            Part(parent, "Threshold_" + key, new Vector3(center.x, 0.03f, center.z),
                vertical ? new Vector3(0.9f, 0.06f, DoorWidth) : new Vector3(DoorWidth, 0.06f, 0.9f),
                RuntimeMaterials.AccentWarn, false);
        }

        static GameObject Part(Transform parent, string name, Vector3 worldPos, Vector3 scale, Material mat, bool collider)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, true);
            go.transform.position = worldPos;
            go.transform.localScale = scale;
            Apply(go, mat);
            if (!collider)
            {
                UnityEngine.Object.Destroy(go.GetComponent<Collider>());
            }
            return go;
        }

        static void Apply(GameObject go, Material mat)
        {
            var rend = go.GetComponent<Renderer>();
            if (rend != null && mat != null) rend.sharedMaterial = mat;
        }

        static bool IsDoorTile(int index, int count)
        {
            if (count <= 1) return index == 0;
            if ((count & 1) == 1) return index == count / 2;
            return index == count / 2 || index == count / 2 - 1;
        }

        static bool Has(Opening mask, Opening flag) => (mask & flag) == flag;
    }
}
