using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// Attendance (HC042): the id a terminal knows an employee by.
/// </summary>
/// <remarks>
/// <para>⚠️ THIS TABLE EXISTS BECAUSE DEVICES DO NOT KNOW ABOUT EMPLOYEES. A terminal reports
/// "user 37 punched at 08:02" — 37 is an enrolment slot on that machine, not an employee number,
/// and the same person can be slot 37 on one door and slot 412 on another.</para>
///
/// <para>⚠️ Scoped to ONE device on purpose. Making the device id optional, so a mapping could be
/// "global", would let two terminals that both use slot 1 resolve to the same person — which is how
/// one employee ends up with another's attendance.</para>
///
/// <para>Where no mapping exists the resolver falls back to matching the reported id against
/// <c>Employee.EmployeeNumber</c>, which is how the simplest deployments are set up. A punch that
/// matches neither is kept UNRESOLVED rather than discarded — see <c>AttendancePunch</c>.</para>
/// </remarks>
public class AttendanceEnrollment : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid AttendanceDeviceId { get; private set; }
    public Guid EmployeeId { get; private set; }

    /// <summary>The identifier as the device reports it.</summary>
    public string DeviceUserId { get; private set; } = string.Empty;

    public bool IsActive { get; private set; } = true;

    public AttendanceDevice? AttendanceDevice { get; private set; }
    public Employee? Employee { get; private set; }

    private AttendanceEnrollment() : base() { }

    public static AttendanceEnrollment Create(Guid deviceId, Guid employeeId, string deviceUserId, bool isActive = true)
    {
        Guard(deviceId, employeeId, deviceUserId);
        return new AttendanceEnrollment
        {
            AttendanceDeviceId = deviceId,
            EmployeeId = employeeId,
            DeviceUserId = deviceUserId.Trim(),
            IsActive = isActive
        };
    }

    public void Update(Guid employeeId, string deviceUserId, bool isActive)
    {
        Guard(AttendanceDeviceId, employeeId, deviceUserId);
        EmployeeId = employeeId;
        DeviceUserId = deviceUserId.Trim();
        IsActive = isActive;
        base.Update();
    }

    private static void Guard(Guid deviceId, Guid employeeId, string deviceUserId)
    {
        if (deviceId == Guid.Empty)
            throw new ArgumentException("Device is required.", nameof(deviceId));
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee is required.", nameof(employeeId));
        if (string.IsNullOrWhiteSpace(deviceUserId))
            throw new ArgumentException("The device user id is required.", nameof(deviceUserId));
    }
}
