namespace JobPortal.Web.Dtos;

public class AdminPostingPackageDto
{
    public long Id { get; set; }
    public long AdminId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int Price { get; set; }
    public int PostingLimit { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int PaymentCount { get; set; }
}

public class CreateAdminPostingPackageRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int Price { get; set; }
    public int PostingLimit { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateAdminPostingPackageRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int Price { get; set; }
    public int PostingLimit { get; set; }
    public bool IsActive { get; set; }
}

public class SetPostingPackageActiveRequest
{
    public bool IsActive { get; set; }
}
