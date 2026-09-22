using Cipher.Gameplay.Interactables;
using UnityEngine;

namespace Cipher.Gameplay.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("Move")]
        [SerializeField] float walkSpeed = 5.5f;
        [SerializeField] float sprintSpeed = 8.5f;
        [SerializeField] float jumpHeight = 1.2f;
        [SerializeField] float gravity = -22f;
        [SerializeField] float interactRange = 2.6f;

        [Header("Look")]
        [SerializeField] Transform cameraPivot;
        [SerializeField] float mouseSensitivity = 1.8f;
        [SerializeField] float minPitch = -85f;
        [SerializeField] float maxPitch = 85f;

        CharacterController _controller;
        PlayerHealth _health;
        float _verticalVelocity;
        float _pitch;
        bool _locked = true;

        public Transform CameraPivot => cameraPivot;
        public bool InputLocked => !_locked || (_health != null && !_health.IsAlive);

        public void BindCamera(Transform pivot)
        {
            cameraPivot = pivot;
        }

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _health = GetComponent<PlayerHealth>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _locked = !_locked;
                Cursor.lockState = _locked ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !_locked;
            }

            if (InputLocked) return;

            Look();
            Move();
            TryInteract();
        }

        void Look()
        {
            float mx = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            float my = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
            transform.Rotate(0f, mx, 0f);
            _pitch = Mathf.Clamp(_pitch - my, minPitch, maxPitch);
            if (cameraPivot != null)
            {
                cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
            }
        }

        void Move()
        {
            bool grounded = _controller.isGrounded;
            if (grounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }

            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");
            Vector3 input = new Vector3(x, 0f, z);
            if (input.sqrMagnitude > 1f) input.Normalize();

            float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
            Vector3 world = transform.TransformDirection(input) * speed;

            if (grounded && Input.GetButtonDown("Jump"))
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            _verticalVelocity += gravity * Time.deltaTime;
            world.y = _verticalVelocity;
            _controller.Move(world * Time.deltaTime);
        }

        void TryInteract()
        {
            if (!Input.GetKeyDown(KeyCode.E) && !Input.GetKeyDown(KeyCode.F)) return;
            if (cameraPivot == null) return;

            Ray ray = new Ray(cameraPivot.position, cameraPivot.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, interactRange)) return;

            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            interactable?.Interact(gameObject);
        }
    }
}
