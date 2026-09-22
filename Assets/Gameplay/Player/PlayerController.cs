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
        [SerializeField] float interactRange = 3.4f;

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
        string _interactPrompt;

        public Transform CameraPivot => cameraPivot;
        public string InteractPrompt => _interactPrompt;
        public bool InputLocked => !_locked || Cipher.Gameplay.GameplayUi.IsModal || (_health != null && !_health.IsAlive);

        public void BindCamera(Transform pivot)
        {
            cameraPivot = pivot;
        }

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (_health == null) _health = GetComponent<PlayerHealth>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            if (_health == null) _health = GetComponent<PlayerHealth>();
            if (!Cipher.Gameplay.GameplayUi.IsModal && Input.GetKeyDown(KeyCode.Escape))
            {
                _locked = !_locked;
                Cursor.lockState = _locked ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !_locked;
            }

            ScanInteractable();
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

        void ScanInteractable()
        {
            _interactPrompt = null;
            if (cameraPivot == null) return;
            Ray ray = new Ray(cameraPivot.position, cameraPivot.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, interactRange, ~0, QueryTriggerInteraction.Collide)) return;
            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null && interactable.CanInteract)
            {
                _interactPrompt = interactable.Prompt;
            }
        }

        void TryInteract()
        {
            if (!Input.GetKeyDown(KeyCode.E) && !Input.GetKeyDown(KeyCode.F)) return;
            if (cameraPivot == null) return;

            Ray ray = new Ray(cameraPivot.position, cameraPivot.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, interactRange, ~0, QueryTriggerInteraction.Collide)) return;

            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            interactable?.Interact(gameObject);
        }
    }
}
