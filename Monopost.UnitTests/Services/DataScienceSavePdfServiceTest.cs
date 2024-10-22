﻿using System;
using System.IO;
using Xunit;
using Monopost.BLL.Services.Implementations;
using Monopost.BLL.Models;
using Monopost.Logging;
using Serilog;
using Monopost.BLL.Services.Interfaces;
using Moq;
using PdfSharp.Pdf;

namespace Monopost.UnitTests.Services
{
    public class DataScienceSavingPdfServiceTests : IDisposable
    {
        private const string TestFilePath = "Jar_statement_13.10.2024_193241.csv";
        private const string OutputDirectory = "C:\\Users\\XPS\\Documents\\Monopost";
        private bool _disposedValue;

        public DataScienceSavingPdfServiceTests()
        {
            SetupTestEnvironment();
        }

        private void SetupTestEnvironment()
        {

            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    CleanupTestEnvironment();
                }
                _disposedValue = true;
            }
        }

        private void CleanupTestEnvironment()
        {
         if (File.Exists(TestFilePath))
         {
             File.Delete(TestFilePath);
         }
        
         if (Directory.Exists(OutputDirectory))
         {
             Directory.Delete(OutputDirectory, true);
         }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        [Fact]
        public void SaveResults_ReturnsSuccess_WhenOutputDirectoryExists()
        {
            var service = new DataScienceSavingPdfService(TestFilePath);
            string fileName = "FinalReport.pdf";

            var result = service.SaveResults(fileName, OutputDirectory);

            Assert.True(result.Success);
            Assert.Contains("Final report generated", result.Message);
        }

       [Fact]
       public void SaveResults_ReturnsFailure_WhenOutputDirectoryDoesNotExist()
       {
           var service = new DataScienceSavingPdfService(TestFilePath);
           string fileName = "FinalReport.pdf";
           string nonExistentDirectory = "NonExistentDirectory";
      
           var result = service.SaveResults(fileName, nonExistentDirectory);
      
           Assert.False(result.Success);
           Assert.Contains("Output directory does not exist", result.Message);
       }
      
    }
}