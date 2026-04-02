using Assets.Scripts.Gameplay.Common.Interfaces;

namespace Assets.Scripts.Gameplay.PlayerFeature.Components
{
    public class Player : IModel
    {
        public Player(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }
}