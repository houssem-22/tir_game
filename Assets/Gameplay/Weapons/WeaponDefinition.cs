using UnityEngine;

namespace Cipher.Gameplay.Weapons
{
    public enum WeaponId
    {
        Pistol = 0,
        Smg = 1,
        AssaultRifle = 2,
        Shotgun = 3,
        Sniper = 4
    }

    [System.Serializable]
    public sealed class WeaponDefinition
    {
        public WeaponId id;
        public string displayName;
        public float damage = 20f;
        public float fireRate = 6f;
        public float range = 80f;
        public int magazineSize = 12;
        public int reserveAmmo = 60;
        public float reloadSeconds = 1.6f;
        public float spreadDegrees = 1.5f;
        public int pellets = 1;
        public bool automatic = false;
        public Color viewTint = Color.gray;
    }

    public static class WeaponCatalog
    {
        public static WeaponDefinition[] CreateDefaultLoadout()
        {
            return new[]
            {
                new WeaponDefinition
                {
                    id = WeaponId.Pistol,
                    displayName = "Pistol",
                    damage = 24f,
                    fireRate = 4.5f,
                    range = 55f,
                    magazineSize = 12,
                    reserveAmmo = 48,
                    reloadSeconds = 1.4f,
                    spreadDegrees = 1.2f,
                    pellets = 1,
                    automatic = false,
                    viewTint = new Color(0.55f, 0.55f, 0.5f)
                },
                new WeaponDefinition
                {
                    id = WeaponId.Smg,
                    displayName = "SMG",
                    damage = 14f,
                    fireRate = 11f,
                    range = 45f,
                    magazineSize = 30,
                    reserveAmmo = 120,
                    reloadSeconds = 1.8f,
                    spreadDegrees = 2.4f,
                    pellets = 1,
                    automatic = true,
                    viewTint = new Color(0.45f, 0.5f, 0.55f)
                },
                new WeaponDefinition
                {
                    id = WeaponId.AssaultRifle,
                    displayName = "Assault Rifle",
                    damage = 22f,
                    fireRate = 8f,
                    range = 90f,
                    magazineSize = 30,
                    reserveAmmo = 120,
                    reloadSeconds = 2.1f,
                    spreadDegrees = 1.6f,
                    pellets = 1,
                    automatic = true,
                    viewTint = new Color(0.35f, 0.4f, 0.35f)
                },
                new WeaponDefinition
                {
                    id = WeaponId.Shotgun,
                    displayName = "Shotgun",
                    damage = 12f,
                    fireRate = 1.2f,
                    range = 28f,
                    magazineSize = 6,
                    reserveAmmo = 30,
                    reloadSeconds = 2.6f,
                    spreadDegrees = 6.5f,
                    pellets = 8,
                    automatic = false,
                    viewTint = new Color(0.4f, 0.32f, 0.28f)
                },
                new WeaponDefinition
                {
                    id = WeaponId.Sniper,
                    displayName = "Sniper",
                    damage = 75f,
                    fireRate = 0.9f,
                    range = 160f,
                    magazineSize = 5,
                    reserveAmmo = 25,
                    reloadSeconds = 2.8f,
                    spreadDegrees = 0.15f,
                    pellets = 1,
                    automatic = false,
                    viewTint = new Color(0.25f, 0.28f, 0.3f)
                }
            };
        }
    }
}
