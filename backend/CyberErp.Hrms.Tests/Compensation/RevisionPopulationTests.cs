using CyberErp.Hrms.App.Features.Core.Compensation;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Tests.Compensation;

/// <summary>
/// Who a salary revision is allowed to touch.
///
/// <para>A leaver keeps their last salary on the record, so nothing about the pay data marks them as
/// gone — without an explicit filter they arrive with a positive base like anyone else and are
/// proposed for a raise. The predicate is compiled from the same expression EF sends to SQL, so this
/// pins the real filter rather than a restatement of it.</para>
/// </summary>
public class RevisionPopulationTests
{
    private static readonly Func<Employee, bool> StillEmployed =
        SalaryRevisionShared.StillEmployed.Compile();

    private static Employee Employee(EmploymentStatus status = EmploymentStatus.Active) =>
        Dom.Entities.Core.Employee.Create(
            Guid.NewGuid(), "EMP-1", status, salary: 50000m, hireDate: new DateTime(2020, 1, 1));

    [Theory]
    [InlineData(EmploymentStatus.Active)]
    [InlineData(EmploymentStatus.Probation)]
    [InlineData(EmploymentStatus.OnLeave)]
    [InlineData(EmploymentStatus.Suspended)]
    public void Everyone_still_on_the_payroll_is_included(EmploymentStatus status)
    {
        // Being on leave or suspended does not stop pay, so it must not stop a pay revision either.
        Assert.True(StillEmployed(Employee(status)));
    }

    [Fact]
    public void A_terminated_employee_is_excluded()
    {
        var employee = Employee();
        employee.Terminate();

        Assert.False(StillEmployed(employee));
    }

    [Fact]
    public void The_status_alone_is_enough_to_exclude()
    {
        // The two fields are set independently and can disagree; either one saying "terminated" wins.
        Assert.False(StillEmployed(Employee(EmploymentStatus.Terminated)));
    }

    [Fact]
    public void A_retired_employee_is_excluded()
    {
        // Retired is its own status with no IsTerminated flag behind it, so it needs its own check —
        // and a retiree has left just as surely as a leaver.
        Assert.False(StillEmployed(Employee(EmploymentStatus.Retired)));
    }

    [Fact]
    public void A_reinstated_employee_is_included_again()
    {
        var employee = Employee();
        employee.Terminate();
        employee.Reinstate(Guid.NewGuid(), null);

        Assert.True(StillEmployed(employee));
    }
}
