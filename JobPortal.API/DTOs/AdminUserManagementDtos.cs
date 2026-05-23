namespace JobPortal.API.DTOs;

public class AdminManagedEmployerDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Image { get; set; }
    public string Status { get; set; } = null!;
    public int PostingLimit { get; set; }
    public int JobCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminManagedJobSeekerDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public string? ProfileImage { get; set; }
    public string Status { get; set; } = null!;
    public int ApplicationCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SetAccountActiveRequest
{
    public bool Active { get; set; }
}
