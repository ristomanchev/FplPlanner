using System.ComponentModel.DataAnnotations;
using ProektIntegrirani.Domain.Enums;

namespace ProektIntegrirani.Web.Request;

public record PlayerRequest(
    [Range(1, int.MaxValue)] int FplId,
    [Required, StringLength(100)] string FirstName,
    [Required, StringLength(100)] string LastName,
    [Required, StringLength(50)] string WebName,
    [EnumDataType(typeof(Position))] Position Position,
    [Range(3.5, 20.0)] decimal Price,
    [EnumDataType(typeof(PlayerStatus))] PlayerStatus Status,
    [Range(0, 100)] int? ChanceOfPlaying,
    [StringLength(500)] string? News,
    [Required] Guid ClubId);
