namespace EvolutionaryArchitecture.Fitnet.Contracts.SignContract;

using Data.Database;
using Events;
using Shared.Events.EventBus;
using Shared.SystemClock;

internal static class SignContractEndpoint
{
    internal static void MapSignContract(this IEndpointRouteBuilder app)
    {
        app.MapPatch(ContractsApiPaths.Sign, async (Guid id, SignContractRequest request,
            ContractsPersistence persistence, IEventBus bus, ISystemClock systemClock, CancellationToken cancellationToken) =>
        {
            var contract =
                await persistence.Contracts.FindAsync(new object?[] { id }, cancellationToken);
            if (contract is null)
                return Results.NotFound();

            contract.Sign(request.SignedAt, systemClock);
            await persistence.SaveChangesAsync(cancellationToken);
            
            var @event = ContractSignedEvent.Create(contract.Id, contract.CustomerId, contract.SignedAt!.Value, contract.ExpiringAt!.Value);
            await bus.PublishAsync(@event, cancellationToken);
            
            return Results.NoContent();
        });
    }
}