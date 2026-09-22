using Cipher.Gameplay.Combat;
using Cipher.Gameplay.Player;
using UnityEngine;

namespace Cipher.Gameplay.Weapons
{
    public sealed class WeaponController : MonoBehaviour
    {
        [SerializeField] Transform muzzle;
        [SerializeField] LayerMask hitMask = ~0;

        WeaponDefinition[] _weapons;
        int[] _mag;
        int[] _reserve;
        int _index;
        float _nextFireTime;
        float _reloadEndTime;
        bool _reloading;
        Renderer _viewRenderer;
        PlayerController _player;

        public WeaponDefinition Current => _weapons[_index];
        public int Magazine => _mag[_index];
        public int Reserve => _reserve[_index];
        public bool IsReloading => _reloading;
        public int WeaponCount => _weapons.Length;

        public void Initialize(Transform muzzlePoint, Renderer viewModelRenderer)
        {
            muzzle = muzzlePoint;
            _viewRenderer = viewModelRenderer;
            _player = GetComponentInParent<PlayerController>();
            _weapons = WeaponCatalog.CreateDefaultLoadout();
            _mag = new int[_weapons.Length];
            _reserve = new int[_weapons.Length];
            for (int i = 0; i < _weapons.Length; i++)
            {
                _mag[i] = _weapons[i].magazineSize;
                _reserve[i] = _weapons[i].reserveAmmo;
            }

            ApplyViewTint();
        }

        void Update()
        {
            if (_weapons == null || _weapons.Length == 0) return;
            if (_player != null && _player.InputLocked) return;

            HandleSwap();
            HandleReload();
            HandleFire();
        }

        void HandleSwap()
        {
            for (int i = 0; i < Mathf.Min(5, _weapons.Length); i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SelectWeapon(i);
                    return;
                }
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0.05f) SelectWeapon((_index + 1) % _weapons.Length);
            if (scroll < -0.05f) SelectWeapon((_index - 1 + _weapons.Length) % _weapons.Length);
        }

        void SelectWeapon(int index)
        {
            if (index < 0 || index >= _weapons.Length) return;
            _index = index;
            _reloading = false;
            ApplyViewTint();
        }

        void HandleReload()
        {
            if (_reloading)
            {
                if (Time.time >= _reloadEndTime)
                {
                    FinishReload();
                }
                return;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                BeginReload();
            }
        }

        void BeginReload()
        {
            var w = Current;
            if (_mag[_index] >= w.magazineSize) return;
            if (_reserve[_index] <= 0) return;
            _reloading = true;
            _reloadEndTime = Time.time + w.reloadSeconds;
        }

        void FinishReload()
        {
            var w = Current;
            int need = w.magazineSize - _mag[_index];
            int take = Mathf.Min(need, _reserve[_index]);
            _mag[_index] += take;
            _reserve[_index] -= take;
            _reloading = false;
        }

        void HandleFire()
        {
            if (_reloading) return;
            var w = Current;
            bool wantsFire = w.automatic ? Input.GetButton("Fire1") : Input.GetButtonDown("Fire1");
            if (!wantsFire) return;
            if (Time.time < _nextFireTime) return;
            if (_mag[_index] <= 0)
            {
                BeginReload();
                return;
            }

            _mag[_index]--;
            _nextFireTime = Time.time + (1f / Mathf.Max(0.01f, w.fireRate));
            FireHitscan(w);
        }

        void FireHitscan(WeaponDefinition w)
        {
            if (muzzle == null) return;

            for (int p = 0; p < w.pellets; p++)
            {
                Vector3 dir = ApplySpread(muzzle.forward, w.spreadDegrees);
                if (Physics.Raycast(muzzle.position, dir, out RaycastHit hit, w.range, hitMask, QueryTriggerInteraction.Ignore))
                {
                    var damageable = hit.collider.GetComponentInParent<IDamageable>();
                    damageable?.ApplyDamage(w.damage, gameObject);
                    Debug.DrawLine(muzzle.position, hit.point, Color.yellow, 0.08f);
                }
                else
                {
                    Debug.DrawRay(muzzle.position, dir * w.range, Color.gray, 0.05f);
                }
            }
        }

        static Vector3 ApplySpread(Vector3 forward, float degrees)
        {
            if (degrees <= 0.001f) return forward;
            return Quaternion.Euler(
                Random.Range(-degrees, degrees),
                Random.Range(-degrees, degrees),
                0f) * forward;
        }

        void ApplyViewTint()
        {
            if (_viewRenderer == null) return;
            _viewRenderer.material.color = Current.viewTint;
        }
    }
}
