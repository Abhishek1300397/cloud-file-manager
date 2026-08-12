namespace CloudStorage.Application.DTOs.Requests
{
    public sealed class FileListRequest
    {
        public int Page { get; init; } = 1;

        public int PageSize { get; init; } = 20;

        public string? Search { get; init; }
    }
}
