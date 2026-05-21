namespace Assets.Scripts.Gameplay.Services.UpdateService
{
    public interface IUpdaterService
    {
        void FixedUpdate(float fixedDeltaTime);
        void LateUpdate(float deltaTime);
        void Update(float deltaTime);
    }
}