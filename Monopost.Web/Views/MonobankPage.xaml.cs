using Microsoft.Win32;
using Monopost.BLL.Enums;
using Monopost.BLL.Services.Implementations;
using Monopost.Web.Commands;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Monopost.Web.Views
{
    public partial class MonobankPage : Page
    {
        private readonly ObservableCollection<string> selectedFilePaths;
        private DataScienceService dataScienceService;
        private double zoomFactor = 1.0;

        public class TransactionAnalysis
        {
            public Dictionary<string, decimal> DonationTotalAmountsByTimeOfDay { get; set; }
            public Dictionary<string, decimal> DonationCountByTimeOfDay { get; set; }
            public Dictionary<string, decimal> TotalAmountByHour { get; set; }
            public Dictionary<string, decimal> AverageAmountByHour { get; set; }
            public Dictionary<string, decimal> MaxAmountByHour { get; set; }
            public Dictionary<string, decimal> DonationCountByDayOfWeek { get; set; }
            public Dictionary<string, decimal> WithdrawalCountByTimeOfDay { get; set; }
            public Dictionary<string, decimal> WithdrawalSumByTimeOfDay { get; set; }
            public decimal TotalWithdrawalCount { get; set; }
            public decimal TotalDonationCount { get; set; }
            public string[] ChartPaths { get; set; }
        }

        public MonobankPage()
        {
            InitializeComponent();
            DataContext = this;
            selectedFilePaths = new ObservableCollection<string>();
            FileItemsControl.ItemsSource = selectedFilePaths;
            RemoveFileCommand = new RelayCommand(RemoveFile);
        }

        public RelayCommand RemoveFileCommand { get; }

        #region File Operations
        private void ChooseFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var file in openFileDialog.FileNames.Where(file => !selectedFilePaths.Contains(file)))
                {
                    selectedFilePaths.Add(file);
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
        #endregion

        #region Data Analysis
        private TransactionAnalysis AnalyzeTransactions()
        {
            dataScienceService = new DataScienceService();
            var fileTuples = selectedFilePaths.Select(filePath => new Tuple<string, string>(filePath, "")).ToList();
            dataScienceService.LoadFromCSVs(fileTuples);

            DateTime? fromDate = new DateTime(2023, 1, 1);
            DateTime? toDate = DateTime.Now;

            var donationAmountsByTimeOfDay = dataScienceService.AggregateTransactions(fromDate, toDate, AggregateBy.TimeOfDay, TransactionType.Donation);
            var totalAmountsByHour = dataScienceService.AggregateTransactions(fromDate, toDate, AggregateBy.HourOfDay).Data;
            var donationCountsByDayOfWeek = dataScienceService.AggregateTransactions(fromDate, toDate, AggregateBy.DayOfWeek, TransactionType.Donation).Data;
            var withdrawalsByTimeOfDay = dataScienceService.AggregateTransactions(fromDate, toDate, AggregateBy.TimeOfDay, TransactionType.Withdrawal).Data;

            var analysis = new TransactionAnalysis
            {
                DonationTotalAmountsByTimeOfDay = dataScienceService.ApplyAggregationOperation(donationAmountsByTimeOfDay.Data, AggregationOperation.Sum),
                DonationCountByTimeOfDay = dataScienceService.ApplyAggregationOperation(donationAmountsByTimeOfDay.Data, AggregationOperation.Count),
                TotalAmountByHour = dataScienceService.ApplyAggregationOperation(totalAmountsByHour, AggregationOperation.Sum),
                AverageAmountByHour = dataScienceService.ApplyAggregationOperation(totalAmountsByHour, AggregationOperation.Average),
                MaxAmountByHour = dataScienceService.ApplyAggregationOperation(totalAmountsByHour, AggregationOperation.Max),
                DonationCountByDayOfWeek = dataScienceService.ApplyAggregationOperation(donationCountsByDayOfWeek, AggregationOperation.Count),
                WithdrawalCountByTimeOfDay = dataScienceService.ApplyAggregationOperation(withdrawalsByTimeOfDay, AggregationOperation.Count),
                WithdrawalSumByTimeOfDay = dataScienceService.ApplyAggregationOperation(withdrawalsByTimeOfDay, AggregationOperation.Sum)
            };

            analysis.TotalWithdrawalCount = analysis.WithdrawalCountByTimeOfDay.Values.Sum();
            analysis.TotalDonationCount = analysis.DonationCountByTimeOfDay.Values.Sum();

            analysis.ChartPaths = GenerateCharts(analysis);

            return analysis;
        }

        private string[] GenerateCharts(TransactionAnalysis analysis)
        {
            return new[]
            {
                dataScienceService.PlotData(analysis.DonationTotalAmountsByTimeOfDay, "Donation Amounts by Time of Day (Pie Chart)", ChartType.Pie),
                dataScienceService.PlotData(analysis.DonationCountByTimeOfDay, "Donation Counts by Time of Day", ChartType.Bar),
                dataScienceService.PlotData(analysis.AverageAmountByHour, "Average Amounts by Hour of Day", ChartType.Line),
                dataScienceService.PlotData(analysis.TotalAmountByHour, "Total Amounts by Hour", ChartType.Bar),
                dataScienceService.PlotData(analysis.DonationCountByDayOfWeek, "Donation count by day of week", ChartType.Bar)
            };
        }
        #endregion

        #region UI Display
        private void OnGetStatisticsClicked(object sender, RoutedEventArgs e)
        {
            if (selectedFilePaths.Count == 0)
            {
                MessageBox.Show("Please select at least one CSV file.");
                return;
            }

            ResetChartAndStats();
            var analysis = AnalyzeTransactions();
            DisplayAnalysisResults(analysis);

            FileItemsControl.Visibility = Visibility.Collapsed;
            SaveStatsButton.Visibility = Visibility.Visible;
            ClearFormButton.Visibility = Visibility.Visible;


        }

        private void DisplayAnalysisResults(TransactionAnalysis analysis)
        {
            var mainPanel = CreateMainPanel();
            AddTitle(mainPanel, "Donation Report");

            AddStatisticsSection(mainPanel, "Donation Amounts by Time of Day:", analysis.DonationTotalAmountsByTimeOfDay);
            AddStatisticsSection(mainPanel, "Donation Counts by Time of Day:", analysis.DonationCountByTimeOfDay);
            AddStatisticsSection(mainPanel, "Total Amounts by Hour of Day:", analysis.TotalAmountByHour);
            AddStatisticsSection(mainPanel, "Average Amounts by Hour of Day:", analysis.AverageAmountByHour);
            AddStatisticsSection(mainPanel, "Donation Counts by Day of Week:", analysis.DonationCountByDayOfWeek);

            AddTotalCount(mainPanel, "Total Withdrawal Count:", analysis.TotalWithdrawalCount);
            AddTotalCount(mainPanel, "Total Donation Count:", analysis.TotalDonationCount);

            AddSeparator(mainPanel);

            foreach (var chartPath in analysis.ChartPaths.Where(path => !string.IsNullOrEmpty(path) && File.Exists(path)))
            {
                AddChartToPanel(mainPanel, chartPath);
            }

            ChartsStackPanel.Children.Add(new ScrollViewer
            {
                Content = mainPanel,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            });
        }

        private StackPanel CreateMainPanel() => new StackPanel { Margin = new Thickness(10) };

        private void AddTitle(StackPanel panel, string title)
        {
            panel.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20),
                HorizontalAlignment = HorizontalAlignment.Center
            });
        }

        private void AddStatisticsSection(StackPanel panel, string title, Dictionary<string, decimal> data)
        {
            panel.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 10, 0, 5)
            });

            foreach (var item in data.OrderBy(x => x.Key))
            {
                panel.Children.Add(new TextBlock
                {
                    Text = $"{item.Key}: {item.Value:N2}",
                    Margin = new Thickness(20, 2, 0, 2),
                    FontSize = 14
                });
            }
        }

        private void AddTotalCount(StackPanel panel, string title, decimal value)
        {
            panel.Children.Add(new TextBlock
            {
                Text = $"{title} {value:N0}",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 10, 0, 5)
            });
        }

        private void AddSeparator(StackPanel panel)
        {
            panel.Children.Add(new Separator
            {
                Margin = new Thickness(0, 20, 0, 20),
                Height = 2,
                Background = Brushes.Gray
            });
        }

        private void AddChartToPanel(StackPanel panel, string chartPath)
        {
            var image = new Image
            {
                Source = new BitmapImage(new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, chartPath))),
                Margin = new Thickness(0, 10, 0, 10),
                Width = 600,
                Height = 450,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            image.MouseLeftButtonDown += OnImageClick;
            panel.Children.Add(image);
        }
        #endregion

        #region PDF Generation
        private void SaveStatsToPdf_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFilePaths.Count == 0)
            {
                MessageBox.Show("Please select at least one CSV file.");
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                DefaultExt = "pdf",
                FileName = "DonationReport.pdf",
                Title = "Save Statistics Report"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var analysis = AnalyzeTransactions();
                    GeneratePdf(analysis, saveFileDialog.FileName);
                    MessageBox.Show("PDF saved successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error generating PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void GeneratePdf(TransactionAnalysis analysis, string fullPath)
        {
            using var document = new PdfDocument();
            document.Info.Title = "Detailed Donation Report";

            AddStatisticsPage(document, analysis);
            AddChartPages(document, analysis.ChartPaths);

            document.Save(fullPath);
        }

        private void AddStatisticsPage(PdfDocument document, TransactionAnalysis analysis)
        {
            var page = document.AddPage();
            page.Width = 800;
            page.Height = 1800;

            using var gfx = XGraphics.FromPdfPage(page);
            var yPosition = 40.0;

            WritePdfTitle(gfx, "Donation Report", ref yPosition);
            WritePdfSection(gfx, "Donation Amounts by Time of Day:", analysis.DonationTotalAmountsByTimeOfDay, ref yPosition);
            WritePdfSection(gfx, "Donation Counts by Time of Day:", analysis.DonationCountByTimeOfDay, ref yPosition);
            WritePdfSection(gfx, "Average Amounts by Hour of Day:", analysis.AverageAmountByHour, ref yPosition);
            WritePdfSection(gfx, "Total Amounts by Hour of Day:", analysis.TotalAmountByHour, ref yPosition);
            WritePdfSection(gfx, "Donation Counts by Day of Week:", analysis.DonationCountByDayOfWeek, ref yPosition);

            WritePdfTotalCount(gfx, "Total Withdrawal Count:", analysis.TotalWithdrawalCount, ref yPosition);
            WritePdfTotalCount(gfx, "Total Donation Count:", analysis.TotalDonationCount, ref yPosition);
        }

        private void WritePdfTitle(XGraphics gfx, string text, ref double yPosition)
        {
            var font = new XFont("Verdana", 16, XFontStyle.Bold);
            gfx.DrawString(text, font, XBrushes.Black, new XPoint(40, yPosition));
            yPosition += 40;
        }

        private void WritePdfSection(XGraphics gfx, string title, Dictionary<string, decimal> data, ref double yPosition)
        {
            var titleFont = new XFont("Verdana", 14, XFontStyle.Bold);
            var contentFont = new XFont("Verdana", 12, XFontStyle.Regular);

            gfx.DrawString(title, titleFont, XBrushes.Black, new XPoint(40, yPosition));
            yPosition += 30;

            foreach (var entry in data)
            {
                gfx.DrawString($"{entry.Key}: {entry.Value:N2}", contentFont, XBrushes.Black, new XPoint(40, yPosition));
                yPosition += 20;
            }
            yPosition += 30;
        }

        private void WritePdfTotalCount(XGraphics gfx, string title, decimal value, ref double yPosition)
        {
            var font = new XFont("Verdana", 14, XFontStyle.Bold);
            gfx.DrawString($"{title} {value}", font, XBrushes.Black, new XPoint(40, yPosition));
            yPosition += 30;
        }

        private void AddChartPages(PdfDocument document, string[] chartPaths)
        {
            foreach (var chartPath in chartPaths.Where(path => !string.IsNullOrEmpty(path) && File.Exists(path)))
            {
                using var image = XImage.FromFile(chartPath);
                var page = document.AddPage();
                page.Width = image.PixelWidth;
                page.Height = image.PixelHeight;

                using var gfx = XGraphics.FromPdfPage(page);
                gfx.DrawImage(image, 0, 0, page.Width, page.Height);
            }
        }
        #endregion

        #region Image Interaction
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
        private void ClearFormButton_Click(object sender, RoutedEventArgs e)
        {
            selectedFilePaths.Clear();

            ResetChartAndStats();

            FileItemsControl.Visibility = Visibility.Visible;

            SaveStatsButton.Visibility = Visibility.Collapsed;
            ClearFormButton.Visibility = Visibility.Collapsed;
            ChooseCSVButton.Visibility = Visibility.Visible;
            GetStatisticsButton.Visibility = Visibility.Visible;
            EnterAPI.Visibility = Visibility.Visible;
            OrTextBlock.Visibility = Visibility.Visible;
        }


        private void OnPopupClose(object sender, MouseButtonEventArgs e)
        {
            ImagePopup.IsOpen = false;
            ResetZoom();
        }

        private void ZoomedImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            const double zoomDelta = 1.1;
            zoomFactor *= e.Delta > 0 ? zoomDelta : 1 / zoomDelta;
            ApplyZoom();
        }

        private void ApplyZoom()
        {
            ZoomedImage.RenderTransform = new ScaleTransform(zoomFactor, zoomFactor);
        }

        private void ResetZoom()
        {
            zoomFactor = 1.0;
            ApplyZoom();
        }
        #endregion

        private void ResetChartAndStats()
        {
            ChartsStackPanel.Children.Clear();
            FileItemsControl.Visibility = Visibility.Visible;
            SaveStatsButton.Visibility = Visibility.Collapsed;
            ResetZoom();
        }
    }
}
