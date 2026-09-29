using ProektIntegrirani.Domain.Dto;

namespace ProektIntegrirani.Service.Interface;

public interface IPredictionService
{
    public const int DefaultHorizon = 6;

    // Runs the Poisson model for the next `horizon` gameweeks and replaces the stored predictions.
    Task<PredictionRunResultDto> RecalculateAsync(int horizon = DefaultHorizon,
        CancellationToken cancellationToken = default);
}
