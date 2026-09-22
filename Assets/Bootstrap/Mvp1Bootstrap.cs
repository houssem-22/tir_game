using Cipher.AI;
using Cipher.Clues;
using Cipher.Gameplay;
using Cipher.Gameplay.Environment;
using Cipher.Gameplay.Interactables;
using Cipher.Gameplay.Player;
using Cipher.Gameplay.Weapons;
using Cipher.UI;
using UnityEngine;

namespace Cipher.Bootstrap
{
    /// <summary>
    /// Phase 3 BLACKSITE bootstrap: 4 m modular kit, LOS bots, objective compass.
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

        [ContextMenu("Build BLACKSITE")]
        public void Build()
        {
            if (_built) return;
            _built = true;

            var bootstrapCam = GameObject.Find("BootstrapCamera");
            if (bootstrapCam != null) Destroy(bootstrapCam);

            GameplayUi.Reset();
            ClueDatabase.ClearRuntimeCache();
            var db = ClueDatabase.CreateBlacksiteRuntime();
            var matchGo = new GameObject("MatchManager");
            var match = matchGo.AddComponent<MatchManager>();
            match.Configure(db, seed);

            BuildLighting();
            BlacksiteLevel.Build();

            // Spawn south of Hospital lobby door — walk north into lobby
            var player = BuildPlayer(BlacksiteLevel.PlayerSpawn);
            var health = player.GetComponent<PlayerHealth>();
            var weapons = player.GetComponentInChildren<WeaponController>();

            BuildClueObjects(db, match);
            BuildCodePad(match);
            BuildBots(player.transform, match);

            var hudGo = new GameObject("HUD");
            hudGo.AddComponent<GameHUD>().Bind(match, health, weapons);

            match.BeginMatch();
            if (match.Mission != null)
            {
                Debug.Log($"[CIPHER] Seed={match.Mission.seed} CODE={match.Mission.code} Final={match.Mission.finalNodeId}");
                Debug.Log("[CIPHER] Path: Hospital lobby computer / cameras → Building 04 west panel → Room 17 terminal → Bunker vault pad.");
            }
        }

        static void BuildLighting()
        {
            if (FindFirstObjectByType<Light>() == null)
            {
                var lightGo = new GameObject("Directional Light");
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = new Color(0.7f, 0.78f, 0.95f);
                light.intensity = 1.15f;
                light.shadows = LightShadows.Soft;
                lightGo.transform.rotation = Quaternion.Euler(42f, -35f, 0f);
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.28f, 0.32f, 0.38f);
            RenderSettings.ambientEquatorColor = new Color(0.18f, 0.18f, 0.16f);
            RenderSettings.ambientGroundColor = new Color(0.08f, 0.07f, 0.06f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.16f, 0.18f, 0.2f);
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.01f;
        }

        static void BuildClueObjects(ClueDatabase db, MatchManager match)
        {
            SpawnClue(db, match, "Camera_12", new Vector3(0.42f, 0.28f, 0.55f), RuntimeMaterials.AccentWarn);
            SpawnClue(db, match, "Building_04", new Vector3(0.18f, 1.5f, 1.2f), RuntimeMaterials.AccentWarn);
            SpawnClue(db, match, "Terminal_C", new Vector3(0.85f, 1.05f, 0.55f), RuntimeMaterials.AccentIntel);
            SpawnClue(db, match, "Camera_03", new Vector3(0.38f, 0.24f, 0.5f), RuntimeMaterials.AccentWarn);
            SpawnClue(db, match, "Computer_A1", new Vector3(0.55f, 0.12f, 0.45f), RuntimeMaterials.Metal);
        }

        static void SpawnClue(ClueDatabase db, MatchManager match, string id, Vector3 scale, Material mat)
        {
            var node = db.Find(id);
            if (node == null) return;
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = id;
            go.transform.position = node.worldPosition;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            go.AddComponent<ClueInteractable>().Setup(node.id, node.displayName, match);
        }

        static void BuildCodePad(MatchManager match)
        {
            var node = match.Database.Find("Bunker_Terminal");
            var pos = node != null ? node.worldPosition : new Vector3(34f, 1.15f, -13f);
            var pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pad.name = "Bunker_CodePad";
            pad.transform.position = pos;
            pad.transform.localScale = new Vector3(0.55f, 1.1f, 0.28f);
            pad.GetComponent<Renderer>().sharedMaterial = RuntimeMaterials.AccentWarn;
            pad.AddComponent<CodePadInteractable>().Setup(match);
        }

        static GameObject BuildPlayer(Vector3 spawn)
        {
            var player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = spawn;
            player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.stepOffset = 0.3f;
            controller.skinWidth = 0.08f;
            controller.minMoveDistance = 0f;
            controller.slopeLimit = 45f;

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(player.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            Object.Destroy(body.GetComponent<Collider>());
            body.GetComponent<Renderer>().sharedMaterial = RuntimeMaterials.Metal;

            var pivot = new GameObject("CameraPivot").transform;
            pivot.SetParent(player.transform, false);
            pivot.localPosition = new Vector3(0f, 1.6f, 0f);

            var camGo = new GameObject("PlayerCamera");
            camGo.transform.SetParent(pivot, false);
            var cam = camGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.05f;
            cam.fieldOfView = 70f;
            cam.farClipPlane = 180f;
            cam.backgroundColor = new Color(0.07f, 0.09f, 0.11f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.tag = "MainCamera";
            camGo.AddComponent<AudioListener>();

            var viewRoot = new GameObject("ViewWeapon");
            viewRoot.transform.SetParent(camGo.transform, false);
            viewRoot.transform.localPosition = new Vector3(0.28f, -0.22f, 0.52f);

            var bodyGun = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bodyGun.name = "Receiver";
            bodyGun.transform.SetParent(viewRoot.transform, false);
            bodyGun.transform.localPosition = Vector3.zero;
            bodyGun.transform.localScale = new Vector3(0.08f, 0.1f, 0.28f);
            Object.Destroy(bodyGun.GetComponent<Collider>());
            bodyGun.GetComponent<Renderer>().sharedMaterial = RuntimeMaterials.Metal;

            var barrel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barrel.name = "Barrel";
            barrel.transform.SetParent(viewRoot.transform, false);
            barrel.transform.localPosition = new Vector3(0f, 0.02f, 0.28f);
            barrel.transform.localScale = new Vector3(0.035f, 0.035f, 0.32f);
            Object.Destroy(barrel.GetComponent<Collider>());
            barrel.GetComponent<Renderer>().sharedMaterial = RuntimeMaterials.Metal;

            var mag = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mag.name = "Mag";
            mag.transform.SetParent(viewRoot.transform, false);
            mag.transform.localPosition = new Vector3(0f, -0.08f, 0.02f);
            mag.transform.localScale = new Vector3(0.05f, 0.12f, 0.08f);
            Object.Destroy(mag.GetComponent<Collider>());
            mag.GetComponent<Renderer>().sharedMaterial = RuntimeMaterials.AccentMuted;

            var muzzle = new GameObject("Muzzle").transform;
            muzzle.SetParent(camGo.transform, false);
            muzzle.localPosition = new Vector3(0.28f, -0.2f, 0.95f);

            player.AddComponent<PlayerHealth>().SetSpawn(spawn, player.transform.rotation);
            player.AddComponent<PlayerController>().BindCamera(pivot);
            player.AddComponent<WeaponController>().Initialize(muzzle, bodyGun.GetComponent<Renderer>());
            return player;
        }

        static void BuildBots(Transform player, MatchManager match)
        {
            SpawnBot("Bot_Yard_East", new Vector3(-17f, 1f, 8f), new[]
            {
                new Vector3(-17f, 1f, 8f), new Vector3(-13f, 1f, 4f), new Vector3(-17f, 1f, 12f)
            }, player, match, new Color(0.45f, 0.2f, 0.18f));

            SpawnBot("Bot_Yard_South", new Vector3(-17f, 1f, -1.5f), new[]
            {
                new Vector3(-17f, 1f, -1.5f), new Vector3(-12f, 1f, -1.5f), new Vector3(-14f, 1f, 3f)
            }, player, match, new Color(0.5f, 0.25f, 0.15f));

            SpawnBot("Bot_Industrial", new Vector3(16f, 1f, 14f), new[]
            {
                new Vector3(16f, 1f, 14f), new Vector3(14f, 1f, 22f), new Vector3(20f, 1f, 18f)
            }, player, match, new Color(0.42f, 0.22f, 0.18f));

            SpawnBot("Bot_Bunker_Approach", new Vector3(28f, 1f, 4f), new[]
            {
                new Vector3(28f, 1f, 4f), new Vector3(40f, 1f, 4f), new Vector3(34f, 1f, 2f)
            }, player, match, new Color(0.38f, 0.16f, 0.14f));
        }

        static void SpawnBot(string name, Vector3 pos, Vector3[] waypoints, Transform player, MatchManager match, Color color)
        {
            var bot = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bot.name = name;
            bot.transform.position = pos;
            var cap = bot.GetComponent<CapsuleCollider>();
            if (cap != null)
            {
                cap.enabled = false;
                Destroy(cap);
            }

            var body = bot.GetComponent<Renderer>();
            body.sharedMaterial = RuntimeMaterials.Rust;
            body.material.color = color;

            var visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visor.name = "Visor";
            visor.transform.SetParent(bot.transform, false);
            visor.transform.localPosition = new Vector3(0f, 0.45f, 0.28f);
            visor.transform.localScale = new Vector3(0.42f, 0.16f, 0.18f);
            Destroy(visor.GetComponent<Collider>());
            visor.GetComponent<Renderer>().sharedMaterial = RuntimeMaterials.AccentIntel;

            var cc = bot.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.38f;
            cc.center = Vector3.zero;
            cc.slopeLimit = 45f;
            cc.stepOffset = 0.3f;

            bot.AddComponent<BotController>().Setup(waypoints, player, match, color);
        }
    }
}
