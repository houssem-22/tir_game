using Cipher.Clues;
using Cipher.Gameplay.Combat;
using Cipher.Gameplay.Player;
using UnityEngine;

namespace Cipher.AI
{
    public enum BotMode
    {
        Patrol,
        Combat,
        SeekClue
    }

    /// <summary>
    /// MVP1 bot: patrol waypoints, shoot player in range, optionally pursue current clue node.
    /// </summary>
    public sealed class BotController : MonoBehaviour, IDamageable
    {
        [SerializeField] float maxHealth = 80f;
        [SerializeField] float moveSpeed = 3.4f;
        [SerializeField] float sightRange = 22f;
        [SerializeField] float attackRange = 16f;
        [SerializeField] float damage = 8f;
        [SerializeField] float fireInterval = 0.55f;
        [SerializeField] float clueInterestChance = 0.35f;

        Vector3[] _waypoints;
        int _wp;
        float _health;
        float _nextFire;
        BotMode _mode = BotMode.Patrol;
        Transform _player;
        MatchManager _match;
        bool _dead;
        Renderer _renderer;
        Color _aliveColor;

        public bool IsAlive => !_dead;

        public void Setup(Vector3[] waypoints, Transform player, MatchManager match, Color color, bool preferSeekClue = false)
        {
            _waypoints = waypoints;
            _player = player;
            _match = match;
            _aliveColor = color;
            _health = maxHealth;
            _renderer = GetComponentInChildren<Renderer>();
            if (_renderer != null) _renderer.material.color = color;
            _mode = preferSeekClue || Random.value < clueInterestChance ? BotMode.SeekClue : BotMode.Patrol;
        }

        void Update()
        {
            if (_dead) return;
            if (_player == null)
            {
                var ph = FindFirstObjectByType<PlayerHealth>();
                if (ph != null) _player = ph.transform;
            }

            float distToPlayer = _player != null ? Vector3.Distance(transform.position, _player.position) : float.MaxValue;
            if (distToPlayer <= sightRange)
            {
                _mode = BotMode.Combat;
            }
            else if (_mode == BotMode.Combat)
            {
                _mode = Random.value < clueInterestChance ? BotMode.SeekClue : BotMode.Patrol;
            }

            switch (_mode)
            {
                case BotMode.Combat:
                    CombatTick(distToPlayer);
                    break;
                case BotMode.SeekClue:
                    SeekClueTick();
                    break;
                default:
                    PatrolTick();
                    break;
            }
        }

        void PatrolTick()
        {
            if (_waypoints == null || _waypoints.Length == 0) return;
            Vector3 target = _waypoints[_wp];
            MoveTowards(target);
            if (Vector3.Distance(Flat(transform.position), Flat(target)) < 0.6f)
            {
                _wp = (_wp + 1) % _waypoints.Length;
            }
        }

        void SeekClueTick()
        {
            Vector3 target = transform.position;
            if (_match != null && _match.Mission != null && _match.CluesFound < 3)
            {
                var step = _match.Mission.steps[Mathf.Clamp(_match.CluesFound, 0, 2)];
                var node = _match.Database != null ? _match.Database.Find(step.targetNodeId) : null;
                if (node != null) target = node.worldPosition;
            }
            else if (_waypoints != null && _waypoints.Length > 0)
            {
                target = _waypoints[_wp];
            }

            MoveTowards(target);
        }

        void CombatTick(float dist)
        {
            if (_player == null) return;
            Vector3 look = _player.position - transform.position;
            look.y = 0f;
            if (look.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), Time.deltaTime * 8f);
            }

            if (dist > attackRange * 0.65f)
            {
                MoveTowards(_player.position);
            }

            if (dist <= attackRange && Time.time >= _nextFire)
            {
                _nextFire = Time.time + fireInterval;
                var health = _player.GetComponent<PlayerHealth>();
                if (health != null && health.IsAlive)
                {
                    // Simple line-of-sight
                    Vector3 origin = transform.position + Vector3.up * 1.4f;
                    Vector3 to = _player.position + Vector3.up * 1.2f;
                    if (Physics.Raycast(origin, (to - origin).normalized, out RaycastHit hit, attackRange))
                    {
                        if (hit.collider.GetComponentInParent<PlayerHealth>() != null)
                        {
                            health.ApplyDamage(damage, gameObject);
                        }
                    }
                }
            }
        }

        void MoveTowards(Vector3 target)
        {
            Vector3 flat = Flat(target);
            Vector3 pos = Flat(transform.position);
            Vector3 dir = flat - pos;
            if (dir.sqrMagnitude < 0.0001f) return;
            dir.Normalize();
            transform.position += dir * moveSpeed * Time.deltaTime;
            transform.position = new Vector3(transform.position.x, 1f, transform.position.z);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 6f);
        }

        static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

        public void ApplyDamage(float amount, GameObject source)
        {
            if (_dead) return;
            _health -= amount;
            if (_health <= 0f)
            {
                _dead = true;
                if (_renderer != null) _renderer.material.color = new Color(0.2f, 0.2f, 0.2f);
                Destroy(gameObject, 2.5f);
            }
        }
    }
}
