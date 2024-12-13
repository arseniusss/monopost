﻿using System.Text;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Pdf.IO;
using Monopost.Logging;
using Serilog;
using Monopost.BLL.Enums;
using Transaction = Monopost.BLL.Models.Transaction;
using Monopost.BLL.Models;
using Monopost.BLL.Services.Interfaces;

namespace Monopost.BLL.Services.Implementations
{
    public class DataScienceSavingPdfService : IDataScienceSavingPdfService
    {
        public static ILogger logger = LoggerConfig.GetLogger();
        private List<Transaction> transactions;
        public DataScienceService? manager = new DataScienceService(); //було private

        public DataScienceSavingPdfService(string filepath)
        {
            transactions = new List<Transaction>();
            manager.LoadFromCSVs(new List<Tuple<string, string>> { new Tuple<string, string>(filepath, "") });
        }

        private void GenerateStatisticsPdf(
            Dictionary<string, decimal> donationTotalAmountsByTimeOfDay,
            Dictionary<string, decimal> donationCountByTimeOfDay,
            Dictionary<string, decimal> totalAmountByHour,
            Dictionary<string, decimal> averageAmountByHour,
            Dictionary<string, decimal> maxAmountByHour,
            Dictionary<string, decimal> donationCountByDayOfWeek,
            Dictionary<string, decimal> withdrawalCountByTimeOfDay,
            Dictionary<string, decimal> withdrawalSumByTimeOfDay,
            Dictionary<string, decimal> withdrawalSumByDayOfWeek,
            string pdfPath)
        {
            decimal totalWithdrawalCount = withdrawalCountByTimeOfDay.Values.Sum();
            decimal totalDonationCount = donationCountByTimeOfDay.Values.Sum();
            using (PdfDocument document = new PdfDocument())
            {
                document.Info.Title = "Detailed Donation Report";

                XFont titleFont = new XFont("Verdana", 16, XFontStyle.Bold);
                XFont sectionFont = new XFont("Verdana", 14, XFontStyle.Bold);
                XFont contentFont = new XFont("Verdana", 12, XFontStyle.Regular);

                double contentHeight = 80;
                contentHeight += 40;
                contentHeight += (donationTotalAmountsByTimeOfDay.Count + donationCountByTimeOfDay.Count +
                                  totalAmountByHour.Count + averageAmountByHour.Count +
                                  maxAmountByHour.Count + donationCountByDayOfWeek.Count) * 20;
                contentHeight += 6 * 60;


                PdfPage page = document.AddPage();
                page.Width = 800;
                page.Height = (int)Math.Ceiling(contentHeight + 600);

                using (XGraphics gfx = XGraphics.FromPdfPage(page))
                {
                    double yOffset = 40;

                    void WriteSection(string title, Dictionary<string, decimal> data, ref double yOffset)
                    {
                        gfx.DrawString(title, sectionFont, XBrushes.Black, new XPoint(40, yOffset));
                        yOffset += 30;
                        foreach (var entry in data)
                        {
                            gfx.DrawString($"{entry.Key}: {entry.Value}", contentFont, XBrushes.Black, new XPoint(40, yOffset));
                            yOffset += 20;
                        }
                        yOffset += 30;
                    }

                    gfx.DrawString("Donation Report", titleFont, XBrushes.Black, new XPoint(40, yOffset));
                    yOffset += 40;

                    WriteSection("Donation Amounts by Time of Day:", donationTotalAmountsByTimeOfDay, ref yOffset);
                    WriteSection("Donation Counts by Time of Day:", donationCountByTimeOfDay, ref yOffset);
                    WriteSection("Total Amounts by Hour of Day:", totalAmountByHour, ref yOffset);
                    WriteSection("Average Amounts by Hour of Day:", averageAmountByHour, ref yOffset);
                    WriteSection("Max Donation Amounts by Hour of Day:", maxAmountByHour, ref yOffset);
                    WriteSection("Donation Counts by Day of  Week:", donationCountByDayOfWeek, ref yOffset);
                    WriteSection("Withdrawal Counts by Time of Day:", withdrawalCountByTimeOfDay, ref yOffset);
                    WriteSection("Withdrawal Amounts by Time of Day:", withdrawalSumByTimeOfDay, ref yOffset);
                    WriteSection("Withdrawal Sum by Day of Week:", withdrawalSumByDayOfWeek, ref yOffset);

                    gfx.DrawString($"Total Withdrawal Count: {totalWithdrawalCount}", sectionFont, XBrushes.Black, new XPoint(40, yOffset));
                    yOffset += 30;

                    gfx.DrawString($"Total Donation Count: {totalDonationCount}", sectionFont, XBrushes.Black, new XPoint(40, yOffset));


                }

                document.Save(pdfPath);
            }
        }

        private void GenerateChartsPdf(List<string> chartPaths, string pdfPath)
        {
            using (PdfDocument document = new PdfDocument())
            {
                foreach (var chartPath in chartPaths)
                {
                    if (File.Exists(chartPath))
                    {
                        using (XImage image = XImage.FromFile(chartPath))
                        {
                            PdfPage chartPage = document.AddPage();
                            chartPage.Width = image.PixelWidth;
                            chartPage.Height = image.PixelHeight;
                            using (XGraphics gfx = XGraphics.FromPdfPage(chartPage))
                            {

                                gfx.DrawImage(image, 0, 0, chartPage.Width, chartPage.Height);
                            }
                        }
                    }
                    else
                    {
                        logger.Warning($"File not found: {chartPath}");
                    }
                }

                document.Save(pdfPath);
            }
        }

        private void MergePdfs(string statsPdfPath, string chartsPdfPath, string outputPdfPath)
        {
            PdfDocument outputDocument = new PdfDocument();

            using (PdfDocument statsDocument = PdfReader.Open(statsPdfPath, PdfDocumentOpenMode.Import))
            {
                CopyPages(statsDocument, outputDocument);
            }

            using (PdfDocument chartsDocument = PdfReader.Open(chartsPdfPath, PdfDocumentOpenMode.Import))
            {
                CopyPages(chartsDocument, outputDocument);
            }

            outputDocument.Save(outputPdfPath);
        }

        private void CopyPages(PdfDocument from, PdfDocument to)
        {
            for (int i = 0; i < from.PageCount; i++)
            {
                to.AddPage(from.Pages[i]);
            }
        }

        [Obsolete]

        public Result<string> SaveResults(string fileName, string outputDirectory)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            try
            {
                if (!Directory.Exists(outputDirectory))
                {
                    return new Result<string>(false, $"Output directory does not exist: {outputDirectory}");
                }

                string fullPath = Path.Combine(outputDirectory, fileName);
                logger.Information($"Saving results to file {fullPath}");
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                DateTime? from = null;
                DateTime? to = null;

                var donationAmountsByTimeOfDay = manager.AggregateTransactions(from, to, AggregateBy.TimeOfDay, TransactionType.Donation).Data;
                var donationTotalAmountsByTimeOfDay = manager.ApplyAggregationOperation(donationAmountsByTimeOfDay, AggregationOperation.Sum);
                var donationCountsByTimeOfDay = manager.ApplyAggregationOperation(donationAmountsByTimeOfDay, AggregationOperation.Count);

                var withdrawalsByTimeOfDay = manager.AggregateTransactions(from, to, AggregateBy.TimeOfDay, TransactionType.Withdrawal).Data;
                var withdrawalCountByTimeOfDay = manager.ApplyAggregationOperation(withdrawalsByTimeOfDay, AggregationOperation.Count);
                var withdrawalAmountsByTimeOfDay = manager.ApplyAggregationOperation(withdrawalsByTimeOfDay, AggregationOperation.Sum);

                var totalAmountsByHour = manager.AggregateTransactions(from, to, AggregateBy.HourOfDay).Data;
                var totalAmountByHour = manager.ApplyAggregationOperation(totalAmountsByHour, AggregationOperation.Sum);
                var averageAmountByHour = manager.ApplyAggregationOperation(totalAmountsByHour, AggregationOperation.Average);
                var maxAmountByHour = manager.ApplyAggregationOperation(totalAmountsByHour, AggregationOperation.Max);

                var donationCountsByDayOfWeek = manager.AggregateTransactions(from, to, AggregateBy.DayOfWeek, TransactionType.Donation).Data;
                var donationCountByDayOfWeek = manager.ApplyAggregationOperation(donationCountsByDayOfWeek, AggregationOperation.Count);

                var withdrawalsSumByDayOfWeek = manager.AggregateTransactions(from, to, AggregateBy.DayOfWeek, TransactionType.Withdrawal).Data;
                var withdrawalSumByDayOfWeek = manager.ApplyAggregationOperation(withdrawalsSumByDayOfWeek, AggregationOperation.Sum);



                List<string> chartPaths = new List<string>
                {
                    manager.PlotData(donationTotalAmountsByTimeOfDay, "Donation Amounts by Time of Day", ChartType.Pie),
                    manager.PlotData(donationCountsByTimeOfDay, "Donation Counts by Time of Day", ChartType.Line),
                    manager.PlotData(totalAmountByHour, "Total Amounts by Hour of Day", ChartType.Bar),
                    manager.PlotData(averageAmountByHour, "Average Amounts by Hour of Day", ChartType.Bar),
                    manager.PlotData(maxAmountByHour, "Max Donation Amounts by Hour of Day", ChartType.Line),
                    manager.PlotData(donationCountByDayOfWeek, "Donation Count by Day of Week", ChartType.Pie),

                    manager.PlotData(withdrawalCountByTimeOfDay, "Withdrawal Counts by Time of Day", ChartType.Bar),
                    manager.PlotData(withdrawalAmountsByTimeOfDay, "Withdrawal Amounts by Time of Day", ChartType.Line),
                    manager.PlotData(withdrawalSumByDayOfWeek, "Withdrawal Sum by Day of Week", ChartType.Line)
                };
                logger.Information("Charts generated.");

                GenerateStatisticsPdf(
                    donationTotalAmountsByTimeOfDay,
                    donationCountsByTimeOfDay,
                    totalAmountByHour,
                    averageAmountByHour,
                    maxAmountByHour,
                    donationCountByDayOfWeek,
                    withdrawalCountByTimeOfDay,
                    withdrawalAmountsByTimeOfDay,
                    withdrawalSumByDayOfWeek,
                    Path.Combine(outputDirectory, "StatisticsReport.pdf")
                );
                logger.Information("Statistics report generated.");

                GenerateChartsPdf(chartPaths, Path.Combine(outputDirectory, "ChartsReport.pdf"));

                logger.Information("Charts report generated.");

                MergePdfs(
                    Path.Combine(outputDirectory, "StatisticsReport.pdf"),
                    Path.Combine(outputDirectory, "ChartsReport.pdf"),
                    fullPath
                );

                logger.Information($"Final report generated.");

                return new Result<string>(true, $"Final report generated: {fullPath}");
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "An error occurred while generating the report.");
                return new Result<string>(false, $"An error occurred while generating the report: {ex.Message}");
            }
        }
    }
}
