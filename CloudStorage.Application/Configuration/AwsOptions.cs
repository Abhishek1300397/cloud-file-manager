namespace CloudStorage.Application.Configuration
{
    public class AwsOptions
    {
        public static readonly string SectionName = "AWS";
        public string Region { get; set; } = string.Empty;
        public string BucketName { get; set; } = string.Empty;
    }
}
