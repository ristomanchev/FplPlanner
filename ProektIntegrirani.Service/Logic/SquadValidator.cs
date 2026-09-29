using ProektIntegrirani.Domain.Dto;
using ProektIntegrirani.Domain.Enums;
using ProektIntegrirani.Domain.Rules;

namespace ProektIntegrirani.Service.Logic;

// Checks a complete squad against the FPL rules and collects every violation.
public static class SquadValidator
{
    public static SquadValidationResultDto Validate(IReadOnlyCollection<SquadMemberDto> squad)
    {
        var errors = new List<string>();

        ValidateSize(squad, errors);
        ValidateUniqueness(squad, errors);
        ValidateShape(squad, errors);
        ValidateClubLimit(squad, errors);
        ValidateFormation(squad, errors);
        ValidateCaptaincy(squad, errors);

        return new SquadValidationResultDto { Errors = errors };
    }

    private static void ValidateSize(IReadOnlyCollection<SquadMemberDto> squad, List<string> errors)
    {
        if (squad.Count != FplRules.SquadSize)
        {
            errors.Add($"A squad must have {FplRules.SquadSize} players, this one has {squad.Count}.");
        }
    }

    private static void ValidateUniqueness(IReadOnlyCollection<SquadMemberDto> squad, List<string> errors)
    {
        foreach (var duplicate in squad.GroupBy(m => m.PlayerId).Where(g => g.Count() > 1))
        {
            errors.Add($"{duplicate.First().WebName} appears more than once.");
        }

        var positions = squad.Select(m => m.SquadPosition).ToList();
        if (positions.Any(p => p is < 1 or > FplRules.SquadSize))
        {
            errors.Add($"Squad positions must be between 1 and {FplRules.SquadSize}.");
        }

        if (positions.Distinct().Count() != positions.Count)
        {
            errors.Add("Each squad position (1–15) can be used only once.");
        }
    }

    private static void ValidateShape(IReadOnlyCollection<SquadMemberDto> squad, List<string> errors)
    {
        foreach (var (position, required) in FplRules.SquadShape)
        {
            var count = squad.Count(m => m.Position == position);
            if (count != required)
            {
                errors.Add($"A squad needs exactly {required} {Plural(position)}, this one has {count}.");
            }
        }
    }

    private static void ValidateClubLimit(IReadOnlyCollection<SquadMemberDto> squad, List<string> errors)
    {
        foreach (var club in squad.GroupBy(m => m.ClubId).Where(g => g.Count() > FplRules.MaxPlayersPerClub))
        {
            errors.Add($"At most {FplRules.MaxPlayersPerClub} players from one club are allowed, " +
                       $"{club.First().ClubShortName} has {club.Count()}.");
        }
    }

    private static void ValidateFormation(IReadOnlyCollection<SquadMemberDto> squad, List<string> errors)
    {
        var starters = squad.Where(m => FplRules.IsStarter(m.SquadPosition)).ToList();
        if (starters.Count != FplRules.StartingElevenSize)
        {
            errors.Add($"Positions 1–{FplRules.StartingElevenSize} must hold the starting XI.");
            return;
        }

        foreach (var (position, (min, max)) in FplRules.StartingElevenLimits)
        {
            var count = starters.Count(m => m.Position == position);
            if (count < min || count > max)
            {
                errors.Add(min == max
                    ? $"The starting XI needs exactly {min} {Plural(position)}, it has {count}."
                    : $"The starting XI needs {min}–{max} {Plural(position)}, it has {count}.");
            }
        }
    }

    private static void ValidateCaptaincy(IReadOnlyCollection<SquadMemberDto> squad, List<string> errors)
    {
        var captains = squad.Where(m => m.IsCaptain).ToList();
        var viceCaptains = squad.Where(m => m.IsViceCaptain).ToList();

        if (captains.Count != 1)
        {
            errors.Add($"The squad needs exactly one captain, it has {captains.Count}.");
        }

        if (viceCaptains.Count != 1)
        {
            errors.Add($"The squad needs exactly one vice-captain, it has {viceCaptains.Count}.");
        }

        if (squad.Any(m => m.IsCaptain && m.IsViceCaptain))
        {
            errors.Add("The captain and vice-captain must be different players.");
        }

        if (captains.Concat(viceCaptains).Any(m => !FplRules.IsStarter(m.SquadPosition)))
        {
            errors.Add("The captain and vice-captain must be in the starting XI.");
        }
    }

    private static string Plural(Position position) => position switch
    {
        Position.Goalkeeper => "goalkeepers",
        Position.Defender => "defenders",
        Position.Midfielder => "midfielders",
        _ => "forwards"
    };
}
