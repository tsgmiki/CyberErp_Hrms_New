using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// One band of the admin-configurable progressive income-tax table (HC231/HC232). Tax is computed
/// marginally: the portion of taxable income between <see cref="LowerBound"/> and
/// <see cref="UpperBound"/> is taxed at <see cref="RatePercent"/>. This is a configurable table, not
/// a hardcoded statutory law — real compliance is a matter of correct configuration per jurisdiction.
/// </summary>
public class TaxBracket : BaseEntity, IAggregateRoot, IAuditable
{
    public decimal LowerBound { get; private set; }
    /// <summary>Upper bound of the band; null = no ceiling (top bracket).</summary>
    public decimal? UpperBound { get; private set; }
    public decimal RatePercent { get; private set; }
    public int SortOrder { get; private set; }

    private TaxBracket() : base() { }

    public static TaxBracket Create(decimal lowerBound, decimal? upperBound, decimal ratePercent, int sortOrder)
    {
        Guard(lowerBound, upperBound, ratePercent);
        return new TaxBracket
        {
            LowerBound = lowerBound,
            UpperBound = upperBound,
            RatePercent = ratePercent,
            SortOrder = sortOrder
        };
    }

    public void Update(decimal lowerBound, decimal? upperBound, decimal ratePercent, int sortOrder)
    {
        Guard(lowerBound, upperBound, ratePercent);
        LowerBound = lowerBound;
        UpperBound = upperBound;
        RatePercent = ratePercent;
        SortOrder = sortOrder;
        base.Update();
    }

    private static void Guard(decimal lowerBound, decimal? upperBound, decimal ratePercent)
    {
        if (lowerBound < 0)
            throw new ArgumentException("Lower bound cannot be negative.", nameof(lowerBound));
        if (upperBound.HasValue && upperBound.Value <= lowerBound)
            throw new ArgumentException("Upper bound must exceed the lower bound.", nameof(upperBound));
        if (ratePercent is < 0 or > 100)
            throw new ArgumentException("Tax rate must be between 0 and 100.", nameof(ratePercent));
    }
}
