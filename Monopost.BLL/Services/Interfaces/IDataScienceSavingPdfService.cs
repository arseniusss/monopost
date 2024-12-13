﻿using Monopost.BLL.Models;

namespace Monopost.BLL.Services.Interfaces
{
    public interface IDataScienceSavingPdfService
    {
        public Result<string> SaveResults(string fileName, string outputDirectory);
    }
}
