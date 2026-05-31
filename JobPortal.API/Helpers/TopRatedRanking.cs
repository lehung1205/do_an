namespace JobPortal.API.Helpers;

/// <summary>
/// Xếp hạng top rated bằng điểm Bayesian (IMDB-style) để tránh 1 đánh giá 5 sao vượt người có nhiều đánh giá.
/// </summary>
public static class TopRatedRanking
{
    /// <summary>
    /// Trọng số prior: cần khoảng N đánh giá thì điểm mới "tin cậy" bằng trung bình thực tế.
    /// </summary>
    public const int MinimumReviewsForPrior = 5;

    public const double FallbackGlobalMean = 3.5;

    public record AggregatedRating(long Id, double AverageRating, int ReviewCount);

    /// <summary>WR = (v * R + m * C) / (v + m)</summary>
    public static double ComputeWeightedScore(double averageRating, int reviewCount, double globalMean)
    {
        return (reviewCount * averageRating + MinimumReviewsForPrior * globalMean)
               / (reviewCount + MinimumReviewsForPrior);
    }

    public static IEnumerable<AggregatedRating> RankByWeightedScore(
        IEnumerable<AggregatedRating> items,
        double globalMean,
        int limit)
    {
        return items
            .Select(item => new
            {
                Item = item,
                WeightedScore = ComputeWeightedScore(item.AverageRating, item.ReviewCount, globalMean)
            })
            .OrderByDescending(x => x.WeightedScore)
            .ThenByDescending(x => x.Item.ReviewCount)
            .ThenByDescending(x => x.Item.AverageRating)
            .Take(limit)
            .Select(x => x.Item);
    }
}
