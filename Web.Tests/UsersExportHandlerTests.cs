using System.IO;
using System.Threading.Tasks;
using ExportWeb.Screens;
using Xunit;

namespace Web.Tests
{
    public class UsersExportHandlerTests
    {
        [Fact]
        public async Task Handler_Should_Create_File_With_Content()
        {
            // Arrange
            var handler = new UsersExportHandler();
            var dir = Path.Combine(Path.GetTempPath(), "HandlerTests");
            Directory.CreateDirectory(dir);

            // Act
            var path = await handler.GenerateAsync(new System.Collections.Generic.Dictionary<string, string>(), dir, default);

            // Assert
            Assert.True(File.Exists(path));
            var content = await File.ReadAllTextAsync(path);
            Assert.Contains("Id,Name,Email", content);
        }
    }
}
