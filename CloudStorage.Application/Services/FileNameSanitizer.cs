namespace CloudStorage.Application.Services
{
    public class FileNameSanitizer
    {
        public static string Sanitize(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return "unnamed-file";

            // Remove directory/path information.
            var sanitized = Path.GetFileName(fileName);

            // Remove characters that are invalid for a file name.
            var invalidCharacters = Path.GetInvalidFileNameChars();

            sanitized = string.Concat(sanitized.Where(character => !invalidCharacters.Contains(character)));

            // Prevent Windows special names from becoming problematic.
            sanitized = sanitized.Trim().Trim('.');

            if (string.IsNullOrWhiteSpace(sanitized)) return "unnamed-file";

            // Avoid excessively long filenames.
            if (sanitized.Length > 255)
            {
                var extension = Path.GetExtension(sanitized);

                var name = Path.GetFileNameWithoutExtension(sanitized);

                var maxNameLength = 255 - extension.Length;

                sanitized = name[..Math.Min(name.Length, maxNameLength)] + extension;
            }

            return sanitized;
        }
    }
}
