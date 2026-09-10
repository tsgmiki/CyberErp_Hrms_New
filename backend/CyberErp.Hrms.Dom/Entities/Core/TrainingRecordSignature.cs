namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// What a signature asserts.
///
/// <para>⚠️ The MEANING is part of the signature, not decoration. Part 11 requires a signed record to
/// carry what the signing meant — "I did this training" and "I checked that they did" are different
/// claims by different people, and a signature that does not say which is evidence of nothing
/// (logic §12.90).</para>
/// </summary>
public enum SignatureMeaning
{
    /// <summary>The learner: "I confirm I completed this training."</summary>
    Completion = 0,
    /// <summary>A verifier: "I confirm this record is accurate."</summary>
    Verification = 1
}

/// <summary>
/// An electronic signature bound to one training record.
///
/// <para>⚠️ IMMUTABLE AND NEVER DELETED. There is no mutator on this type and no delete path in the
/// application. A signature that can be withdrawn is not a signature; a correction is a new record
/// and a new signing, which is why <see cref="ContentHash"/> exists.</para>
///
/// <para>⚠️ <see cref="ContentHash"/> BINDS THE SIGNATURE TO WHAT WAS SIGNED. It is a SHA-256 over a
/// canonical rendering of the facts — who, which course, which version, when completed, what score.
/// If any of those later differ, the stored hash no longer matches and the record is detectably not
/// the one that was signed. Without it a signature is a row asserting something about a record that
/// may since have changed, which is the failure mode Part 11's "signature linked to the record"
/// clause exists to prevent.</para>
///
/// <para><see cref="SignedStatement"/> keeps the human-readable text that was shown at signing, so
/// the manifestation on a printed record is what the person actually agreed to rather than a
/// re-rendering that may have drifted.</para>
/// </summary>
public class TrainingRecordSignature : BaseEntity, IAggregateRoot, IAuditable
{
    /// <summary>The completion being signed. Every signature is anchored to one.</summary>
    public Guid TrainingEnrollmentId { get; private set; }
    /// <summary>Set when the completion also discharges a mandatory-training obligation.</summary>
    public Guid? AssignmentObligationId { get; private set; }
    /// <summary>Whose training record this is.</summary>
    public Guid EmployeeId { get; private set; }

    public SignatureMeaning Meaning { get; private set; }
    /// <summary>The account that signed — the identity re-authenticated at the moment of signing.</summary>
    public Guid SignedByUserId { get; private set; }
    /// <summary>Captured at signing so the manifestation does not depend on the account still existing.</summary>
    public string SignedByName { get; private set; } = string.Empty;
    public DateTime SignedOn { get; private set; }

    /// <summary>SHA-256, base64, over the canonical statement of facts. See the type remarks.</summary>
    public string ContentHash { get; private set; } = string.Empty;
    /// <summary>The exact wording presented to the signer.</summary>
    public string SignedStatement { get; private set; } = string.Empty;
    /// <summary>Optional note from a verifier.</summary>
    public string? Note { get; private set; }

    private TrainingRecordSignature() : base() { }

    public static TrainingRecordSignature Create(
        Guid trainingEnrollmentId,
        Guid? assignmentObligationId,
        Guid employeeId,
        SignatureMeaning meaning,
        Guid signedByUserId,
        string signedByName,
        string contentHash,
        string signedStatement,
        string? note)
    {
        if (trainingEnrollmentId == Guid.Empty)
            throw new ArgumentException("An enrolment is required.", nameof(trainingEnrollmentId));
        if (employeeId == Guid.Empty)
            throw new ArgumentException("An employee is required.", nameof(employeeId));
        if (signedByUserId == Guid.Empty)
            throw new ArgumentException("A signing account is required.", nameof(signedByUserId));
        if (string.IsNullOrWhiteSpace(contentHash))
            throw new ArgumentException("A content hash is required.", nameof(contentHash));
        if (string.IsNullOrWhiteSpace(signedStatement))
            throw new ArgumentException("A signed statement is required.", nameof(signedStatement));

        return new TrainingRecordSignature
        {
            TrainingEnrollmentId = trainingEnrollmentId,
            AssignmentObligationId = assignmentObligationId == Guid.Empty ? null : assignmentObligationId,
            EmployeeId = employeeId,
            Meaning = meaning,
            SignedByUserId = signedByUserId,
            SignedByName = signedByName,
            SignedOn = DateTime.UtcNow,
            ContentHash = contentHash,
            SignedStatement = signedStatement,
            Note = note
        };
    }
}
