namespace Game.Arena.Domain.Interactions.Contracts
{
    public interface IInteractionRequest
    {
        InteractionCorrelation Correlation { get; }
        InteractionKind Kind { get; }
    }
}
