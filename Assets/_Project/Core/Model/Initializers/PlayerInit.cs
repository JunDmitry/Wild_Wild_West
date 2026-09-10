using Game.Core.Model.Configs;
using Game.Core.Model.Entities;
using Game.Core.Model.States;

namespace Game.Core.Model.Initializers
{
    public static class PlayerInit
    {
        public static PlayerState FromConfig(in GameConfig config, EntityId playerId)
        {
            return new PlayerState(
                playerId,
                config.Player.StartPosition,
                config.Player.MaxHealth,
                config.Player.MaxHealth,
                Enums.WeaponKind.Ranged,
                config.Player.StartRangedReadyTime,
                config.Player.StartMeleeReadyTime);
        }
    }
}
