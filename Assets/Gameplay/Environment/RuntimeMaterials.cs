using UnityEngine;

namespace Cipher.Gameplay.Environment
{
    /// <summary>
    /// Runtime materials using Poly Haven CC0 albedo textures from Resources/Textures.
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

        public static Material Concrete { get { Ready(); return _concrete; } }
        public static Material Metal { get { Ready(); return _metal; } }
        public static Material Asphalt { get { Ready(); return _asphalt; } }
        public static Material Plaster { get { Ready(); return _plaster; } }
        public static Material Floor { get { Ready(); return _floor; } }
        public static Material Rust { get { Ready(); return _rust; } }
        public static Material AccentWarn { get { Ready(); return _accentWarn; } }
        public static Material AccentIntel { get { Ready(); return _accentIntel; } }

        static void Ready()
        {
            if (_ready) return;
            _concrete = Make("concrete_diff", new Color(0.45f, 0.44f, 0.42f), 0.05f, 0.25f, 2.5f);
            _metal = Make("metal_diff", new Color(0.35f, 0.37f, 0.4f), 0.75f, 0.55f, 1.5f);
            _asphalt = Make("asphalt_diff", new Color(0.18f, 0.18f, 0.18f), 0.02f, 0.15f, 4f);
            _plaster = Make("plaster_diff", new Color(0.62f, 0.6f, 0.55f), 0.02f, 0.2f, 2f);
            _floor = Make("floor_diff", new Color(0.4f, 0.39f, 0.37f), 0.05f, 0.28f, 3f);
            _rust = Make("rust_diff", new Color(0.45f, 0.28f, 0.18f), 0.35f, 0.3f, 1.5f);
            _accentWarn = Solid(new Color(0.78f, 0.58f, 0.12f));
            _accentIntel = Solid(new Color(0.2f, 0.72f, 0.88f));
            _ready = true;
        }

        static Material Make(string textureName, Color fallback, float metallic, float gloss, float tiling)
        {
            var shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Diffuse");
            var mat = new Material(shader);
            var tex = Resources.Load<Texture2D>("Textures/" + textureName);
            if (tex != null)
            {
                mat.mainTexture = tex;
                mat.color = Color.white;
                mat.mainTextureScale = new Vector2(tiling, tiling);
            }
            else
            {
                mat.color = fallback;
            }

            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", gloss);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", gloss);
            return mat;
        }

        static Material Solid(Color c)
        {
            var shader = Shader.Find("Standard") ?? Shader.Find("Diffuse");
            var mat = new Material(shader) { color = c };
            return mat;
        }
    }
}
