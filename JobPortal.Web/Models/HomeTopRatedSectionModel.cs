using JobPortal.Web.Dtos;

namespace JobPortal.Web.Models;

public class HomeTopRatedSectionModel
{
    /// <summary>seeker | employer</summary>
    public string Variant { get; init; } = "seeker";

    public IReadOnlyList<TopRatedUserDto> TopEmployers { get; init; } = Array.Empty<TopRatedUserDto>();

    public IReadOnlyList<TopRatedUserDto> TopJobSeekers { get; init; } = Array.Empty<TopRatedUserDto>();

    public bool IsEmployerVariant => string.Equals(Variant, "employer", StringComparison.OrdinalIgnoreCase);

    public bool LoadFailed { get; init; }

    public static HomeTopRatedSectionModel FromLists(TopRatedListsDto? lists, string variant) =>
        lists == null
            ? new HomeTopRatedSectionModel { Variant = variant, LoadFailed = true }
            : new HomeTopRatedSectionModel
            {
                Variant = variant,
                TopEmployers = lists.TopEmployers,
                TopJobSeekers = lists.TopJobSeekers
            };
}
