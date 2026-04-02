namespace Assets.Scripts.Gameplay.PlayerFeature.Components
{
    public class IdentifierService : IIdentifierService<int>
    {
        private int _nextId = 1;

        public int GetNextId()
        {
            return _nextId++;
        }
    }
}