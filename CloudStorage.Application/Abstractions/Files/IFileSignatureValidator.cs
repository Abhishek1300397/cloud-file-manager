namespace CloudStorage.Application.Abstractions.Files
{
    public interface IFileSignatureValidator
    {
        Task<bool> IsValidAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
    }
}
