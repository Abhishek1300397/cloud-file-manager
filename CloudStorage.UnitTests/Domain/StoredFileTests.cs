using CloudStorage.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStorage.UnitTests.Domain
{
    public class StoredFileTests
    {
        [Fact]
        public void Constructor_Should_Create_File_With_Expected_Values()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var fileName = "document.pdf";
            var objectKey = "users/123/document.pdf";
            var contentType = "application/pdf";
            var size = 1024L;

            // Act
            var file = new StoredFile(
                userId,
                fileName,
                objectKey,
                contentType,
                size);

            // Assert
            Assert.NotEqual(Guid.Empty, file.Id);
            Assert.Equal(userId, file.UserId);
            Assert.Equal(fileName, file.OriginalFileName);
            Assert.Equal(objectKey, file.ObjectKey);
            Assert.Equal(contentType, file.ContentType);
            Assert.Equal(size, file.Size);
            Assert.NotEqual(default, file.CreatedAtUtc);
        }

        [Fact]
        public void Rename_Should_Update_OriginalFileName()
        {
            // Arrange
            var file = new StoredFile(
                Guid.NewGuid(),
                "old-name.pdf",
                "users/123/file.pdf",
                "application/pdf",
                1024);

            // Act
            file.Rename("new-name.pdf");

            // Assert
            Assert.Equal("new-name.pdf", file.OriginalFileName);
        }

        [Fact]
        public void Rename_Should_Not_Change_ObjectKey()
        {
            // Arrange
            var objectKey = "users/123/abc123.pdf";

            var file = new StoredFile(
                Guid.NewGuid(),
                "old-name.pdf",
                objectKey,
                "application/pdf",
                1024);

            // Act
            file.Rename("new-name.pdf");

            // Assert
            Assert.Equal(objectKey, file.ObjectKey);
        }


        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public void Rename_Should_Throw_When_FileName_Is_Empty(string fileName)
        {
            // Arrange
            var file = new StoredFile(
                Guid.NewGuid(),
                "old-name.pdf",
                "users/123/file.pdf",
                "application/pdf",
                1024);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(
                () => file.Rename(fileName));

            Assert.Equal(
                "fileName",
                exception.ParamName);
        }
    }
}
