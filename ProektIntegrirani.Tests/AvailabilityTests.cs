using ProektIntegrirani.Domain.Enums;
using ProektIntegrirani.Domain.Models;
using ProektIntegrirani.Service.Logic;

namespace ProektIntegrirani.Tests;

public class AvailabilityTests
{
    [Fact]
    public void AvailablePlayer_IsAlwaysAvailable()
    {
        var player = new Player { Status = PlayerStatus.Available };

        Assert.Equal(1, PoissonPredictionModel.Availability(player, 0));
        Assert.Equal(1, PoissonPredictionModel.Availability(player, 3));
    }

    [Fact]
    public void ChanceOfPlaying_OverridesStatusForNextGameweek()
    {
        var player = new Player { Status = PlayerStatus.Doubtful, ChanceOfPlaying = 75 };

        Assert.Equal(0.75, PoissonPredictionModel.Availability(player, 0), 6);
    }

    [Fact]
    public void InjuredPlayer_RecoversOverTheFollowingGameweeks()
    {
        var player = new Player { Status = PlayerStatus.Injured, ChanceOfPlaying = 0 };

        var now = PoissonPredictionModel.Availability(player, 0);
        var later = PoissonPredictionModel.Availability(player, 3);

        Assert.Equal(0, now);
        Assert.True(later > now);
    }

    [Fact]
    public void UnavailablePlayer_NeverPlays()
    {
        var player = new Player { Status = PlayerStatus.Unavailable, ChanceOfPlaying = 100 };

        Assert.Equal(0, PoissonPredictionModel.Availability(player, 0));
        Assert.Equal(0, PoissonPredictionModel.Availability(player, 5));
    }
}
