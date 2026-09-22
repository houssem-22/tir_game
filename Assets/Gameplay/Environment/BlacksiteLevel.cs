using Cipher.Gameplay;
using UnityEngine;

namespace Cipher.Gameplay.Environment
{
    /// <summary>
    /// BLACKSITE layout: enterable Hospital → Outdoor path → Building 04 → Bunker.
    /// Rooms are hollow with doorways — not solid cubes.
    /// </summary>
    public static class BlacksiteLevel
    {
        static readonly Color HospitalLight = new Color(1f, 0.92f, 0.78f);
        static readonly Color IndustrialLight = new Color(0.85f, 0.9f, 1f);
        static readonly Color BunkerLight = new Color(0.55f, 0.85f, 0.6f);

        public static void Build()
        {
            BuildTerrain();
            BuildHospital();
            BuildIndustrial();
            BuildBunkerCompound();
            BuildExteriorCover();
            BuildPathMarkers();
        }

        static void BuildTerrain()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground_Asphalt";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(12f, 1f, 12f);
            ground.GetComponent<Renderer>().sharedMaterial = RuntimeMaterials.Asphalt;

            LabelPad("ZONE_Hospital", new Vector3(-30f, 0.03f, 6f), new Vector3(22f, 0.04f, 18f), RuntimeMaterials.Plaster, new Color(0.85f, 0.82f, 0.7f));
            LabelPad("ZONE_Industrial", new Vector3(2f, 0.03f, 20f), new Vector3(22f, 0.04f, 18f), RuntimeMaterials.Concrete, new Color(0.7f, 0.78f, 0.85f));
            LabelPad("ZONE_Bunker", new Vector3(34f, 0.03f, -8f), new Vector3(18f, 0.04f, 16f), RuntimeMaterials.Rust, new Color(0.95f, 0.7f, 0.35f));
        }

        static void LabelPad(string name, Vector3 pos, Vector3 scale, Material mat, Color labelColor)
        {
            var pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pad.name = name;
            pad.transform.position = pos;
            pad.transform.localScale = scale;
            pad.GetComponent<Renderer>().sharedMaterial = mat;
            Object.Destroy(pad.GetComponent<Collider>());
            var marker = pad.AddComponent<ZoneMarker>();
            marker.Setup(name.Replace('_', ' '), labelColor);
        }

        static void BuildHospital()
        {
            ModularBuilder.BuildRoom(
                "Hospital_Lobby",
                new Vector3(-28f, 0f, 2f),
                new Vector3(10f, 3.2f, 8f),
                RuntimeMaterials.Plaster,
                RuntimeMaterials.Floor,
                RuntimeMaterials.Concrete,
                doorSouth: true,
                doorNorth: true,
                lightColor: HospitalLight);

            ModularBuilder.BuildCorridor(
                "Hospital_Corridor",
                new Vector3(-28f, 0f, 10f),
                length: 8f,
                width: 4f,
                height: 3f,
                RuntimeMaterials.Plaster,
                RuntimeMaterials.Floor,
                axisZ: true,
                lightColor: HospitalLight);

            ModularBuilder.BuildRoom(
                "Hospital_CameraRoom",
                new Vector3(-28f, 0f, 17f),
                new Vector3(8f, 3.2f, 6f),
                RuntimeMaterials.Plaster,
                RuntimeMaterials.Floor,
                RuntimeMaterials.Concrete,
                doorSouth: true,
                lightColor: HospitalLight);

            ModularBuilder.CreateProp("Hospital_Desk", new Vector3(-30.5f, 0.45f, 2.5f), new Vector3(1.8f, 0.9f, 0.75f), RuntimeMaterials.Metal);
            ModularBuilder.CreateProp("Hospital_CrateA", new Vector3(-25.5f, 0.45f, 3.2f), new Vector3(0.9f, 0.9f, 0.9f), RuntimeMaterials.Rust);
            ModularBuilder.CreateCylinderProp("Hospital_Pillar", new Vector3(-29.6f, 1.5f, 10f), new Vector3(0.35f, 1.5f, 0.35f), RuntimeMaterials.Concrete);

            ModularBuilder.CreateProp("Hospital_Facade_L", new Vector3(-34.2f, 2f, 2f), new Vector3(1.2f, 4f, 10f), RuntimeMaterials.Concrete);
            ModularBuilder.CreateProp("Hospital_Facade_R", new Vector3(-21.8f, 2f, 2f), new Vector3(1.2f, 4f, 10f), RuntimeMaterials.Concrete);
            ModularBuilder.CreateProp("Hospital_Sign", new Vector3(-28f, 2.7f, -2.15f), new Vector3(2.4f, 0.45f, 0.12f), RuntimeMaterials.AccentWarn);
        }

        static void BuildIndustrial()
        {
            ModularBuilder.CreateProp("Road_Block_A", new Vector3(-12f, 0.4f, 12f), new Vector3(1.2f, 0.8f, 2.4f), RuntimeMaterials.Concrete);
            ModularBuilder.CreateProp("Road_Block_B", new Vector3(-4f, 0.5f, 14f), new Vector3(2f, 1f, 1f), RuntimeMaterials.Rust);

            ModularBuilder.BuildRoom(
                "Building_04_Hall",
                new Vector3(2f, 0f, 18f),
                new Vector3(12f, 3.6f, 10f),
                RuntimeMaterials.Concrete,
                RuntimeMaterials.Floor,
                RuntimeMaterials.Metal,
                doorWest: true,
                doorNorth: true,
                doorSouth: true,
                lightColor: IndustrialLight);

            ModularBuilder.BuildRoom(
                "Building_04_Room17",
                new Vector3(2f, 0f, 26f),
                new Vector3(8f, 3.2f, 6f),
                RuntimeMaterials.Concrete,
                RuntimeMaterials.Floor,
                RuntimeMaterials.Metal,
                doorSouth: true,
                lightColor: IndustrialLight);

            ModularBuilder.CreateProp("B04_ServerRack", new Vector3(5.5f, 1.1f, 18.5f), new Vector3(0.8f, 2.2f, 0.6f), RuntimeMaterials.Metal);
            ModularBuilder.CreateProp("B04_Workbench", new Vector3(-1.5f, 0.6f, 20f), new Vector3(2.2f, 0.9f, 0.8f), RuntimeMaterials.Rust);
            ModularBuilder.CreateProp("B04_TerminalDesk", new Vector3(4.4f, 0.45f, 27.2f), new Vector3(1.3f, 0.9f, 0.7f), RuntimeMaterials.Metal);
            ModularBuilder.CreateProp("B04_Sign", new Vector3(2f, 2.9f, 12.9f), new Vector3(2.2f, 0.4f, 0.12f), RuntimeMaterials.AccentWarn);
        }

        static void BuildBunkerCompound()
        {
            ModularBuilder.CreateProp("Bunker_Sandbag_L", new Vector3(24f, 0.45f, -2f), new Vector3(3f, 0.9f, 1f), RuntimeMaterials.Rust);
            ModularBuilder.CreateProp("Bunker_Sandbag_R", new Vector3(28f, 0.45f, -2f), new Vector3(3f, 0.9f, 1f), RuntimeMaterials.Rust);

            ModularBuilder.BuildRoom(
                "Bunker_07_Antechamber",
                new Vector3(34f, 0f, -6f),
                new Vector3(10f, 3.4f, 8f),
                RuntimeMaterials.Concrete,
                RuntimeMaterials.Floor,
                RuntimeMaterials.Metal,
                doorSouth: true,
                doorNorth: true,
                lightColor: BunkerLight);

            ModularBuilder.BuildRoom(
                "Bunker_07_Vault",
                new Vector3(34f, 0f, -13f),
                new Vector3(8f, 3.2f, 6f),
                RuntimeMaterials.Metal,
                RuntimeMaterials.Floor,
                RuntimeMaterials.Metal,
                doorNorth: true,
                lightColor: BunkerLight);

            ModularBuilder.CreateProp("Bunker_Support", new Vector3(30.5f, 1.4f, -6f), new Vector3(0.5f, 2.8f, 0.5f), RuntimeMaterials.Metal);
            ModularBuilder.CreateProp("Bunker_Support2", new Vector3(37.5f, 1.4f, -6f), new Vector3(0.5f, 2.8f, 0.5f), RuntimeMaterials.Metal);
            ModularBuilder.CreateProp("Bunker_Sign", new Vector3(34f, 2.7f, -9.85f), new Vector3(2.6f, 0.4f, 0.12f), RuntimeMaterials.AccentIntel);
        }

        static void BuildExteriorCover()
        {
            ModularBuilder.CreateProp("Cover_Concrete_A", new Vector3(-16f, 0.7f, 6f), new Vector3(2.2f, 1.4f, 0.8f), RuntimeMaterials.Concrete);
            ModularBuilder.CreateProp("Cover_Concrete_B", new Vector3(12f, 0.7f, 8f), new Vector3(2.5f, 1.4f, 1f), RuntimeMaterials.Concrete);
            ModularBuilder.CreateProp("Cover_Barrels", new Vector3(18f, 0.55f, 0f), new Vector3(1.2f, 1.1f, 1.2f), RuntimeMaterials.Rust);
            ModularBuilder.CreateCylinderProp("LightPole_A", new Vector3(-8f, 2.2f, 4f), new Vector3(0.25f, 2.2f, 0.25f), RuntimeMaterials.Metal);
            ModularBuilder.CreateCylinderProp("LightPole_B", new Vector3(20f, 2.2f, 12f), new Vector3(0.25f, 2.2f, 0.25f), RuntimeMaterials.Metal);
            AttachPoleLight("LightPole_A_Lamp", new Vector3(-8f, 4.1f, 4f), new Color(1f, 0.88f, 0.65f));
            AttachPoleLight("LightPole_B_Lamp", new Vector3(20f, 4.1f, 12f), new Color(1f, 0.88f, 0.65f));
        }

        static void AttachPoleLight(string name, Vector3 pos, Color color)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 14f;
            light.intensity = 2.2f;
            light.color = color;
        }

        static void BuildPathMarkers()
        {
            PlaceChevron(new Vector3(-28f, 0.06f, -3.2f), 0f);
            PlaceChevron(new Vector3(-18f, 0.06f, 1.5f), 90f);
            PlaceChevron(new Vector3(-18f, 0.06f, 12f), 90f);
            PlaceChevron(new Vector3(2f, 0.06f, 12.5f), 0f);
            PlaceChevron(new Vector3(20f, 0.06f, 4f), 90f);
            PlaceChevron(new Vector3(34f, 0.06f, -1.5f), 180f);
        }

        static void PlaceChevron(Vector3 pos, float yaw)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "PathChevron";
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            go.transform.localScale = new Vector3(0.9f, 0.04f, 0.28f);
            go.GetComponent<Renderer>().sharedMaterial = RuntimeMaterials.AccentWarn;
            Object.Destroy(go.GetComponent<Collider>());
        }
    }
}
