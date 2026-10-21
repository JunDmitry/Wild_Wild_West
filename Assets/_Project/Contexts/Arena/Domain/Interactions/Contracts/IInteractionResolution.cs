namespace Game.Arena.Domain.Interactions.Contracts
{
    public interface IInteractionResolution
    {
        InteractionCorrelation Correlation { get; }
        InteractionKind Kind { get; }
    }
}
