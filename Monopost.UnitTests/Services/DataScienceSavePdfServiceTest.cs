using Monopost.BLL.Services.Implementations;
using Moq;

namespace Monopost.UnitTests.Services
{
    public interface IDirectoryWrapper
    {
        bool Exists { get; }
        string FullName { get; }
    }

    public class DirectoryWrapper : IDirectoryWrapper
    {
        private readonly DirectoryInfo _directoryInfo;

        public DirectoryWrapper(string path)
        {
            _directoryInfo = new DirectoryInfo(path);
        }

        public bool Exists => _directoryInfo.Exists;
        public string FullName => _directoryInfo.FullName;
    }

    public class DataScienceSavingPdfServiceTests
    {
        private readonly DataScienceSavingPdfService _service;
        private readonly Mock<IDirectoryWrapper> _directoryMock;
        private readonly string _testFilePath;

        public DataScienceSavingPdfServiceTests()
        {

            var testCsvContent = @"Дата та час операції,Категорія операції,Сума,Валюта,Додаткова інформація,Коментар до платежу,Залишок,Валюта залишку
13.10.2024 08:39,Часткове зняття,1700.00,UAH,На чорну картку,,7000.52,UAH";

            _testFilePath = Path.GetTempFileName();
            File.WriteAllText(_testFilePath, testCsvContent);

            _directoryMock = new Mock<IDirectoryWrapper>();
            _service = new DataScienceSavingPdfService(_testFilePath);
        }

        [Fact]
        public void SaveResults_ReturnsSuccess_WhenOutputDirectoryExists()
        {

            string validPath = Path.GetTempPath();

            var result = _service.SaveResults("test.pdf", validPath);

            Assert.True(result.Success);
            Assert.Contains("Final report generated", result.Message);
        }

        [Fact]
        public void SaveResults_ReturnsFailure_WhenOutputDirectoryDoesNotExist()
        {
            _directoryMock.Setup(d => d.Exists).Returns(false);
            _directoryMock.Setup(d => d.FullName).Returns("test/nonexistent/path");

            var result = _service.SaveResults("test.pdf", _directoryMock.Object.FullName);

            Assert.False(result.Success);
            Assert.Contains("Output directory does not exist", result.Message);
        }

        public void Dispose()
        {
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }
    }
}