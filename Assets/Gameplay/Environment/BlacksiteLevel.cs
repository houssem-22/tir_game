using Cipher.Gameplay;
using UnityEngine;

namespace Cipher.Gameplay.Environment
{
    /// <summary>
    /// BLACKSITE on a 4 m modular grid: Hospital → yard → Building 04 → Bunker.
    /// </summary>
    public static class BlacksiteLevel
    {
        static readonly Color HospitalLight = new Color(1f, 0.92f, 0.78f);
        static readonly Color IndustrialLight = new Color(0.85f, 0.9f, 1f);
        static readonly Color BunkerLight = new Color(0.55f, 0.85f, 0.6f);

        public static Vector3 PlayerSpawn => new Vector3(-26f, 0.08f, -3.2f);

        public static void Build()
        {
            ModularKit.Begin();
            BuildTerrain();
            BuildHospital();
            BuildIndustrial();
            BuildBunkerCompound();
            BuildExteriorCover();
        }

        static void BuildTerrain()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground_Asphalt";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(14f, 1f, 14f);
            ground.GetComponent<Renderer>().sharedMaterial = RuntimeMaterials.Asphalt;

            LabelPad("ZONE_Hospital", new Vector3(-26f, 0.03f, 8f), new Vector3(18f, 0.04f, 22f), RuntimeMaterials.Plaster, new Color(0.85f, 0.82f, 0.7f));
            LabelPad("ZONE_Industrial", new Vector3(6f, 0.03f, 22f), new Vector3(18f, 0.04f, 20f), RuntimeMaterials.Concrete, new Color(0.7f, 0.78f, 0.85f));
            LabelPad("ZONE_Bunker", new Vector3(36f, 0.03f, -8f), new Vector3(16f, 0.04f, 18f), RuntimeMaterials.Rust, new Color(0.95f, 0.7f, 0.35f));
        }

        static void LabelPad(string name, Vector3 pos, Vector3 scale, Material mat, Color labelColor)
        {
            var pad = ModularKit.CreateProp(name, pos, scale, mat, collider: false);
            pad.AddComponent<ZoneMarker>().Setup(name.Replace('_', ' '), labelColor);
        }

        static void BuildHospital()
        {
            ModularKit.Fill(
                "Hospital_Lobby",
                gx: -8, gz: 0, tilesX: 3, tilesZ: 2,
                RuntimeMaterials.Plaster, RuntimeMaterials.Floor, RuntimeMaterials.Concrete,
                ModularKit.Opening.South | ModularKit.Opening.North,
                HospitalLight);

            ModularKit.Fill(
                "Hospital_Corridor",
                gx: -7, gz: 2, tilesX: 1, tilesZ: 2,
                RuntimeMaterials.Plaster, RuntimeMaterials.Floor, RuntimeMaterials.Concrete,
                ModularKit.Opening.South | ModularKit.Opening.North,
                HospitalLight);

            ModularKit.Fill(
                "Hospital_CameraRoom",
                gx: -8, gz: 4, tilesX: 2, tilesZ: 2,
                RuntimeMaterials.Plaster, RuntimeMaterials.Floor, RuntimeMaterials.Concrete,
                ModularKit.Opening.South,
                HospitalLight);

            ModularKit.CreateProp("Hospital_Desk", new Vector3(-29.6f, 0.4f, 3.2f), new Vector3(1.4f, 0.8f, 0.65f), RuntimeMaterials.Metal);
            ModularKit.CreateProp("Hospital_CrateA", new Vector3(-22.5f, 0.4f, 5.5f), new Vector3(0.8f, 0.8f, 0.8f), RuntimeMaterials.Rust);
            ModularKit.CreateProp("Hospital_Sign", new Vector3(-26f, 2.7f, -0.18f), new Vector3(2.2f, 0.4f, 0.1f), RuntimeMaterials.AccentWarn, collider: false);
        }

        static void BuildIndustrial()
        {
            ModularKit.CreateProp("Road_Block_A", new Vector3(-12f, 0.4f, 12f), new Vector3(1.2f, 0.8f, 2.4f), RuntimeMaterials.Concrete);
            ModularKit.CreateProp("Road_Block_B", new Vector3(-4f, 0.45f, 14f), new Vector3(2f, 0.9f, 1f), RuntimeMaterials.Rust);

            ModularKit.Fill(
                "Building_04_Hall",
                gx: 0, gz: 4, tilesX: 3, tilesZ: 2,
                RuntimeMaterials.Concrete, RuntimeMaterials.Floor, RuntimeMaterials.Metal,
                ModularKit.Opening.West | ModularKit.Opening.North | ModularKit.Opening.South,
                IndustrialLight);

            ModularKit.Fill(
                "Building_04_Room17",
                gx: 0, gz: 6, tilesX: 2, tilesZ: 2,
                RuntimeMaterials.Concrete, RuntimeMaterials.Floor, RuntimeMaterials.Metal,
                ModularKit.Opening.South,
                IndustrialLight);

            ModularKit.CreateProp("B04_ServerRack", new Vector3(10.2f, 1.1f, 18.5f), new Vector3(0.7f, 2.2f, 0.55f), RuntimeMaterials.Metal);
            ModularKit.CreateProp("B04_Workbench", new Vector3(3.5f, 0.45f, 21.5f), new Vector3(1.8f, 0.9f, 0.7f), RuntimeMaterials.Rust);
            ModularKit.CreateProp("B04_TerminalDesk", new Vector3(6.2f, 0.4f, 28.5f), new Vector3(1.2f, 0.8f, 0.65f), RuntimeMaterials.Metal);
            ModularKit.CreateProp("B04_Sign", new Vector3(6f, 2.7f, 15.85f), new Vector3(2.2f, 0.35f, 0.1f), RuntimeMaterials.AccentWarn, collider: false);
        }

        static void BuildBunkerCompound()
        {
            ModularKit.CreateProp("Bunker_Sandbag_L", new Vector3(28f, 0.4f, 1.2f), new Vector3(2.4f, 0.8f, 0.9f), RuntimeMaterials.Rust);
            ModularKit.CreateProp("Bunker_Sandbag_R", new Vector3(32f, 0.4f, 1.2f), new Vector3(2.4f, 0.8f, 0.9f), RuntimeMaterials.Rust);

            ModularKit.Fill(
                "Bunker_07_Antechamber",
                gx: 8, gz: -2, tilesX: 2, tilesZ: 2,
                RuntimeMaterials.Concrete, RuntimeMaterials.Floor, RuntimeMaterials.Metal,
                ModularKit.Opening.North | ModularKit.Opening.South,
                BunkerLight);

            ModularKit.Fill(
                "Bunker_07_Vault",
                gx: 8, gz: -4, tilesX: 2, tilesZ: 2,
                RuntimeMaterials.Metal, RuntimeMaterials.Floor, RuntimeMaterials.Metal,
                ModularKit.Opening.North,
                BunkerLight);

            ModularKit.CreateProp("Bunker_Sign", new Vector3(36f, 2.7f, -0.18f), new Vector3(2.4f, 0.35f, 0.1f), RuntimeMaterials.AccentIntel, collider: false);
        }

        static void BuildExteriorCover()
        {
            ModularKit.CreateProp("Cover_Concrete_A", new Vector3(-16f, 0.65f, 6f), new Vector3(2.2f, 1.3f, 0.8f), RuntimeMaterials.Concrete);
            ModularKit.CreateProp("Cover_Concrete_B", new Vector3(16f, 0.65f, 10f), new Vector3(2.5f, 1.3f, 1f), RuntimeMaterials.Concrete);
            ModularKit.CreateProp("Cover_Barrels", new Vector3(20f, 0.5f, 2f), new Vector3(1.1f, 1f, 1.1f), RuntimeMaterials.Rust);
            ModularKit.CreateCylinderProp("LightPole_A", new Vector3(-10f, 2.2f, 4f), new Vector3(0.22f, 2.2f, 0.22f), RuntimeMaterials.Metal, collider: false);
            ModularKit.CreateCylinderProp("LightPole_B", new Vector3(18f, 2.2f, 12f), new Vector3(0.22f, 2.2f, 0.22f), RuntimeMaterials.Metal, collider: false);
        }
    }
}
