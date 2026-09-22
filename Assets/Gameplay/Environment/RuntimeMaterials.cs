using UnityEngine;

namespace Cipher.Gameplay.Environment
{
    /// <summary>
    /// Runtime materials: Poly Haven CC0 albedo from Resources/Textures when present,
    /// otherwise generated detail maps so the graybox stays readable.
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
            _concrete = Make("concrete_diff", new Color(0.42f, 0.41f, 0.38f), 0.04f, 0.22f, 3.2f, TextureKind.Concrete);
            _metal = Make("metal_diff", new Color(0.28f, 0.3f, 0.33f), 0.82f, 0.48f, 2.2f, TextureKind.Metal);
            _asphalt = Make("asphalt_diff", new Color(0.14f, 0.14f, 0.145f), 0.02f, 0.12f, 6f, TextureKind.Asphalt);
            _plaster = Make("plaster_diff", new Color(0.58f, 0.56f, 0.5f), 0.02f, 0.18f, 2.4f, TextureKind.Plaster);
            _floor = Make("floor_diff", new Color(0.32f, 0.3f, 0.27f), 0.06f, 0.3f, 3.5f, TextureKind.Floor);
            _rust = Make("rust_diff", new Color(0.42f, 0.24f, 0.14f), 0.4f, 0.22f, 2f, TextureKind.Rust);
            _accentWarn = Emissive(new Color(0.92f, 0.62f, 0.12f), new Color(1.4f, 0.75f, 0.08f));
            _accentIntel = Emissive(new Color(0.12f, 0.72f, 0.82f), new Color(0.15f, 1.1f, 1.4f));
            _accentMuted = Solid(new Color(0.18f, 0.2f, 0.18f), 0.15f, 0.2f);
            _glass = Solid(new Color(0.35f, 0.42f, 0.45f, 0.35f), 0.1f, 0.85f);
            _ready = true;
        }

        enum TextureKind
        {
            Concrete,
            Metal,
            Asphalt,
            Plaster,
            Floor,
            Rust
        }

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
            var shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Diffuse");
            var mat = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            return mat;
        }

        static void SetMetalGloss(Material mat, float metallic, float gloss)
        {
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", gloss);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", gloss);
        }

        static Texture2D Generate(TextureKind kind, Color baseColor)
        {
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGB24, true)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave,
                name = "Gen_" + kind
            };

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float n = Hash(x, y, (int)kind);
                    float n2 = Hash(x * 3, y * 2, 17 + (int)kind);
                    Color c = baseColor;
                    switch (kind)
                    {
                        case TextureKind.Concrete:
                            c *= 0.88f + n * 0.18f;
                            if (x % 32 < 1 || y % 32 < 1) c *= 0.82f;
                            break;
                        case TextureKind.Asphalt:
                            c *= 0.85f + n * 0.22f;
                            if ((x + y) % 64 < 2) c = Color.Lerp(c, new Color(0.55f, 0.5f, 0.2f), 0.25f);
                            break;
                        case TextureKind.Plaster:
                            c *= 0.92f + n * 0.1f;
                            if (n2 > 0.94f) c *= 0.9f;
                            break;
                        case TextureKind.Metal:
                            float stripe = (x % 8 < 1) ? 0.12f : 0f;
                            c *= 0.9f + n * 0.15f - stripe;
                            break;
                        case TextureKind.Floor:
                            bool tile = ((x / 16) + (y / 16)) % 2 == 0;
                            c *= tile ? 0.92f + n * 0.08f : 0.8f + n * 0.08f;
                            if (x % 16 < 1 || y % 16 < 1) c *= 0.7f;
                            break;
                        case TextureKind.Rust:
                            c = Color.Lerp(c, new Color(0.25f, 0.18f, 0.12f), n2 * 0.45f);
                            c *= 0.85f + n * 0.25f;
                            break;
                    }

                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply(true, true);
            return tex;
        }

        static float Hash(int x, int y, int seed)
        {
            unchecked
            {
                int n = x * 374761 + y * 668265 + seed * 127;
                n = (n << 13) ^ n;
                return (1f - ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824f) * 0.5f + 0.5f;
            }
        }
    }
}
