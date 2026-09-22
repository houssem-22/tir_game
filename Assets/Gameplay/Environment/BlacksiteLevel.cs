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

            // Zone labels (thin pads, not blocking)
            LabelPad("ZONE_Hospital", new Vector3(-30f, 0.03f, 6f), new Vector3(22f, 0.04f, 18f), RuntimeMaterials.Plaster);
            LabelPad("ZONE_Industrial", new Vector3(2f, 0.03f, 20f), new Vector3(22f, 0.04f, 18f), RuntimeMaterials.Concrete);
            LabelPad("ZONE_Bunker", new Vector3(34f, 0.03f, -8f), new Vector3(18f, 0.04f, 16f), RuntimeMaterials.Rust);
        }

        static void LabelPad(string name, Vector3 pos, Vector3 scale, Material mat)
        {
            var pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pad.name = name;
            pad.transform.position = pos;
            pad.transform.localScale = scale;
            pad.GetComponent<Renderer>().sharedMaterial = mat;
            Object.Destroy(pad.GetComponent<Collider>());
            var marker = pad.AddComponent<ZoneMarker>();
            marker.Setup(name.Replace('_', ' '), Color.white);
        }

        static void BuildHospital()
        {
            // Lobby — door SOUTH toward spawn
            ModularBuilder.BuildRoom(
                "Hospital_Lobby",
                new Vector3(-28f, 0f, 2f),
                new Vector3(10f, 3.2f, 8f),
                RuntimeMaterials.Plaster,
                RuntimeMaterials.Floor,
                RuntimeMaterials.Concrete,
                doorSouth: true,
                doorNorth: true);

            // Corridor toward Camera room
            ModularBuilder.BuildCorridor(
                "Hospital_Corridor",
                new Vector3(-28f, 0f, 10f),
                length: 8f,
                width: 4f,
                height: 3f,
                RuntimeMaterials.Plaster,
                RuntimeMaterials.Floor,
                axisZ: true);

            // Camera room (enterable) — Camera_12 lives here
            ModularBuilder.BuildRoom(
                "Hospital_CameraRoom",
                new Vector3(-28f, 0f, 16f),
                new Vector3(8f, 3.2f, 6f),
                RuntimeMaterials.Plaster,
                RuntimeMaterials.Floor,
                RuntimeMaterials.Concrete,
                doorSouth: true);

            // Interior props (not solid building masses)
            ModularBuilder.CreateProp("Hospital_Desk", new Vector3(-30.5f, 0.55f, 2.5f), new Vector3(1.6f, 0.9f, 0.7f), RuntimeMaterials.Metal);
            ModularBuilder.CreateProp("Hospital_CrateA", new Vector3(-25.5f, 0.45f, 3.2f), new Vector3(0.9f, 0.9f, 0.9f), RuntimeMaterials.Rust);
            ModularBuilder.CreateCylinderProp("Hospital_Pillar", new Vector3(-28f, 1.5f, 10f), new Vector3(0.45f, 1.5f, 0.45f), RuntimeMaterials.Concrete);

            // Exterior facade strip (visual mass without blocking the door)
            ModularBuilder.CreateProp("Hospital_Facade_L", new Vector3(-34f, 2f, 2f), new Vector3(1.2f, 4f, 10f), RuntimeMaterials.Concrete);
            ModularBuilder.CreateProp("Hospital_Facade_R", new Vector3(-22f, 2f, 2f), new Vector3(1.2f, 4f, 10f), RuntimeMaterials.Concrete);
        }

        static void BuildIndustrial()
        {
            // Outdoor approach between hospital exit and Building 04
            ModularBuilder.CreateProp("Road_Block_A", new Vector3(-12f, 0.4f, 12f), new Vector3(1.2f, 0.8f, 2.4f), RuntimeMaterials.Concrete);
            ModularBuilder.CreateProp("Road_Block_B", new Vector3(-4f, 0.5f, 14f), new Vector3(2f, 1f, 1f), RuntimeMaterials.Rust);

            // Building 04 main hall — door WEST from path, door NORTH to Room 17
            ModularBuilder.BuildRoom(
                "Building_04_Hall",
                new Vector3(2f, 0f, 18f),
                new Vector3(12f, 3.6f, 10f),
                RuntimeMaterials.Concrete,
                RuntimeMaterials.Floor,
                RuntimeMaterials.Metal,
                doorWest: true,
                doorNorth: true,
                doorSouth: true);

            // Room 17 + Terminal C
            ModularBuilder.BuildRoom(
                "Building_04_Room17",
                new Vector3(2f, 0f, 26f),
                new Vector3(8f, 3.2f, 6f),
                RuntimeMaterials.Concrete,
                RuntimeMaterials.Floor,
                RuntimeMaterials.Metal,
                doorSouth: true);

            ModularBuilder.CreateProp("B04_ServerRack", new Vector3(5.5f, 1.1f, 18.5f), new Vector3(0.8f, 2.2f, 0.6f), RuntimeMaterials.Metal);
            ModularBuilder.CreateProp("B04_Workbench", new Vector3(-1.5f, 0.6f, 20f), new Vector3(2.2f, 0.9f, 0.8f), RuntimeMaterials.Rust);
            ModularBuilder.CreateProp("B04_Panel_Interact", new Vector3(-3.5f, 1.2f, 14.8f), new Vector3(1.2f, 1.8f, 0.25f), RuntimeMaterials.AccentWarn);
        }

        static void BuildBunkerCompound()
        {
            // Approach trench
            ModularBuilder.CreateProp("Bunker_Sandbag_L", new Vector3(24f, 0.45f, -2f), new Vector3(3f, 0.9f, 1f), RuntimeMaterials.Rust);
            ModularBuilder.CreateProp("Bunker_Sandbag_R", new Vector3(28f, 0.45f, -2f), new Vector3(3f, 0.9f, 1f), RuntimeMaterials.Rust);

            // Bunker antechamber — door SOUTH
            ModularBuilder.BuildRoom(
                "Bunker_07_Antechamber",
                new Vector3(34f, 0f, -6f),
                new Vector3(10f, 3.4f, 8f),
                RuntimeMaterials.Concrete,
                RuntimeMaterials.Floor,
                RuntimeMaterials.Metal,
                doorSouth: true,
                doorNorth: true);

            // Inner vault / code room
            ModularBuilder.BuildRoom(
                "Bunker_07_Vault",
                new Vector3(34f, 0f, -13f),
                new Vector3(8f, 3.2f, 6f),
                RuntimeMaterials.Metal,
                RuntimeMaterials.Floor,
                RuntimeMaterials.Metal,
                doorSouth: true);

            ModularBuilder.CreateProp("Bunker_Support", new Vector3(30.5f, 1.4f, -6f), new Vector3(0.5f, 2.8f, 0.5f), RuntimeMaterials.Metal);
            ModularBuilder.CreateProp("Bunker_Support2", new Vector3(37.5f, 1.4f, -6f), new Vector3(0.5f, 2.8f, 0.5f), RuntimeMaterials.Metal);
        }

        static void BuildExteriorCover()
        {
            ModularBuilder.CreateProp("Cover_Concrete_A", new Vector3(-16f, 0.7f, 6f), new Vector3(2.2f, 1.4f, 0.8f), RuntimeMaterials.Concrete);
            ModularBuilder.CreateProp("Cover_Concrete_B", new Vector3(12f, 0.7f, 8f), new Vector3(2.5f, 1.4f, 1f), RuntimeMaterials.Concrete);
            ModularBuilder.CreateProp("Cover_Barrels", new Vector3(18f, 0.55f, 0f), new Vector3(1.2f, 1.1f, 1.2f), RuntimeMaterials.Rust);
            ModularBuilder.CreateCylinderProp("LightPole_A", new Vector3(-8f, 2.2f, 4f), new Vector3(0.25f, 2.2f, 0.25f), RuntimeMaterials.Metal);
            ModularBuilder.CreateCylinderProp("LightPole_B", new Vector3(20f, 2.2f, 12f), new Vector3(0.25f, 2.2f, 0.25f), RuntimeMaterials.Metal);
        }

        static void BuildPathMarkers()
        {
            // Subtle objective chevrons on ground (no collision)
            PlaceChevron(new Vector3(-18f, 0.06f, 1.5f));
            PlaceChevron(new Vector3(-28f, 0.06f, -3.2f));
            PlaceChevron(new Vector3(-18f, 0.06f, 12f));
            PlaceChevron(new Vector3(2f, 0.06f, 12.5f));
            PlaceChevron(new Vector3(20f, 0.06f, 4f));
            PlaceChevron(new Vector3(34f, 0.06f, -1.5f));
        }

        static void PlaceChevron(Vector3 pos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "PathChevron";
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.8f, 0.04f, 0.35f);
            go.GetComponent<Renderer>().sharedMaterial = RuntimeMaterials.AccentWarn;
            Object.Destroy(go.GetComponent<Collider>());
        }
    }
}
