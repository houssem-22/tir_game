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
    /// Phase 2 BLACKSITE bootstrap: enterable modular interiors + PBR textures.
    /// Quaternius FBX drop-ins under Assets/Art/**/Quaternius are preferred when present.
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
            var player = BuildPlayer(new Vector3(-28f, 0.1f, -4.5f));
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
                Debug.Log("[CIPHER] Path: enter Hospital door (north) → Camera room → Building 04 → Room 17 Terminal → Bunker vault code.");
            }
        }

        static void BuildLighting()
        {
            if (FindFirstObjectByType<Light>() == null)
            {
                var lightGo = new GameObject("Directional Light");
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = new Color(0.55f, 0.68f, 0.9f);
                light.intensity = 0.85f;
                light.shadows = LightShadows.Soft;
                lightGo.transform.rotation = Quaternion.Euler(38f, -40f, 0f);
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.18f, 0.22f, 0.28f);
            RenderSettings.ambientEquatorColor = new Color(0.12f, 0.12f, 0.11f);
            RenderSettings.ambientGroundColor = new Color(0.06f, 0.05f, 0.04f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.12f, 0.14f, 0.16f);
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.018f;
            RenderSettings.subtractiveShadowColor = new Color(0.15f, 0.16f, 0.2f);
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
            pad.transform.localScale = new Vector3(0.7f, 1.1f, 0.35f);
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
            controller.stepOffset = 0.35f;

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
            // Phase 2: 5 bots — mix of patrol / seek-clue
            SpawnBot("Bot_Hospital_Guard", new Vector3(-24f, 1f, 0f), new[]
            {
                new Vector3(-24f, 1f, 0f), new Vector3(-32f, 1f, 2f), new Vector3(-26f, 1f, 8f)
            }, player, match, new Color(0.45f, 0.2f, 0.18f), seeker: false);

            SpawnBot("Bot_Hospital_Seeker", new Vector3(-28f, 1f, 12f), new[]
            {
                new Vector3(-28f, 1f, 12f), new Vector3(-28f, 1f, 16f)
            }, player, match, new Color(0.5f, 0.25f, 0.15f), seeker: true);

            SpawnBot("Bot_Industrial_A", new Vector3(-2f, 1f, 14f), new[]
            {
                new Vector3(-2f, 1f, 14f), new Vector3(6f, 1f, 18f), new Vector3(0f, 1f, 24f)
            }, player, match, new Color(0.42f, 0.22f, 0.18f), seeker: false);

            SpawnBot("Bot_Industrial_Seeker", new Vector3(4f, 1f, 20f), new[]
            {
                new Vector3(4f, 1f, 20f), new Vector3(2f, 1f, 26f)
            }, player, match, new Color(0.48f, 0.2f, 0.16f), seeker: true);

            SpawnBot("Bot_Bunker_Defender", new Vector3(30f, 1f, -4f), new[]
            {
                new Vector3(30f, 1f, -4f), new Vector3(36f, 1f, -6f), new Vector3(32f, 1f, -12f)
            }, player, match, new Color(0.38f, 0.16f, 0.14f), seeker: false);
        }

        static void SpawnBot(string name, Vector3 pos, Vector3[] waypoints, Transform player, MatchManager match, Color color, bool seeker)
        {
            var bot = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bot.name = name;
            bot.transform.position = pos;
            var body = bot.GetComponent<Renderer>();
            body.sharedMaterial = RuntimeMaterials.Rust;
            body.material.color = color;

            var visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visor.name = "Visor";
            visor.transform.SetParent(bot.transform, false);
            visor.transform.localPosition = new Vector3(0f, 0.45f, 0.28f);
            visor.transform.localScale = new Vector3(0.42f, 0.16f, 0.18f);
            Object.Destroy(visor.GetComponent<Collider>());
            visor.GetComponent<Renderer>().sharedMaterial = RuntimeMaterials.AccentIntel;

            var ai = bot.AddComponent<BotController>();
            ai.Setup(waypoints, player, match, color, preferSeekClue: seeker);
        }
    }
}
