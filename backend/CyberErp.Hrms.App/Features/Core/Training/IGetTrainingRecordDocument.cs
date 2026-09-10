namespace CyberErp.Hrms.App.Features.Core.Training
{
    /// <summary>
    /// Renders an employee's complete training record as a document, with signature manifestations.
    ///
    /// <para>Declared here and implemented in Inf, because generating a PDF is infrastructure — the
    /// application layer states that a record copy exists, not how it is drawn (logic §12.90).</para>
    /// </summary>
    public interface IGetTrainingRecordDocument
    {
        Task<(byte[] Content, string FileName)> GetAsync(Guid employeeId);
    }
}
