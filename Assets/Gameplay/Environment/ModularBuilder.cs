using UnityEngine;

namespace Cipher.Gameplay.Environment
{
    /// <summary>
    /// Back-compat aliases. New layout code should call <see cref="ModularKit"/>.
    /// </summary>
    public static class ModularBuilder
    {
        public static GameObject CreateProp(string name, Vector3 worldPos, Vector3 scale, Material mat)
        {
            return ModularKit.CreateProp(name, worldPos, scale, mat);
        }

        public static GameObject CreateCylinderProp(string name, Vector3 worldPos, Vector3 scale, Material mat)
        {
            return ModularKit.CreateCylinderProp(name, worldPos, scale, mat);
        }
    }
}
