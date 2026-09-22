using UnityEngine;

namespace Cipher.Gameplay.Interactables
{
    public interface IInteractable
    {
        string Prompt { get; }
        bool CanInteract { get; }
        void Interact(GameObject actor);
    }
}
