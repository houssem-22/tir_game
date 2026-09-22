using UnityEngine;

namespace Cipher.Gameplay.Environment
{
    /// <summary>
    /// Builds hollow, walkable modular rooms (walls + floor + ceiling + door gap).
    /// </summary>
    public static class ModularBuilder
    {
        const float WallThickness = 0.35f;
        const float DoorWidth = 1.9f;
        const float DoorHeight = 2.35f;

        public static GameObject BuildRoom(
            string name,
            Vector3 center,
            Vector3 size,
            Material wallMat,
            Material floorMat,
            Material ceilingMat,
            bool doorNorth = false,
            bool doorSouth = false,
            bool doorEast = false,
            bool doorWest = false,
            bool withCeiling = true,
            Color? lightColor = null)
        {
            var root = new GameObject(name);
            root.transform.position = center;

            float w = size.x;
            float h = size.y;
            float d = size.z;

            CreatePart(root.transform, "Floor", new Vector3(0f, 0.05f, 0f), new Vector3(w, 0.1f, d), floorMat);
            if (withCeiling)
            {
                CreatePart(root.transform, "Ceiling", new Vector3(0f, h, 0f), new Vector3(w, 0.12f, d), ceilingMat);
            }

            BuildWallX(root.transform, "Wall_S", new Vector3(0f, h * 0.5f, -d * 0.5f + WallThickness * 0.5f), w, h, WallThickness, wallMat, doorSouth);
            BuildWallX(root.transform, "Wall_N", new Vector3(0f, h * 0.5f, d * 0.5f - WallThickness * 0.5f), w, h, WallThickness, wallMat, doorNorth);
            BuildWallZ(root.transform, "Wall_W", new Vector3(-w * 0.5f + WallThickness * 0.5f, h * 0.5f, 0f), WallThickness, h, d, wallMat, doorWest);
            BuildWallZ(root.transform, "Wall_E", new Vector3(w * 0.5f - WallThickness * 0.5f, h * 0.5f, 0f), WallThickness, h, d, wallMat, doorEast);

            var lightGo = new GameObject("InteriorLight");
            lightGo.transform.SetParent(root.transform, false);
            lightGo.transform.localPosition = new Vector3(0f, h * 0.78f, 0f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = Mathf.Max(w, d) * 0.95f + 2f;
            light.intensity = 1.55f;
            light.color = lightColor ?? new Color(1f, 0.93f, 0.8f);
            light.shadows = LightShadows.Soft;

            CreatePart(root.transform, "LightFixture", new Vector3(0f, h - 0.18f, 0f), new Vector3(1.1f, 0.08f, 0.45f), RuntimeMaterials.Metal);
            return root;
        }

        public static GameObject BuildCorridor(
            string name,
            Vector3 center,
            float length,
            float width,
            float height,
            Material wallMat,
            Material floorMat,
            bool axisZ = true,
            Color? lightColor = null)
        {
            if (axisZ)
            {
                return BuildRoom(name, center, new Vector3(width, height, length), wallMat, floorMat, wallMat,
                    doorNorth: true, doorSouth: true, lightColor: lightColor);
            }

            return BuildRoom(name, center, new Vector3(length, height, width), wallMat, floorMat, wallMat,
                doorEast: true, doorWest: true, lightColor: lightColor);
        }

        public static GameObject CreateProp(string name, Vector3 worldPos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = worldPos;
            go.transform.localScale = scale;
            Apply(go, mat);
            return go;
        }

        public static GameObject CreateCylinderProp(string name, Vector3 worldPos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.position = worldPos;
            go.transform.localScale = scale;
            Apply(go, mat);
            return go;
        }

        static void BuildWallX(Transform parent, string name, Vector3 localPos, float width, float height, float thickness, Material mat, bool door)
        {
            if (!door)
            {
                CreatePart(parent, name, localPos, new Vector3(width, height, thickness), mat);
                return;
            }

            float side = (width - DoorWidth) * 0.5f;
            float yBase = localPos.y - height * 0.5f;
            if (side > 0.05f)
            {
                CreatePart(parent, name + "_L",
                    new Vector3(localPos.x - (DoorWidth * 0.5f + side * 0.5f), localPos.y, localPos.z),
                    new Vector3(side, height, thickness), mat);
                CreatePart(parent, name + "_R",
                    new Vector3(localPos.x + (DoorWidth * 0.5f + side * 0.5f), localPos.y, localPos.z),
                    new Vector3(side, height, thickness), mat);
            }

            float lintelH = height - DoorHeight;
            if (lintelH > 0.05f)
            {
                CreatePart(parent, name + "_Lintel",
                    new Vector3(localPos.x, yBase + DoorHeight + lintelH * 0.5f, localPos.z),
                    new Vector3(DoorWidth, lintelH, thickness), mat);
            }

            AddDoorFrameX(parent, name, localPos, yBase, thickness);
        }

        static void BuildWallZ(Transform parent, string name, Vector3 localPos, float thickness, float height, float depth, Material mat, bool door)
        {
            if (!door)
            {
                CreatePart(parent, name, localPos, new Vector3(thickness, height, depth), mat);
                return;
            }

            float side = (depth - DoorWidth) * 0.5f;
            float yBase = localPos.y - height * 0.5f;
            if (side > 0.05f)
            {
                CreatePart(parent, name + "_F",
                    new Vector3(localPos.x, localPos.y, localPos.z - (DoorWidth * 0.5f + side * 0.5f)),
                    new Vector3(thickness, height, side), mat);
                CreatePart(parent, name + "_B",
                    new Vector3(localPos.x, localPos.y, localPos.z + (DoorWidth * 0.5f + side * 0.5f)),
                    new Vector3(thickness, height, side), mat);
            }

            float lintelH = height - DoorHeight;
            if (lintelH > 0.05f)
            {
                CreatePart(parent, name + "_Lintel",
                    new Vector3(localPos.x, yBase + DoorHeight + lintelH * 0.5f, localPos.z),
                    new Vector3(thickness, lintelH, DoorWidth), mat);
            }

            AddDoorFrameZ(parent, name, localPos, yBase, thickness);
        }

        static void AddDoorFrameX(Transform parent, string name, Vector3 localPos, float yBase, float thickness)
        {
            var frame = RuntimeMaterials.Metal;
            const float jam = 0.08f;
            CreatePart(parent, name + "_FrameL",
                new Vector3(localPos.x - DoorWidth * 0.5f, yBase + DoorHeight * 0.5f, localPos.z),
                new Vector3(jam, DoorHeight, thickness + 0.06f), frame);
            CreatePart(parent, name + "_FrameR",
                new Vector3(localPos.x + DoorWidth * 0.5f, yBase + DoorHeight * 0.5f, localPos.z),
                new Vector3(jam, DoorHeight, thickness + 0.06f), frame);
        }

        static void AddDoorFrameZ(Transform parent, string name, Vector3 localPos, float yBase, float thickness)
        {
            var frame = RuntimeMaterials.Metal;
            const float jam = 0.08f;
            CreatePart(parent, name + "_FrameF",
                new Vector3(localPos.x, yBase + DoorHeight * 0.5f, localPos.z - DoorWidth * 0.5f),
                new Vector3(thickness + 0.06f, DoorHeight, jam), frame);
            CreatePart(parent, name + "_FrameB",
                new Vector3(localPos.x, yBase + DoorHeight * 0.5f, localPos.z + DoorWidth * 0.5f),
                new Vector3(thickness + 0.06f, DoorHeight, jam), frame);
        }

        static void CreatePart(Transform parent, string name, Vector3 localPos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            Apply(go, mat);
        }

        static void Apply(GameObject go, Material mat)
        {
            var rend = go.GetComponent<Renderer>();
            if (rend != null && mat != null) rend.sharedMaterial = mat;
        }
    }
}
