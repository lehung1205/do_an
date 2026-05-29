namespace JobPortal.API.DTOs;

public class TopRatedUserDto
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Email { get; set; }

    public double AverageRating { get; set; }

    public int ReviewCount { get; set; }
}

public class TopRatedListsDto
{
    public IReadOnlyList<TopRatedUserDto> TopEmployers { get; set; } = Array.Empty<TopRatedUserDto>();

    public IReadOnlyList<TopRatedUserDto> TopJobSeekers { get; set; } = Array.Empty<TopRatedUserDto>();
}
