using CloudStorage.Application.Services;

namespace CloudStorage.UnitTests.Utilities
{
    public class FileNameSanitizerTests
    {
        [Fact]
        public void Sanitize_Should_Keep_Valid_FileName_Unchanged()
        {
            // Arrange
            var fileName = "document.pdf";

            // Act
            var result = FileNameSanitizer.Sanitize(fileName);

            // Assert
            Assert.Equal(fileName, result);
        }

        [Theory]
        [InlineData(" document.pdf ")]
        [InlineData("  document.pdf")]
        [InlineData("document.pdf  ")]
        public void Sanitize_Should_Trim_Whitespace(string fileName)
        {
            // Act
            var result = FileNameSanitizer.Sanitize(fileName);

            // Assert
            Assert.Equal("document.pdf", result);
        }


        [Fact]
        public void Sanitize_Should_Replace_Invalid_Characters()
        {
            // Arrange
            var fileName = "my/file.pdf";

            // Act
            var result = FileNameSanitizer.Sanitize(fileName);

            // Assert
            Assert.Equal("my_file.pdf", result);
        }


        [Theory]
        [InlineData("folder/file.pdf")]
        [InlineData("folder\\file.pdf")]
        public void Sanitize_Should_Remove_Path_Separators( string fileName)
        {
            // Act
            var result = FileNameSanitizer.Sanitize(fileName);

            // Assert
            Assert.DoesNotContain("/", result);
            Assert.DoesNotContain("\\", result);
        }

        [Fact]
        public void Sanitize_Should_Remove_Control_Characters()
        {
            // Arrange
            var fileName = "document\u0000.pdf";

            // Act
            var result = FileNameSanitizer.Sanitize(fileName);

            // Assert
            Assert.DoesNotContain('\0', result);
        }
    }
}
