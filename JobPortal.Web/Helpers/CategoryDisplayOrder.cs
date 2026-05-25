using JobPortal.Web.Dtos;

namespace JobPortal.Web.Helpers;

public static class CategoryDisplayOrder
{
    public static List<CategoryDto> SortOtherLast(IEnumerable<CategoryDto> categories) =>
        categories
            .OrderBy(c => IsOtherCategory(c.Name) ? 1 : 0)
            .ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

    public static bool IsOtherCategory(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var trimmed = name.Trim();
        return string.Equals(trimmed, "Khác", StringComparison.OrdinalIgnoreCase)
            || string.Equals(trimmed, "Khac", StringComparison.OrdinalIgnoreCase);
    }
}
