using Cipher.AI;
using Cipher.Clues;
using Cipher.Gameplay;
using Cipher.Gameplay.Interactables;
using Cipher.Gameplay.Player;
using Cipher.Gameplay.Weapons;
using Cipher.UI;
using UnityEngine;

namespace Cipher.Bootstrap
{
    /// <summary>
    /// Builds the entire MVP1 playable slice at runtime (graybox primitives).
    /// Open Scenes/MVP1_Blacksite and press Play.
    /// </summary>
    public sealed class Mvp1Bootstrap : MonoBehaviour
    {
        [SerializeField] int seed = 847291;
        [SerializeField] bool buildOnAwake = true;

        bool _built;

        void Awake()
        {
            if (buildOnAwake) Build();
        }

        [ContextMenu("Build MVP1")]
        public void Build()
        {
            if (_built) return;
            _built = true;

            var bootstrapCam = GameObject.Find("BootstrapCamera");
            if (bootstrapCam != null) Destroy(bootstrapCam);

            var db = ClueDatabase.CreateBlacksiteRuntime();
            var matchGo = new GameObject("MatchManager");
            var match = matchGo.AddComponent<MatchManager>();
            match.Configure(db, seed);

            BuildLighting();
            BuildGround();
            BuildZones();
            BuildBuildings();

            var player = BuildPlayer(new Vector3(-18f, 0.1f, 0f));
            var health = player.GetComponent<PlayerHealth>();
            var weapons = player.GetComponentInChildren<WeaponController>();

            BuildClueObjects(db, match);
            BuildBunker(match);
            BuildBots(player.transform, match);

            var hudGo = new GameObject("HUD");
            var hud = hudGo.AddComponent<GameHUD>();
            hud.Bind(match, health, weapons);

            match.BeginMatch();

            // Debug helper: print code in console for QA (not shown in HUD).
            if (match.Mission != null)
            {
                Debug.Log($"[CIPHER MVP1] Seed={match.Mission.seed} CODE={match.Mission.code} Final={match.Mission.finalNodeId}");
            }
        }

        static void BuildLighting()
        {
            if (FindFirstObjectByType<Light>() == null)
            {
                var lightGo = new GameObject("Directional Light");
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.15f;
                light.shadows = LightShadows.Soft;
                lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.35f, 0.37f, 0.4f);
        }

        static void BuildGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(8f, 1f, 8f);
            ground.GetComponent<Renderer>().material.color = new Color(0.22f, 0.23f, 0.22f);
        }

        static void BuildZones()
        {
            CreateZonePad("ZONE Hospital", new Vector3(-28f, 0.02f, 8f), new Vector3(18f, 1f, 16f), new Color(0.28f, 0.25f, 0.22f));
            CreateZonePad("ZONE Industrial", new Vector3(0f, 0.02f, 18f), new Vector3(20f, 1f, 16f), new Color(0.24f, 0.26f, 0.28f));
            CreateZonePad("ZONE Bunker", new Vector3(30f, 0.02f, -6f), new Vector3(16f, 1f, 14f), new Color(0.2f, 0.22f, 0.2f));
        }

        static void CreateZonePad(string name, Vector3 pos, Vector3 scale, Color color)
        {
            var pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pad.name = name;
            pad.transform.position = pos;
            pad.transform.localScale = new Vector3(scale.x, 0.05f, scale.z);
            var marker = pad.AddComponent<ZoneMarker>();
            marker.Setup(name, color);
        }

        static void BuildBuildings()
        {
            // Hospital wing massing
            CreateBox("Hospital_Wing", new Vector3(-30f, 2f, 8f), new Vector3(10f, 4f, 12f), new Color(0.4f, 0.38f, 0.35f));
            CreateBox("Hospital_Corridor", new Vector3(-24f, 1.5f, 10f), new Vector3(4f, 3f, 3f), new Color(0.42f, 0.4f, 0.38f));

            // Building 04
            CreateBox("Building_04", new Vector3(2f, 2.5f, 20f), new Vector3(10f, 5f, 8f), new Color(0.35f, 0.38f, 0.42f));
            CreateBox("Building_04_Room17", new Vector3(2f, 1.5f, 23.5f), new Vector3(4f, 3f, 3f), new Color(0.38f, 0.4f, 0.44f));

            // Cover blocks
            CreateBox("Cover_A", new Vector3(-10f, 0.75f, 6f), new Vector3(2f, 1.5f, 1f), new Color(0.3f, 0.3f, 0.3f));
            CreateBox("Cover_B", new Vector3(12f, 0.75f, 4f), new Vector3(2.5f, 1.5f, 1.2f), new Color(0.3f, 0.3f, 0.3f));
            CreateBox("Cover_C", new Vector3(22f, 0.75f, -2f), new Vector3(2f, 1.5f, 2f), new Color(0.3f, 0.3f, 0.3f));
        }

        static void BuildBunker(MatchManager match)
        {
            CreateBox("Bunker_07", new Vector3(32f, 2f, -10f), new Vector3(10f, 4f, 8f), new Color(0.25f, 0.28f, 0.26f));
            var door = CreateBox("Bunker_Door", new Vector3(32f, 1.5f, -6f), new Vector3(3f, 3f, 0.4f), new Color(0.15f, 0.15f, 0.15f));

            var pad = CreateBox("Bunker_CodePad", new Vector3(32f, 1.2f, -6.5f), new Vector3(0.6f, 0.9f, 0.3f), new Color(0.55f, 0.45f, 0.2f));
            pad.AddComponent<BoxCollider>();
            var code = pad.AddComponent<CodePadInteractable>();
            code.Setup(match);

            // Keep door reference for visual feedback via code pad color only in MVP1.
            door.name = "Bunker_Door";
        }

        static void BuildClueObjects(ClueDatabase db, MatchManager match)
        {
            // Camera_12
            var camNode = db.Find("Camera_12");
            var cam = CreateBox("Camera_12", camNode.worldPosition, new Vector3(0.5f, 0.4f, 0.5f), new Color(0.7f, 0.65f, 0.3f));
            cam.AddComponent<ClueInteractable>().Setup(camNode.id, camNode.displayName, match);

            // Building_04 interactable marker (door panel)
            var b4 = db.Find("Building_04");
            var b4panel = CreateBox("Building_04_Panel", b4.worldPosition + new Vector3(0f, 1f, -4.2f), new Vector3(1f, 1.6f, 0.2f), new Color(0.6f, 0.55f, 0.35f));
            b4panel.AddComponent<ClueInteractable>().Setup(b4.id, b4.displayName, match);

            // Terminal_C
            var term = db.Find("Terminal_C");
            var terminal = CreateBox("Terminal_C", term.worldPosition, new Vector3(0.8f, 1.2f, 0.6f), new Color(0.3f, 0.55f, 0.65f));
            terminal.AddComponent<ClueInteractable>().Setup(term.id, term.displayName, match);

            // Alternate clue props exist in DB for generator variation but are not required in graybox.
            var cam3 = db.Find("Camera_03");
            if (cam3 != null)
            {
                var c3 = CreateBox("Camera_03", cam3.worldPosition, new Vector3(0.45f, 0.35f, 0.45f), new Color(0.55f, 0.5f, 0.28f));
                c3.AddComponent<ClueInteractable>().Setup(cam3.id, cam3.displayName, match);
            }

            var comp = db.Find("Computer_A1");
            if (comp != null)
            {
                var pc = CreateBox("Computer_A1", comp.worldPosition, new Vector3(0.7f, 0.8f, 0.5f), new Color(0.4f, 0.45f, 0.5f));
                pc.AddComponent<ClueInteractable>().Setup(comp.id, comp.displayName, match);
            }
        }

        static GameObject BuildPlayer(Vector3 spawn)
        {
            var player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = spawn;

            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 0.9f, 0f);

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(player.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            Object.Destroy(body.GetComponent<Collider>());
            body.GetComponent<Renderer>().material.color = new Color(0.45f, 0.48f, 0.42f);

            var pivot = new GameObject("CameraPivot").transform;
            pivot.SetParent(player.transform, false);
            pivot.localPosition = new Vector3(0f, 1.6f, 0f);

            var camGo = new GameObject("PlayerCamera");
            camGo.transform.SetParent(pivot, false);
            var cam = camGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.05f;
            cam.fieldOfView = 75f;
            camGo.tag = "MainCamera";
            camGo.AddComponent<AudioListener>();

            var view = GameObject.CreatePrimitive(PrimitiveType.Cube);
            view.name = "ViewWeapon";
            view.transform.SetParent(camGo.transform, false);
            view.transform.localPosition = new Vector3(0.28f, -0.22f, 0.55f);
            view.transform.localScale = new Vector3(0.12f, 0.12f, 0.45f);
            Object.Destroy(view.GetComponent<Collider>());

            var muzzle = new GameObject("Muzzle").transform;
            muzzle.SetParent(camGo.transform, false);
            muzzle.localPosition = new Vector3(0.28f, -0.18f, 0.85f);

            var pc = player.AddComponent<PlayerController>();
            pc.BindCamera(pivot);
            var health = player.AddComponent<PlayerHealth>();
            health.SetSpawn(spawn, Quaternion.identity);

            var weapon = player.AddComponent<WeaponController>();
            weapon.Initialize(muzzle, view.GetComponent<Renderer>());

            return player;
        }

        static void BuildBots(Transform player, MatchManager match)
        {
            SpawnBot("Bot_Patrol_Hospital", new Vector3(-22f, 1f, 12f), new[]
            {
                new Vector3(-22f, 1f, 12f),
                new Vector3(-18f, 1f, 6f),
                new Vector3(-26f, 1f, 4f)
            }, player, match, new Color(0.55f, 0.25f, 0.22f));

            SpawnBot("Bot_Patrol_Industrial", new Vector3(6f, 1f, 14f), new[]
            {
                new Vector3(6f, 1f, 14f),
                new Vector3(-2f, 1f, 16f),
                new Vector3(8f, 1f, 22f)
            }, player, match, new Color(0.5f, 0.28f, 0.2f));

            SpawnBot("Bot_Bunker_Guard", new Vector3(26f, 1f, -4f), new[]
            {
                new Vector3(26f, 1f, -4f),
                new Vector3(28f, 1f, -10f),
                new Vector3(22f, 1f, -8f)
            }, player, match, new Color(0.45f, 0.2f, 0.2f));
        }

        static void SpawnBot(string name, Vector3 pos, Vector3[] waypoints, Transform player, MatchManager match, Color color)
        {
            var bot = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bot.name = name;
            bot.transform.position = pos;
            bot.GetComponent<Renderer>().material.color = color;
            var ai = bot.AddComponent<BotController>();
            ai.Setup(waypoints, player, match, color);
        }

        static GameObject CreateBox(string name, Vector3 pos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().material.color = color;
            return go;
        }
    }
}
