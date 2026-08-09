using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Tests.Compensation;

/// <summary>
/// The line is the durable record of a pay decision, so it has to carry the REASON alongside the
/// number. These cover what the increment grid reads back after a revision is saved.
/// </summary>
public class SalaryRevisionLineTests
{
    private static readonly Guid Plan = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Alice = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");

    [Fact]
    public void A_line_defaults_to_a_full_unexplained_increment()
    {
        // The flat-rate path passes no eligibility data at all, and must stay unchanged.
        var line = SalaryRevisionLine.Create(Plan, Alice, 50000m, 55000m);

        Assert.Equal(1m, line.ProrationFactor);
        Assert.Null(line.MonthsOfService);
        Assert.Null(line.Note);
    }

    [Fact]
    public void A_prorated_line_keeps_the_service_that_justified_it()
    {
        var line = SalaryRevisionLine.Create(
            Plan, Alice, 50000m, 51666.67m, monthsOfService: 4, prorationFactor: 0.333333m,
            note: "Prorated to 4/12 months of service.");

        Assert.Equal(4, line.MonthsOfService);
        Assert.Equal(0.333333m, line.ProrationFactor);
        Assert.Contains("4/12", line.Note!);
    }

    [Fact]
    public void An_HR_override_stops_claiming_the_engine_prorated_it()
    {
        // Otherwise "prorated to 4/12 months of service" would sit next to a hand-keyed figure and
        // read as the system's justification for a decision a person actually made.
        var line = SalaryRevisionLine.Create(
            Plan, Alice, 50000m, 51666.67m, 4, 0.333333m, "Prorated to 4/12 months of service.");

        line.SetProposed(54000m);

        Assert.Equal(54000m, line.ProposedSalary);
        Assert.Equal(1m, line.ProrationFactor);
        Assert.DoesNotContain("Prorated", line.Note!);
    }

    [Fact]
    public void An_override_cannot_be_negative()
    {
        var line = SalaryRevisionLine.Create(Plan, Alice, 50000m, 55000m);

        Assert.Throws<ArgumentException>(() => line.SetProposed(-1m));
    }
}
