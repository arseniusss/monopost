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

namespace Monopost.Web.Views
{
    public partial class MonobankPage : Page
    {
        private ObservableCollection<string> selectedFilePaths;
        private readonly DataScienceService? manager = new DataScienceService();

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

            foreach (var filePath in selectedFilePaths)
            {
                var dataScienceService = new DataScienceService();
                dataScienceService.LoadFromCsv(filePath);
                DisplayAllStatistics(dataScienceService, filePath);
            }

            FileItemsControl.Visibility = Visibility.Collapsed;
            SaveStatsButton.Visibility = Visibility.Visible;
        }


        private void DisplayAllStatistics(DataScienceService dataScienceService, string filePath)
        {
            DateTime? fromDate = new DateTime(2023, 1, 1);
            DateTime? toDate = DateTime.Now;

            var donationAmounts = dataScienceService.AggregateTransactions(fromDate, toDate, AggregateBy.Month, TransactionType.Donation);
            if (!donationAmounts.Success)
            {
                MessageBox.Show($"Error: {donationAmounts.Message}");
                return;
            }


            var donationData = donationAmounts.Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);
            var withdrawalAmounts = dataScienceService.AggregateTransactions(fromDate, toDate, AggregateBy.Month, TransactionType.Withdrawal);
            var withdrawalData = withdrawalAmounts.Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);

            var donationAmountsByTimeOfDay = dataScienceService.AggregateTransactions(fromDate, toDate, AggregateBy.TimeOfDay, TransactionType.Donation);
            var donationDataByTime = donationAmountsByTimeOfDay.Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);

            var withdrawalAmountsByTimeOfDay = dataScienceService.AggregateTransactions(fromDate, toDate, AggregateBy.TimeOfDay, TransactionType.Withdrawal);
            var withdrawalDataByTime = withdrawalAmountsByTimeOfDay.Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);

            string withdrawalChartByTimePath = dataScienceService.PlotData(withdrawalDataByTime, "Withdrawal Transactions by Time of Day", ChartType.Line);
            var donationAmountsByHour = dataScienceService.AggregateTransactions(fromDate, toDate, AggregateBy.HourOfDay, TransactionType.Donation);
            var donationDataByHour = donationAmountsByHour.Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);

            var donationAmountsByTime = dataScienceService.AggregateTransactions(fromDate, toDate, AggregateBy.TimeOfDay, TransactionType.Donation);
            var donationDataByTimePath = donationAmountsByTime.Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);

            string donationChartByTimePath = dataScienceService.PlotData(donationDataByTimePath, "Donation Counts by Time of Day", ChartType.Line);
            string donationChartByHourPath = dataScienceService.PlotData(donationDataByHour, "Donation Transactions by Hour", ChartType.Bar);

            var withdrawalAmountsByHour = dataScienceService.AggregateTransactions(fromDate, toDate, AggregateBy.HourOfDay, TransactionType.Withdrawal);
            var withdrawalDataByHour = withdrawalAmountsByHour.Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);

            string withdrawalChartByHourPath = dataScienceService.PlotData(withdrawalDataByHour, "Withdrawal Transactions by Hour", ChartType.Bar);

            string donationByTimeofDayChart = dataScienceService.PlotData(donationDataByTime, "Donation Amounts by Time of Day (Pie Chart)", ChartType.Pie);
            string donationChartPath = dataScienceService.PlotData(donationData, "Donation Transactions by Month", ChartType.Bar);
            string withdrawalChartPath = dataScienceService.PlotData(withdrawalData, "Withdrawal Transactions by Month", ChartType.Bar);


            AddChartToUI(donationByTimeofDayChart);

            //AddChartToUI(donationChartByTimePath); - saving in pdf is not working with this line of code (at least for me)

            AddChartToUI(donationChartPath);
            AddChartToUI(withdrawalChartPath);
            AddChartToUI(withdrawalChartByTimePath);
            AddChartToUI(donationChartByHourPath);
            AddChartToUI(withdrawalChartByHourPath);


        }


        private void AddTextToUI(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Text is empty or null.");
                return;
            }

            TextBlock textBlock = new TextBlock
            {
                Text = text,
                Margin = new Thickness(0, 10, 0, 10),
                FontSize = 14,
                FontWeight = FontWeights.Bold, 
                TextAlignment = TextAlignment.Center 
            };

            ChartsStackPanel.Children.Add(textBlock);
        }

        private void AddChartToUI(string chartFilePath)
        {
            if (string.IsNullOrEmpty(chartFilePath) || !File.Exists(chartFilePath))
            {
                MessageBox.Show("Chart file not found or could not be generated.");
                return;
            }

            Uri chartUri = new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, chartFilePath));

            Image chartImage = new Image
            {
                Source = new BitmapImage(chartUri),
                Margin = new Thickness(0, 10, 0, 10),
                Width = 600,
                Height = 450
            };

            ChartsStackPanel.Children.Add(chartImage);
        }
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


        private void SaveStatsToPdf_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFilePaths.Count == 0)
            {
                MessageBox.Show("Please select at least one CSV file.");
                return;
            }

            string outputDirectory = @"C:\Users\Laptopchik\Desktop\monopost";

            try
            {
                string selectedFile = selectedFilePaths[0];

                var pdfService = new DataScienceSavingPdfService(selectedFile);

                var result = pdfService.SaveResults("StatisticsReport.pdf", outputDirectory);

                if (result.Success)
                {
                    MessageBox.Show($"PDF saved successfully {result.Data}");
                }
                else
                {
                    MessageBox.Show($"Error saving PDF: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating PDF: {ex.Message}");
            }
        }

        private void ResetChartAndStats()
        {
            ChartsStackPanel.Children.Clear(); 

            FileItemsControl.Visibility = Visibility.Visible;
            SaveStatsButton.Visibility = Visibility.Collapsed;
        }
    }
}
