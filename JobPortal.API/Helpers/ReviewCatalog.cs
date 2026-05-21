namespace JobPortal.API.Helpers;

public static class ReviewCatalog
{
    public const string EmployerToSeeker = "employer_to_seeker";
    public const string SeekerToEmployer = "seeker_to_employer";

    public const int MinRating = 1;
    public const int MaxRating = 5;
    public const int MaxCommentLength = 2000;

    public static bool IsValidReviewType(string type) =>
        type.Trim().ToLowerInvariant() is EmployerToSeeker or SeekerToEmployer;
}
