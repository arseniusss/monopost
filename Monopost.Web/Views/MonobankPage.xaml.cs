using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Monopost.BLL.Services.Implementations;
using Monopost.BLL.Enums;
using Monopost.Web.Commands;
using System.Windows.Media;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace Monopost.Web.Views
{
    public partial class MonobankPage : Page
    {
        private ObservableCollection<string> selectedFilePaths;
        private readonly DataScienceService? manager = new DataScienceService();

        private DataScienceService dataScienceService;
        public MonobankPage()
        {
            InitializeComponent();
            DataContext = this;
            selectedFilePaths = new ObservableCollection<string>();
            FileItemsControl.ItemsSource = selectedFilePaths;

            RemoveFileCommand = new RelayCommand(RemoveFile);
        }

        public RelayCommand RemoveFileCommand { get; }

        private void ChooseFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var file in openFileDialog.FileNames)
                {
                    if (!selectedFilePaths.Contains(file))
                    {
                        selectedFilePaths.Add(file);
                    }
                }
            }
        }

        private void RemoveFile(object parameter)
        {
            if (parameter is string filePath && selectedFilePaths.Contains(filePath))
            {
                selectedFilePaths.Remove(filePath);
            }
        }

        private void OnGetStatisticsClicked(object sender, RoutedEventArgs e)
        {
            if (selectedFilePaths.Count == 0)
            {
                MessageBox.Show("Please select at least one CSV file.");
                return;
            }

            ResetChartAndStats();

            dataScienceService = new DataScienceService();
            List<Tuple<string, string>> fileTuples = selectedFilePaths
                .Select(filePath => new Tuple<string, string>(filePath, ""))
                .ToList();

            dataScienceService.LoadFromCSVs(fileTuples);

            var mainPanel = new StackPanel { Margin = new Thickness(10) };
            var mainScroll = new ScrollViewer
            {
                Content = mainPanel,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            DateTime? fromDate = new DateTime(2023, 1, 1);
            DateTime? toDate = DateTime.Now;

            var donationAmountsByTimeOfDay = dataScienceService.AggregateTransactions(fromDate, toDate, AggregateBy.TimeOfDay, TransactionType.Donation);
            var donationTotalAmountsByTimeOfDay = dataScienceService.ApplyAggregationOperation(donationAmountsByTimeOfDay.Data, AggregationOperation.Sum);
            var donationCountByTimeOfDay = dataScienceService.ApplyAggregationOperation(donationAmountsByTimeOfDay.Data, AggregationOperation.Count);

            var totalAmountsByHour = dataScienceService.AggregateTransactions(fromDate, toDate, AggregateBy.HourOfDay).Data;
            var totalAmountByHour = dataScienceService.ApplyAggregationOperation(totalAmountsByHour, AggregationOperation.Sum);
            var averageAmountByHour = dataScienceService.ApplyAggregationOperation(totalAmountsByHour, AggregationOperation.Average);
            var maxAmountByHour = dataScienceService.ApplyAggregationOperation(totalAmountsByHour, AggregationOperation.Max);

            var donationCountsByDayOfWeek = dataScienceService.AggregateTransactions(fromDate, toDate, AggregateBy.DayOfWeek, TransactionType.Donation).Data;
            var donationCountByDayOfWeek = dataScienceService.ApplyAggregationOperation(donationCountsByDayOfWeek, AggregationOperation.Count);

            var withdrawalsByTimeOfDay = dataScienceService.AggregateTransactions(fromDate, toDate, AggregateBy.TimeOfDay, TransactionType.Withdrawal).Data;
            var withdrawalCountByTimeOfDay = dataScienceService.ApplyAggregationOperation(withdrawalsByTimeOfDay, AggregationOperation.Count);
            var withdrawalSumByTimeOfDay = dataScienceService.ApplyAggregationOperation(withdrawalsByTimeOfDay, AggregationOperation.Sum);

            var withdrawalsSumByDayOfWeek = dataScienceService.AggregateTransactions(fromDate, toDate, AggregateBy.DayOfWeek, TransactionType.Withdrawal).Data;
            var withdrawalSumByDayOfWeek = dataScienceService.ApplyAggregationOperation(withdrawalsSumByDayOfWeek, AggregationOperation.Sum);


            var titleBlock = new TextBlock
            {
                Text = "Donation Report",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            mainPanel.Children.Add(titleBlock);


            AddStatisticsSection(mainPanel, "Donation Amounts by Time of Day:", donationTotalAmountsByTimeOfDay);
            AddStatisticsSection(mainPanel, "Donation Counts by Time of Day:", donationCountByTimeOfDay);
            AddStatisticsSection(mainPanel, "Total Amounts by Hour of Day:", totalAmountByHour);
            AddStatisticsSection(mainPanel, "Average Amounts by Hour of Day:", averageAmountByHour);
            AddStatisticsSection(mainPanel, "Max Donation Amounts by Hour of Day:", maxAmountByHour);
            AddStatisticsSection(mainPanel, "Donation Counts by Day of Week:", donationCountByDayOfWeek);
            //AddStatisticsSection(mainPanel, "Withdrawal Counts by Time of Day:", withdrawalCountByTimeOfDay);
            //AddStatisticsSection(mainPanel, "Withdrawal Amounts by Time of Day:", withdrawalSumByTimeOfDay);
            //AddStatisticsSection(mainPanel, "Withdrawal Sum by Day of Week:", withdrawalSumByDayOfWeek);


            var totalWithdrawalCount = withdrawalCountByTimeOfDay.Values.Sum();
            var totalDonationCount = donationCountByTimeOfDay.Values.Sum();

            AddTotalCount(mainPanel, "Total Withdrawal Count:", totalWithdrawalCount);
            AddTotalCount(mainPanel, "Total Donation Count:", totalDonationCount);


            mainPanel.Children.Add(new Separator
            {
                Margin = new Thickness(0, 20, 0, 20),
                Height = 2,
                Background = Brushes.Gray
            });


            var donationDataByTime = donationAmountsByTimeOfDay.Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);
            var withdrawalDataByTime = withdrawalsByTimeOfDay.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);

            var charts = new[]
            {
              dataScienceService.PlotData(donationTotalAmountsByTimeOfDay, "Donation Amounts by Time of Day (Pie Chart)", ChartType.Pie),
              dataScienceService.PlotData(donationCountByTimeOfDay, "Donation Counts by Time of Day", ChartType.Bar),
              dataScienceService.PlotData(averageAmountByHour, "Average Amounts by Hour of Day", ChartType.Line),
              dataScienceService.PlotData(totalAmountByHour, "Total Amounts by Hour", ChartType.Bar),
              dataScienceService.PlotData(donationCountByDayOfWeek, "Donation count by day of weeek", ChartType.Bar)
               //dataScienceService.PlotData(withdrawalDataByTime, "Withdrawal Transactions by Month", ChartType.Bar)
              };

            foreach (var chartPath in charts.Where(path => !string.IsNullOrEmpty(path) && File.Exists(path)))
            {
                AddChartToPanel(mainPanel, chartPath);
            }

            ChartsStackPanel.Children.Add(mainScroll);

            FileItemsControl.Visibility = Visibility.Collapsed;
            SaveStatsButton.Visibility = Visibility.Visible;
        }

        private void AddStatisticsSection(StackPanel panel, string title, Dictionary<string, decimal> data)
        {
            var titleBlock = new TextBlock
            {
                Text = title,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 10, 0, 5)
            };
            panel.Children.Add(titleBlock);

            foreach (var item in data.OrderBy(x => x.Key))
            {
                var valueBlock = new TextBlock
                {
                    Text = $"{item.Key}: {item.Value:N2}",
                    Margin = new Thickness(20, 2, 0, 2),
                    FontSize = 14
                };
                panel.Children.Add(valueBlock);
            }
        }

        private void AddTotalCount(StackPanel panel, string title, decimal value)
        {
            var totalBlock = new TextBlock
            {
                Text = $"{title} {value:N0}",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 10, 0, 5)
            };
            panel.Children.Add(totalBlock);
        }

        private void AddChartToPanel(StackPanel panel, string chartPath)
        {
            var chartUri = new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, chartPath));
            var chartImage = new Image
            {
                Source = new BitmapImage(chartUri),
                Margin = new Thickness(0, 10, 0, 10),
                Width = 600,
                Height = 450,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            chartImage.MouseLeftButtonDown += OnImageClick;
            panel.Children.Add(chartImage);
        }

        // private void SaveStatsToPdf_Click(object sender, RoutedEventArgs e)
        // {
        //     if (selectedFilePaths.Count == 0)
        //     {
        //         MessageBox.Show("Please select at least one CSV file.");
        //         return;
        //     }
        // 
        //     SaveFileDialog saveFileDialog = new SaveFileDialog
        //     {
        //         Filter = "PDF Files (*.pdf)|*.pdf",
        //         DefaultExt = "pdf",
        //         FileName = "",
        //         AddExtension = true,
        //         Title = "Save Statistics Report"
        //     };
        // 
        //     if (saveFileDialog.ShowDialog() == true)
        //     {
        //         try
        //         {
        //             string outputDirectory = Path.GetDirectoryName(saveFileDialog.FileName);
        //             string fileName = Path.GetFileName(saveFileDialog.FileName);
        // 
        //             
        //             var pdfService = new DataScienceSavingPdfService(selectedFilePaths[0]);
        // 
        //            
        //             List<Tuple<string, string>> fileTuples = selectedFilePaths
        //                 .Select(filePath => new Tuple<string, string>(filePath, ""))
        //                 .ToList();
        // 
        //             // Завантажуємо всі файли в manager сервісу
        //             pdfService.manager.LoadFromCSVs(fileTuples);
        // 
        //             var result = pdfService.SaveResults(fileName, outputDirectory);
        // 
        //             if (result.Success)
        //             {
        //                 MessageBox.Show(
        //                     "PDF saved successfully",
        //                     "Success",
        //                     MessageBoxButton.OK,
        //                     MessageBoxImage.Information);
        //             }
        //             else
        //             {
        //                 MessageBox.Show(
        //                     $"Error saving PDF: {result.Message}",
        //                     "Error",
        //                     MessageBoxButton.OK,
        //                     MessageBoxImage.Error);
        //             }
        //         }
        //         catch (Exception ex)
        //         {
        //             MessageBox.Show(
        //                 $"Error generating PDF: {ex.Message}",
        //                 "Error",
        //                 MessageBoxButton.OK,
        //                 MessageBoxImage.Error);
        //         }
        //     }
        // }


        //бидлокод(робоча версія)
        private void SaveStatsToPdf_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFilePaths.Count == 0)
            {
                MessageBox.Show("Please select at least one CSV file.");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                DefaultExt = "pdf",
                FileName = "",
                AddExtension = true,
                Title = "Save Statistics Report"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string outputDirectory = Path.GetDirectoryName(saveFileDialog.FileName);
                    string fileName = Path.GetFileName(saveFileDialog.FileName);
                    string fullPath = Path.Combine(outputDirectory, fileName);

                    var dataScienceService = new DataScienceService();
                    List<Tuple<string, string>> fileTuples = selectedFilePaths
                        .Select(filePath => new Tuple<string, string>(filePath, ""))
                        .ToList();

                    dataScienceService.LoadFromCSVs(fileTuples);

                    DateTime? fromDate = new DateTime(2023, 1, 1);
                    DateTime? toDate = DateTime.Now;

                    var donationAmountsByTimeOfDay = dataScienceService.AggregateTransactions(fromDate, toDate, AggregateBy.TimeOfDay, TransactionType.Donation);
                    var donationTotalAmountsByTimeOfDay = dataScienceService.ApplyAggregationOperation(donationAmountsByTimeOfDay.Data, AggregationOperation.Sum);
                    var donationCountByTimeOfDay = dataScienceService.ApplyAggregationOperation(donationAmountsByTimeOfDay.Data, AggregationOperation.Count);

                    var totalAmountsByHour = dataScienceService.AggregateTransactions(fromDate, toDate, AggregateBy.HourOfDay).Data;
                    var totalAmountByHour = dataScienceService.ApplyAggregationOperation(totalAmountsByHour, AggregationOperation.Sum);
                    var averageAmountByHour = dataScienceService.ApplyAggregationOperation(totalAmountsByHour, AggregationOperation.Average);
                    var maxAmountByHour = dataScienceService.ApplyAggregationOperation(totalAmountsByHour, AggregationOperation.Max);

                    var donationCountsByDayOfWeek = dataScienceService.AggregateTransactions(fromDate, toDate, AggregateBy.DayOfWeek, TransactionType.Donation).Data;
                    var donationCountByDayOfWeek = dataScienceService.ApplyAggregationOperation(donationCountsByDayOfWeek, AggregationOperation.Count);

                    var withdrawalsByTimeOfDay = dataScienceService.AggregateTransactions(fromDate, toDate, AggregateBy.TimeOfDay, TransactionType.Withdrawal).Data;
                    var withdrawalCountByTimeOfDay = dataScienceService.ApplyAggregationOperation(withdrawalsByTimeOfDay, AggregationOperation.Count);
                    var withdrawalSumByTimeOfDay = dataScienceService.ApplyAggregationOperation(withdrawalsByTimeOfDay, AggregationOperation.Sum);

                    var withdrawalsSumByDayOfWeek = dataScienceService.AggregateTransactions(fromDate, toDate, AggregateBy.DayOfWeek, TransactionType.Withdrawal).Data;
                    var withdrawalSumByDayOfWeek = dataScienceService.ApplyAggregationOperation(withdrawalsSumByDayOfWeek, AggregationOperation.Sum);


                    using (PdfDocument document = new PdfDocument())
                    {
                        document.Info.Title = "Detailed Donation Report";

                        PdfPage statsPage = document.AddPage();
                        statsPage.Width = 800;
                        statsPage.Height = 1800;

                        using (XGraphics gfx = XGraphics.FromPdfPage(statsPage))
                        {
                            XFont titleFont = new XFont("Verdana", 16, XFontStyle.Bold);
                            XFont sectionFont = new XFont("Verdana", 14, XFontStyle.Bold);
                            XFont contentFont = new XFont("Verdana", 12, XFontStyle.Regular);

                            double yOffset = 40;

                            void WriteSection(string title, Dictionary<string, decimal> data, ref double yOffset)
                            {
                                gfx.DrawString(title, sectionFont, XBrushes.Black, new XPoint(40, yOffset));
                                yOffset += 30;
                                foreach (var entry in data)
                                {
                                    gfx.DrawString($"{entry.Key}: {entry.Value:N2}", contentFont, XBrushes.Black, new XPoint(40, yOffset));
                                    yOffset += 20;
                                }
                                yOffset += 30;
                            }

                            gfx.DrawString("Donation Report", titleFont, XBrushes.Black, new XPoint(40, yOffset));
                            yOffset += 40;

                            WriteSection("Donation Amounts by Time of Day:", donationTotalAmountsByTimeOfDay, ref yOffset);
                            WriteSection("Donation Counts by Time of Day:", donationCountByTimeOfDay, ref yOffset);
                            WriteSection("Average Amounts by Hour of Day:", averageAmountByHour, ref yOffset);
                            WriteSection("Total Amounts by Hour of Day:", totalAmountByHour, ref yOffset);
                            WriteSection("Donation Counts by Day of Week:", donationCountByDayOfWeek, ref yOffset);
                            //WriteSection("Withdrawal Counts by Time of Day:", withdrawalCountByTimeOfDay, ref yOffset);
                            //WriteSection("Withdrawal Amounts by Time of Day:", withdrawalSumByTimeOfDay, ref yOffset);
                            //WriteSection("Withdrawal Sum by Day of Week:", withdrawalSumByDayOfWeek, ref yOffset);

                            var totalWithdrawalCount = withdrawalCountByTimeOfDay.Values.Sum();
                            var totalDonationCount = donationCountByTimeOfDay.Values.Sum();

                            gfx.DrawString($"Total Withdrawal Count: {totalWithdrawalCount}", sectionFont, XBrushes.Black, new XPoint(40, yOffset));
                            yOffset += 30;
                            gfx.DrawString($"Total Donation Count: {totalDonationCount}", sectionFont, XBrushes.Black, new XPoint(40, yOffset));
                        }

                        var charts = new[]
                        {
                         dataScienceService.PlotData(donationTotalAmountsByTimeOfDay, "Donation Amounts by Time of Day (Pie Chart)", ChartType.Pie),
                         dataScienceService.PlotData(donationCountByTimeOfDay, "Donation Counts by Time of Day", ChartType.Bar),
                         dataScienceService.PlotData(averageAmountByHour, "Average Amounts by Hour of Day", ChartType.Line),
                         dataScienceService.PlotData(totalAmountByHour, "Total Amounts by Hour", ChartType.Bar),
                         dataScienceService.PlotData(donationCountByDayOfWeek, "Donation count by day of weeek", ChartType.Bar)
                          //dataScienceService.PlotData(withdrawalDataByTime, "Withdrawal Transactions by Month", ChartType.Bar)
                         };

                        
                        foreach (var chartPath in charts.Where(path => !string.IsNullOrEmpty(path) && File.Exists(path)))
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

                        document.Save(fullPath);
                    }

                    MessageBox.Show(
                        "PDF saved successfully",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error generating PDF: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }






        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //уже Оніщука


        // private void OnGetStatisticsClicked(object sender, RoutedEventArgs e)
        // {
        //     if (selectedFilePaths.Count == 0)
        //     {
        //         MessageBox.Show("Please select at least one CSV file.");
        //         return;
        //     }
        //
        //     ResetChartAndStats();
        //
        //     
        //     var dataScienceService = new DataScienceService();
        //     List<Tuple<string, string>> fileTuples = selectedFilePaths
        //         .Select(filePath => new Tuple<string, string>(filePath, ""))
        //         .ToList();
        //
        //    
        //     dataScienceService.LoadFromCSVs(fileTuples);
        //     DisplayAllStatistics(dataScienceService);
        //     FileItemsControl.Visibility = Visibility.Collapsed;
        //     SaveStatsButton.Visibility = Visibility.Visible;
        //
        // }

        // private void DisplayAllStatistics(DataScienceService dataScienceService)
        // {
        //     DateTime? fromDate = new DateTime(2023, 1, 1);
        //     DateTime? toDate = DateTime.Now;
        //
        //     var donationAmounts = dataScienceService.AggregateTransactions(fromDate, toDate, AggregateBy.Month, TransactionType.Donation);
        //     if (!donationAmounts.Success)
        //     {
        //         MessageBox.Show($"Error: {donationAmounts.Message}");
        //         return;
        //     }
        //
        //
        //     var donationData = donationAmounts.Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);
        //     var withdrawalAmounts = dataScienceService.AggregateTransactions(fromDate, toDate, AggregateBy.Month, TransactionType.Withdrawal);
        //     var withdrawalData = withdrawalAmounts.Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);
        //
        //     var donationAmountsByTimeOfDay = dataScienceService.AggregateTransactions(fromDate, toDate, AggregateBy.TimeOfDay, TransactionType.Donation);
        //     var donationDataByTime = donationAmountsByTimeOfDay.Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);
        //
        //     var withdrawalAmountsByTimeOfDay = dataScienceService.AggregateTransactions(fromDate, toDate, AggregateBy.TimeOfDay, TransactionType.Withdrawal);
        //     var withdrawalDataByTime = withdrawalAmountsByTimeOfDay.Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);
        //
        //     string withdrawalChartByTimePath = dataScienceService.PlotData(withdrawalDataByTime, "Withdrawal Transactions by Time of Day", ChartType.Line);
        //     var donationAmountsByHour = dataScienceService.AggregateTransactions(fromDate, toDate, AggregateBy.HourOfDay, TransactionType.Donation);
        //     var donationDataByHour = donationAmountsByHour.Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);
        //
        //     var donationAmountsByTime = dataScienceService.AggregateTransactions(fromDate, toDate, AggregateBy.TimeOfDay, TransactionType.Donation);
        //     var donationDataByTimePath = donationAmountsByTime.Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);
        //
        //     string donationChartByTimePath = dataScienceService.PlotData(donationDataByTimePath, "Donation Counts by Time of Day", ChartType.Line);
        //     string donationChartByHourPath = dataScienceService.PlotData(donationDataByHour, "Donation Transactions by Hour", ChartType.Bar);
        //
        //     var withdrawalAmountsByHour = dataScienceService.AggregateTransactions(fromDate, toDate, AggregateBy.HourOfDay, TransactionType.Withdrawal);
        //     var withdrawalDataByHour = withdrawalAmountsByHour.Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);
        //
        //     string withdrawalChartByHourPath = dataScienceService.PlotData(withdrawalDataByHour, "Withdrawal Transactions by Hour", ChartType.Bar);
        //
        //     string donationByTimeofDayChart = dataScienceService.PlotData(donationDataByTime, "Donation Amounts by Time of Day (Pie Chart)", ChartType.Pie);
        //     string donationChartPath = dataScienceService.PlotData(donationData, "Donation Transactions by Month", ChartType.Bar);
        //     string withdrawalChartPath = dataScienceService.PlotData(withdrawalData, "Withdrawal Transactions by Month", ChartType.Bar);
        //
        //
        //     AddChartToUI(donationByTimeofDayChart);
        //
        //     //AddChartToUI(donationChartByTimePath); - saving in pdf is not working with this line of code (at least for me)
        //
        //     AddChartToUI(donationChartPath);
        //     AddChartToUI(withdrawalChartPath);
        //     AddChartToUI(withdrawalChartByTimePath);
        //     AddChartToUI(donationChartByHourPath);
        //     AddChartToUI(withdrawalChartByHourPath);
        //
        // }

        //  private void AddTextToUI(string text)
        //  {
        //      if (string.IsNullOrEmpty(text))
        //      {
        //          MessageBox.Show("Text is empty or null.");
        //          return;
        //      }
        //
        //      TextBlock textBlock = new TextBlock
        //      {
        //          Text = text,
        //          Margin = new Thickness(0, 10, 0, 10),
        //          FontSize = 14,
        //          FontWeight = FontWeights.Bold, 
        //          TextAlignment = TextAlignment.Center 
        //      };
        //
        //      ChartsStackPanel.Children.Add(textBlock);
        //  }
        //
        //  private void AddChartToUI(string chartFilePath)
        //  {
        //      if (string.IsNullOrEmpty(chartFilePath) || !File.Exists(chartFilePath))
        //      {
        //          MessageBox.Show("Chart file not found or could not be generated.");
        //          return;
        //      }
        //
        //      Uri chartUri = new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, chartFilePath));
        //
        //      Image chartImage = new Image
        //      {
        //          Source = new BitmapImage(chartUri),
        //          Margin = new Thickness(0, 10, 0, 10),
        //          Width = 600,
        //          Height = 450
        //      };
        //
        //      ChartsStackPanel.Children.Add(chartImage);
        //  }

        // private void SaveStatsToPdf_Click(object sender, RoutedEventArgs e)
        // {
        //     if (selectedFilePaths.Count == 0)
        //     {
        //         MessageBox.Show("Please select at least one CSV file.");
        //         return;
        //     }
        //
        //     string outputDirectory = @"C:\Users\Laptopchik\Desktop\monopost";
        //
        //     try
        //     {
        //         string selectedFile = selectedFilePaths[0];
        //
        //         var pdfService = new DataScienceSavingPdfService(selectedFile);
        //
        //         var result = pdfService.SaveResults("StatisticsReport.pdf", outputDirectory);
        //
        //         if (result.Success)
        //         {
        //             MessageBox.Show($"PDF saved successfully {result.Data}");
        //         }
        //         else
        //         {
        //             MessageBox.Show($"Error saving PDF: {result.Message}");
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         MessageBox.Show($"Error generating PDF: {ex.Message}");
        //     }
        // }

        private double zoomFactor = 1.0;

        private void OnImageClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Image clickedImage)
            {
                ZoomedImage.Source = clickedImage.Source;

                ZoomedImage.Width = clickedImage.ActualWidth * 2;
                ZoomedImage.Height = clickedImage.ActualHeight * 2;

                ImagePopup.IsOpen = true;
            }
        }

        private void OnPopupClose(object sender, MouseButtonEventArgs e)
        {
            ImagePopup.IsOpen = false;
        }

        private void ZoomedImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                zoomFactor *= 1.1;
            }
            else if (e.Delta < 0)
            {
                zoomFactor /= 1.1;
            }

            ZoomedImage.RenderTransform = new ScaleTransform(zoomFactor, zoomFactor);
        }

        private void ResetChartAndStats()
        {
            ChartsStackPanel.Children.Clear();

            FileItemsControl.Visibility = Visibility.Visible;
            SaveStatsButton.Visibility = Visibility.Collapsed;
        }
    }
}
