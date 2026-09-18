using Game.Arena.Domain.Identity;

namespace Game.Arena.Application.Identity
{
    public interface IPlayerIdSource
    {
        PlayerId Allocate();
    }
}
