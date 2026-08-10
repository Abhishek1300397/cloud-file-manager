using CloudStorage.Application.Abstractions.Files;

namespace CloudStorage.Application.Services
{
    internal class FileSignatureValidator : IFileSignatureValidator
    {
        private static readonly Dictionary<string, byte[][]> Signatures = new(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = [[0x25, 0x50, 0x44, 0x46]],

            [".jpg"] = [[0xFF, 0xD8, 0xFF]],

            [".jpeg"] = [[0xFF, 0xD8, 0xFF]],

            [".png"] = [[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]],

            [".doc"] = [[0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1]],

            [".docx"] = [[0x50, 0x4B, 0x03, 0x04]]

        };

        public async Task<bool> IsValidAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
        {
            var extension = Path.GetExtension(fileName);

            // Text files don't have a reliable magic-byte signature.
            if (extension.Equals(".txt", StringComparison.OrdinalIgnoreCase)) return true;

            if (!Signatures.TryGetValue(extension, out var signatures)) return false;

            if (!stream.CanSeek) throw new InvalidOperationException("File stream must support seeking.");

            stream.Position = 0;

            var maxSignatureLength = signatures.Max(signature => signature.Length);

            var buffer = new byte[maxSignatureLength];

            var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, maxSignatureLength), cancellationToken);

            stream.Position = 0;

            return signatures.Any(signature => bytesRead >= signature.Length && buffer.AsSpan(0, signature.Length).SequenceEqual(signature));
        }
    }
}
