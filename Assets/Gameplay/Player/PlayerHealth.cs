using System;
using Cipher.Gameplay.Combat;
using UnityEngine;

namespace Cipher.Gameplay.Player
{
    public sealed class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] float maxHealth = 100f;
        [SerializeField] float respawnDelay = 2.5f;

        float _health;
        bool _dead;
        Vector3 _spawnPosition;
        Quaternion _spawnRotation;
        CharacterController _controller;

        float _protectUntil;

        public float MaxHealth => maxHealth;
        public float CurrentHealth => _health;
        public bool IsAlive => !_dead;
        public event Action Died;
        public event Action<float, float> HealthChanged;

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _health = maxHealth;
            _spawnPosition = transform.position;
            _spawnRotation = transform.rotation;
            _protectUntil = Time.time + 3f;
        }

        public void SetSpawn(Vector3 position, Quaternion rotation)
        {
            _spawnPosition = position;
            _spawnRotation = rotation;
            _protectUntil = Time.time + 3f;
        }

        public void ApplyDamage(float amount, GameObject source)
        {
            if (_dead || amount <= 0f) return;
            if (Time.time < _protectUntil) return;

            _health = Mathf.Max(0f, _health - amount);
            HealthChanged?.Invoke(_health, maxHealth);

            if (_health <= 0f)
            {
                Die();
            }
        }

        public void HealFull()
        {
            _health = maxHealth;
            _dead = false;
            HealthChanged?.Invoke(_health, maxHealth);
        }

        void Die()
        {
            if (_dead) return;
            _dead = true;
            Died?.Invoke();
            CancelInvoke(nameof(Respawn));
            Invoke(nameof(Respawn), respawnDelay);
        }

        void Respawn()
        {
            if (_controller != null)
            {
                _controller.enabled = false;
            }

            transform.SetPositionAndRotation(_spawnPosition, _spawnRotation);

            if (_controller != null)
            {
                _controller.enabled = true;
            }

            HealFull();
            _protectUntil = Time.time + 3f;
        }

        void LateUpdate()
        {
            if (transform.position.y < -4f)
            {
                Respawn();
            }
        }
    }
}
