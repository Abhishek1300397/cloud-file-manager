using CloudStorage.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudStorage.Infrastructure.Persistence.Configurations
{
    public sealed class StoredFileConfiguration: IEntityTypeConfiguration<StoredFile>
    {
        public void Configure(EntityTypeBuilder<StoredFile> builder)
        {
            builder.ToTable("stored_files");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(x => x.UserId).IsRequired().HasMaxLength(100);

            builder.Property(x => x.OriginalFileName).IsRequired().HasMaxLength(255);

            builder.Property(x => x.ObjectKey).IsRequired().HasMaxLength(500);

            builder.Property(x => x.ContentType).IsRequired().HasMaxLength(150);

            builder.Property(x => x.Size).IsRequired();

            builder.Property(x => x.CreatedAtUtc).IsRequired();
        }
    }
}
