using Game.Arena.Application.Input;

namespace Game.Arena.Application.Ports
{
    public interface IPlayerInputSource
    {
        PlayerFrameInput Read();
    }
}
