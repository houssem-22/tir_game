using Cipher.Clues;
using Cipher.Gameplay.Combat;
using Cipher.Gameplay.Player;
using UnityEngine;

namespace Cipher.AI
{
    public enum BotMode
    {
        Patrol,
        Combat
    }

    /// <summary>
    /// Phase 3 bot: CharacterController locomotion, combat only with line of sight.
    /// Never walks onto clue objects or through walls.
    /// </summary>
    public sealed class BotController : MonoBehaviour, IDamageable
    {
        [SerializeField] float maxHealth = 80f;
        [SerializeField] float moveSpeed = 2.8f;
        [SerializeField] float sightRange = 16f;
        [SerializeField] float attackRange = 12f;
        [SerializeField] float stopDistance = 4.5f;
        [SerializeField] float damage = 7f;
        [SerializeField] float fireInterval = 0.85f;

        Vector3[] _waypoints;
        int _wp;
        float _health;
        float _nextFire;
        BotMode _mode = BotMode.Patrol;
        Transform _player;
        bool _dead;
        Renderer _renderer;
        CharacterController _cc;
        float _vertical;

        public bool IsAlive => !_dead;

        public void Setup(Vector3[] waypoints, Transform player, MatchManager match, Color color, bool preferSeekClue = false)
        {
            _waypoints = waypoints;
            _player = player;
            _health = maxHealth;
            _renderer = GetComponentInChildren<Renderer>();
            if (_renderer != null) _renderer.material.color = color;
            _mode = BotMode.Patrol;
            _ = match;
            _ = preferSeekClue;
            _cc = GetComponent<CharacterController>();
            if (_cc == null)
            {
                var capsule = GetComponent<CapsuleCollider>();
                if (capsule != null) Destroy(capsule);
                _cc = gameObject.AddComponent<CharacterController>();
                _cc.height = 2f;
                _cc.radius = 0.38f;
                _cc.center = Vector3.zero;
                _cc.slopeLimit = 45f;
                _cc.stepOffset = 0.3f;
            }
        }

        void Update()
        {
            if (_dead) return;
            if (_player == null)
            {
                var ph = FindFirstObjectByType<PlayerHealth>();
                if (ph != null) _player = ph.transform;
            }

            bool seesPlayer = HasLineOfSight();
            _mode = seesPlayer ? BotMode.Combat : BotMode.Patrol;

            if (_mode == BotMode.Combat) CombatTick();
            else PatrolTick();
        }

        bool HasLineOfSight()
        {
            if (_player == null) return false;
            Vector3 origin = transform.position + Vector3.up * 0.8f;
            Vector3 to = _player.position + Vector3.up * 1.2f;
            Vector3 delta = to - origin;
            if (delta.sqrMagnitude > sightRange * sightRange) return false;
            if (!Physics.Raycast(origin, delta.normalized, out RaycastHit hit, sightRange, ~0, QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            return hit.collider.GetComponentInParent<PlayerHealth>() != null;
        }

        void PatrolTick()
        {
            if (_waypoints == null || _waypoints.Length == 0) return;
            Vector3 target = _waypoints[_wp];
            MoveTowards(target, 0.7f);
            Vector3 flat = transform.position;
            flat.y = 0f;
            target.y = 0f;
            if (Vector3.Distance(flat, target) < 0.7f)
            {
                _wp = (_wp + 1) % _waypoints.Length;
            }
        }

        void CombatTick()
        {
            if (_player == null) return;
            Vector3 look = _player.position - transform.position;
            look.y = 0f;
            if (look.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), Time.deltaTime * 8f);
            }

            float dist = Vector3.Distance(transform.position, _player.position);
            if (dist > stopDistance)
            {
                MoveTowards(_player.position, stopDistance * 0.85f);
            }

            if (dist <= attackRange && Time.time >= _nextFire && HasLineOfSight())
            {
                _nextFire = Time.time + fireInterval;
                var health = _player.GetComponent<PlayerHealth>();
                if (health != null && health.IsAlive)
                {
                    health.ApplyDamage(damage, gameObject);
                }
            }
        }

        void MoveTowards(Vector3 target, float hold)
        {
            Vector3 dir = target - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < hold * hold) dir = Vector3.zero;
            else dir.Normalize();

            if (dir.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 6f);
            }

            Vector3 motion = dir * moveSpeed;
            if (_cc != null && _cc.enabled)
            {
                if (_cc.isGrounded && _vertical < 0f) _vertical = -2f;
                _vertical += -22f * Time.deltaTime;
                motion.y = _vertical;
                _cc.Move(motion * Time.deltaTime);
            }
            else
            {
                transform.position += dir * moveSpeed * Time.deltaTime;
            }
        }

        public void ApplyDamage(float amount, GameObject source)
        {
            if (_dead) return;
            _health -= amount;
            if (_health <= 0f)
            {
                _dead = true;
                if (_cc != null) _cc.enabled = false;
                var col = GetComponent<Collider>();
                if (col != null) col.enabled = false;
                if (_renderer != null) _renderer.material.color = new Color(0.2f, 0.2f, 0.2f);
                Destroy(gameObject, 2.5f);
            }
        }
    }
}
