namespace Game.Core.Model.Results
{
    public readonly struct SimulationResult
    {
        public bool PlayerAttackIssued { get; }
        public bool PlayerDamageReceived { get; }
        public bool PlayerDefeated { get; }
        public bool EnemySpawned { get; }
        public bool EnemyDamaged { get; }
        public bool EnemyDefeated { get; }
        public bool WavePhaseChanged { get; }
        public bool WaveStarted { get; }
        public bool GamePhaseChanged { get; }
        public bool RestartRequested { get; }
    }
}
