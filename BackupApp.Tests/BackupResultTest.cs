using BackupApp.Models;
namespace BackupApp.Tests;

public class BackupResultTests
{
    [Fact]
    public void ToString_ShouldFormatMegabytesCorrectly()
    {
        // Arrange
        var result = new BackupResult(5, new Dictionary<string, string>(), 2097152); // 2 MB в байтах

        // Act
        string output = result.ToString();

        // Assert
        Assert.Equal("Files successfully copied: 5 | Total volume: 2,00 MB", output);
    }
}