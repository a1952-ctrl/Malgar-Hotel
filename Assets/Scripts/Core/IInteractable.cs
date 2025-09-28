using MalgarHotel.Player;

namespace MalgarHotel.Core
{
    public interface IInteractable
    {
        string InteractionPrompt { get; }

        void Interact(PlayerController player);
    }
}
