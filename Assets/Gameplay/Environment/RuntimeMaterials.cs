using UnityEngine;

namespace Cipher.Gameplay.Environment
{
    /// <summary>
    /// Runtime materials. Uses Resources/Textures when present; otherwise a tiny generated atlas.
    /// Generation is a single SetPixels pass so Awake does not hitch the editor.
    /// </summary>
    public static class RuntimeMaterials
    {
        static bool _ready;
        static Material _concrete;
        static Material _metal;
        static Material _asphalt;
        static Material _plaster;
        static Material _floor;
        static Material _rust;
        static Material _accentWarn;
        static Material _accentIntel;
        static Material _accentMuted;
        static Material _glass;

        public static Material Concrete { get { Ready(); return _concrete; } }
        public static Material Metal { get { Ready(); return _metal; } }
        public static Material Asphalt { get { Ready(); return _asphalt; } }
        public static Material Plaster { get { Ready(); return _plaster; } }
        public static Material Floor { get { Ready(); return _floor; } }
        public static Material Rust { get { Ready(); return _rust; } }
        public static Material AccentWarn { get { Ready(); return _accentWarn; } }
        public static Material AccentIntel { get { Ready(); return _accentIntel; } }
        public static Material AccentMuted { get { Ready(); return _accentMuted; } }
        public static Material Glass { get { Ready(); return _glass; } }

        static void Ready()
        {
            if (_ready) return;
            _concrete = Make("concrete_diff", new Color(0.42f, 0.41f, 0.38f), 0.04f, 0.22f, 2.4f, TextureKind.Concrete);
            _metal = Make("metal_diff", new Color(0.28f, 0.3f, 0.33f), 0.82f, 0.48f, 1.8f, TextureKind.Metal);
            _asphalt = Make("asphalt_diff", new Color(0.14f, 0.14f, 0.145f), 0.02f, 0.12f, 5f, TextureKind.Asphalt);
            _plaster = Make("plaster_diff", new Color(0.58f, 0.56f, 0.5f), 0.02f, 0.18f, 2f, TextureKind.Plaster);
            _floor = Make("floor_diff", new Color(0.32f, 0.3f, 0.27f), 0.06f, 0.3f, 2.8f, TextureKind.Floor);
            _rust = Make("rust_diff", new Color(0.42f, 0.24f, 0.14f), 0.4f, 0.22f, 1.6f, TextureKind.Rust);
            _accentWarn = Emissive(new Color(0.92f, 0.62f, 0.12f), new Color(1.4f, 0.75f, 0.08f));
            _accentIntel = Emissive(new Color(0.12f, 0.72f, 0.82f), new Color(0.15f, 1.1f, 1.4f));
            _accentMuted = Solid(new Color(0.18f, 0.2f, 0.18f), 0.15f, 0.2f);
            _glass = Solid(new Color(0.35f, 0.42f, 0.45f, 0.35f), 0.1f, 0.85f);
            _ready = true;
        }

        enum TextureKind { Concrete, Metal, Asphalt, Plaster, Floor, Rust }

        static Material Make(string textureName, Color fallback, float metallic, float gloss, float tiling, TextureKind kind)
        {
            var mat = NewLit();
            var tex = Resources.Load<Texture2D>("Textures/" + textureName);
            if (tex == null) tex = Generate(kind, fallback);
            mat.mainTexture = tex;
            mat.color = Color.white;
            mat.mainTextureScale = new Vector2(tiling, tiling);
            SetMetalGloss(mat, metallic, gloss);
            return mat;
        }

        static Material Solid(Color c, float metallic, float gloss)
        {
            var mat = NewLit();
            mat.color = c;
            SetMetalGloss(mat, metallic, gloss);
            return mat;
        }

        static Material Emissive(Color albedo, Color emission)
        {
            var mat = NewLit();
            mat.color = albedo;
            SetMetalGloss(mat, 0.15f, 0.55f);
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission);
            }
            return mat;
        }

        static Material NewLit()
        {
            var shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Diffuse");
            return new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
        }

        static void SetMetalGloss(Material mat, float metallic, float gloss)
        {
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", gloss);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", gloss);
        }

        static Texture2D Generate(TextureKind kind, Color baseColor)
        {
            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGB24, false)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave,
                name = "Gen_" + kind
            };

            var pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                int x = i % size;
                int y = i / size;
                float n = ((x * 13 + y * 7 + (int)kind * 3) & 255) / 255f;
                Color c = baseColor * (0.88f + n * 0.14f);
                if (kind == TextureKind.Floor && ((x / 8) + (y / 8)) % 2 == 0) c *= 0.9f;
                if (kind == TextureKind.Concrete && (x % 16 == 0 || y % 16 == 0)) c *= 0.85f;
                pixels[i] = c;
            }

            tex.SetPixels(pixels);
            tex.Apply(false, true);
            return tex;
        }
    }
}
