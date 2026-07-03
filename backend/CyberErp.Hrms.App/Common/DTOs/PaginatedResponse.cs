namespace CyberErp.Hrms.App.Common.DTOs
{
    public class PaginatedResponse<T>
    {
        public int Total { get; set; }
        public IEnumerable<T> Data { get; set; } = new List<T>();
    }
}
