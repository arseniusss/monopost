using ScottPlot;
using System.Text;
using Monopost.BLL.Enums;
using Monopost.BLL.Services.Interfaces;
using Monopost.BLL.Models;
using Serilog;
using Transaction = Monopost.BLL.Models.Transaction;
using Monopost.Logging;

namespace Monopost.BLL.Services.Implementations
{
    public static class DateTimeValidator
    {
        public static Result ValidateDateRange(DateTime? from, DateTime? to)
        {
            var MINIMUM_VIABLE_DATE = new DateTime(2000, 1, 1);
            if (from == null && to == null)
            {
                return new Result(true, "Date range is valid.");
            }

            if (from == null && to != null)
            {
                return new Result(true, "'From' date is not provided, only 'To' date will be considered.");
            }

            if (from != null && to == null)
            {
                if (from < MINIMUM_VIABLE_DATE)
                {
                    return new Result(false, "'From' date must be after January 1, 2000.");
                }

                return new Result(true, "'To' date is not provided, only 'From' date will be considered.");
            }

            if (from > to)
            {
                return new Result(false, "'From' date must be earlier than or equal to the 'To' date.");
            }

            if (from < MINIMUM_VIABLE_DATE)
            {
                return new Result(false, "'From' date must be after January 1, 2000.");
            }

            return new Result(true, "Date range is valid.");
        }
    }
    public class DataScienceService : IDataScienceService
    {
        private List<Transaction> transactions;
        public static ILogger logger = LoggerConfig.GetLogger();

        public DataScienceService()
        {
            logger.Information("DataScienceService created.");
            transactions = new List<Transaction>();
        }

        public void LoadFromCsv(Tuple<string, string> filePathWithLabel)
        {
            List<Transaction> tempTransactions = new List<Transaction>();

            var lines = File.ReadAllLines(filePathWithLabel.Item1, Encoding.UTF8).Skip(1);
            logger.Information($"Transactions parsed from CSV: {lines.Count()} transaction(s)");
            foreach (var line in lines)
            {
                Transaction tempTransaction = Transaction.ParseFromCsv(line);
                tempTransaction.FromJar = filePathWithLabel.Item2;
                tempTransactions.Add(tempTransaction);
            }
            SetWithdrawals(tempTransactions);

            transactions.AddRange(tempTransactions);
        }

        public void LoadFromCSVs(List<Tuple<string, string>> jarsWithLabels)
        {
            foreach (var filePath in jarsWithLabels)
            {
                LoadFromCsv(filePath);
            }
        }

        private void SetWithdrawals(List<Transaction> transactions)
        {
            if (transactions.Count < 2)
            {
                logger.Information("Not enough transactions to set withdrawals.");
                return;
            }

            transactions = transactions.OrderByDescending(t => t.DateTime).ToList();
            logger.Information($"Transactions ordered by date: {string.Join(", ", transactions.Select(t => t.DateTime))}");

            for (int i = 0; i < transactions.Count - 1; i++)
            {
                var currentTransaction = transactions[i];
                var previousTransaction = transactions[i + 1];

                currentTransaction.IsWithdrawal = currentTransaction.Balance < previousTransaction.Balance;
            }
        }




        public Result<List<Transaction>> GetWithdrawals()
        {
            if (transactions.Count < 2)
            {
                logger.Information("Not enough transactions to detect withdrawals.");
                return new Result<List<Transaction>>(true, "Not enough transactions to detect withdrawals.", new List<Transaction>());
            }

            var withdrawals = transactions
                .Where(transaction => transaction.IsWithdrawal)
                .ToList();

            logger.Information($"Withdrawals retrieved: {withdrawals.Count} transactions.");
            return new Result<List<Transaction>>(true, "Withdrawals retrieved.", withdrawals);
        }

        public Result<List<Transaction>> FilterTransactionsByDay(DateTime? from, DateTime? to)
        {
            var validationResult = DateTimeValidator.ValidateDateRange(from, to);
            if (!validationResult.Success)
            {
                logger.Warning($"Date range validation failed: {validationResult.Message}");
                return new Result<List<Transaction>>(false, validationResult.Message);
            }

            var filteredTransactions = transactions.Where(t =>
                (!from.HasValue || t.DateTime.Date >= from.Value.Date) &&
                (!to.HasValue || t.DateTime.Date <= to.Value.Date)
            ).ToList();

            logger.Information($"Transactions filtered by date range from {from?.ToString("yyyy-MM-dd")} to {to?.ToString("yyyy-MM-dd")}: {filteredTransactions.Count} transactions found.");
            return new Result<List<Transaction>>(true, "Transactions filtered by date range.", filteredTransactions);
        }

        public List<Transaction> FilterTransactionsByType(List<Transaction> transactions, TransactionType transactionType)
        {
            logger.Information($"Filtering transactions by type: {transactionType}");

            var filteredTransactions = transactions.Where(transaction =>
                transactionType switch
                {
                    TransactionType.Donation => !transaction.IsWithdrawal,
                    TransactionType.Withdrawal => transaction.IsWithdrawal,
                    TransactionType.Any => true,
                    _ => false,
                }).ToList();
                    
            logger.Information($"Filtered transactions: {filteredTransactions.Count} transactions found.");
            return filteredTransactions;
        }

        public Result<Dictionary<string, List<Transaction>>> AggregateTransactions(DateTime? from, DateTime? to, AggregateBy aggregateBy = AggregateBy.HourOfDay, TransactionType transactionType = TransactionType.Donation)
        {
            logger.Information($"Aggregating {transactions.Count} transactionsof type {transactionType} from {from} to {to} by {aggregateBy}.");
            var dateValidationResult = DateTimeValidator.ValidateDateRange(from, to);
            if (!dateValidationResult.Success)
            {
                logger.Warning($"Date range validation failed: {dateValidationResult.Message}");
                return new Result<Dictionary<string, List<Transaction>>>(false, dateValidationResult.Message);
            }

            var aggregatedResults = new Dictionary<string, List<Transaction>>();
            var prefilteredTransactions = FilterTransactionsByDay(from, to);
            if (prefilteredTransactions.Data == null || prefilteredTransactions.Data.Count == 0)
            {
                logger.Warning("No transactions found in the specified date range.");
                return new Result<Dictionary<string, List<Transaction>>>(true, "No transactions found in the specified date range.", aggregatedResults);
            }

            var filteredTransactions = FilterTransactionsByType(prefilteredTransactions.Data, transactionType);

            foreach (var transaction in filteredTransactions)
            {
                string key = GetAggregationKey(transaction, aggregateBy);

                if (!aggregatedResults.ContainsKey(key))
                {
                    aggregatedResults[key] = new List<Transaction>();
                }

                aggregatedResults[key].Add(transaction);
            }

            logger.Information($"Aggregation successful: {aggregatedResults.Count} keys aggregated.");
            return new Result<Dictionary<string, List<Transaction>>>(true, "Aggregation successful.", aggregatedResults.OrderBy(kvp => kvp.Key)
                                                                                                                   .ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
        }

        public Dictionary<string, decimal> ApplyAggregationOperation(
            Dictionary<string, List<Transaction>> aggregatedResults,
            AggregationOperation operation)
        {
            logger.Information($"Applying aggrefation operation {operation}. It always results in success.");
            var results = new Dictionary<string, decimal>();

            foreach (var kvp in aggregatedResults)
            {
                string key = kvp.Key;
                var transactions = kvp.Value;

                decimal result = 0;

                switch (operation)
                {
                    case AggregationOperation.Sum:
                        result = transactions.Sum(t => t.Amount);
                        break;
                    case AggregationOperation.Count:
                        result = transactions.Count;
                        break;
                    case AggregationOperation.Average:
                        result = transactions.Any() ? transactions.Average(t => t.Amount) : 0;
                        break;
                    case AggregationOperation.Max:
                        result = transactions.Any() ? transactions.Max(t => t.Amount) : 0;
                        break;
                    case AggregationOperation.Min:
                        result = transactions.Any() ? transactions.Min(t => t.Amount) : 0;
                        break;
                    default:
                        logger.Warning("Invalid aggregation operation");
                        break;
                }

                results[key] = result;
            }

            return results;
        }
        private string GetAggregationKey(Transaction transaction, AggregateBy aggregateBy)
        {
            return aggregateBy switch
            {
                AggregateBy.HourOfDay => transaction.DateTime.Hour.ToString("D2"),
                AggregateBy.DayOfWeek => transaction.DateTime.DayOfWeek.ToString(),
                AggregateBy.TimeOfDay => GetTimeOfDayKey(transaction.DateTime.Hour),
                AggregateBy.DayOfMonth => transaction.DateTime.Day.ToString(),
                AggregateBy.Month => transaction.DateTime.ToString("MMMM"),
                AggregateBy.Year => transaction.DateTime.Year.ToString(),
                AggregateBy.DonationAmount => GetDonationAmountKey(transaction.Amount),
                _ => string.Empty,
            };
        }
        private string GetTimeOfDayKey(int hour)
        {
            return hour switch
            {
                < 6 => "Night",
                < 12 => "Morning",
                < 18 => "Day",
                < 22 => "Evening",
                _ => "Night",
            };
        }

        private string GetDonationAmountKey(decimal amount)
        {
            return amount switch
            {
                < 10 => "1-10",
                < 50 => "10-50",
                < 250 => "50-250",
                < 1000 => "250-1000",
                _ => "1000+",
            };
        }

        public Result<List<Transaction>> GetBiggestTransactions(DateTime? from, DateTime? to, TransactionType transactionType = TransactionType.Donation, int limit = 1)
        {
            logger.Information("Retrieving biggest transactions.");

            var validationResult = DateTimeValidator.ValidateDateRange(from, to);
            if (!validationResult.Success)
            {
                logger.Warning($"Date range validation failed: {validationResult.Message}");
                return new Result<List<Transaction>>(false, validationResult.Message);
            }

            var prefilteredTransactions = FilterTransactionsByDay(from, to);
            if (prefilteredTransactions.Data == null || prefilteredTransactions.Data.Count == 0)
            {
                logger.Warning("No transactions found in the specified date range.");
                return new Result<List<Transaction>>(true, "No transactions found in the specified date range.", new List<Transaction>());
            }

            var filteredTransactions = FilterTransactionsByType(prefilteredTransactions.Data, transactionType);
            logger.Information($"Top {limit} transactions retrieved: {filteredTransactions.Count} transactions found.");

            return new Result<List<Transaction>>(true, $"Top {limit} transactions retrieved.", filteredTransactions.OrderByDescending(t => t.Amount).Take(limit).ToList());
        }

        public Result<List<Transaction>> GetSmallestTransactions(DateTime? from, DateTime? to, TransactionType transactionType = TransactionType.Donation, int limit = 1)
        {
            logger.Information("Retrieving smallest transactions.");

            var validationResult = DateTimeValidator.ValidateDateRange(from, to);
            if (!validationResult.Success)
            {
                logger.Warning($"Date range validation failed: {validationResult.Message}");
                return new Result<List<Transaction>>(false, validationResult.Message);
            }

            var prefilteredTransactions = FilterTransactionsByDay(from, to);
            if (prefilteredTransactions.Data == null || prefilteredTransactions.Data.Count == 0)
            {
                logger.Warning("No transactions found in the specified date range.");
                return new Result<List<Transaction>>(true, "No transactions found in the specified date range.", new List<Transaction>());
            }

            var filteredTransactions = FilterTransactionsByType(prefilteredTransactions.Data, transactionType);
            logger.Information($"Smallest {limit} transactions retrieved: {filteredTransactions.Count} transactions found.");

            return new Result<List<Transaction>>(true, $"Smallest {limit} transactions retrieved.", filteredTransactions.OrderBy(t => t.Amount).Take(limit).ToList());
        }

        public Result<decimal> TotalTransactionAmount(DateTime? from, DateTime? to, TransactionType transactionType)
        {
            logger.Information("Calculating total transaction amount.");

            var validationResult = DateTimeValidator.ValidateDateRange(from, to);
            if (!validationResult.Success)
            {
                logger.Warning($"Date range validation failed: {validationResult.Message}");
                return new Result<decimal>(false, validationResult.Message);
            }

            var prefilteredTransactions = FilterTransactionsByDay(from, to).Data;

            if (prefilteredTransactions == null)
            {
                logger.Warning("No transactions found in the specified date range.");
                return new Result<decimal>(true, "No transactions found in the specified date range.", 0);
            }

            var filteredTransactions = FilterTransactionsByType(prefilteredTransactions, transactionType);
            decimal totalAmount = filteredTransactions.Sum(t => t.Amount);
            logger.Information($"Total transaction amount calculated: {totalAmount}");

            return new Result<decimal>(true, "Total transaction amount calculated.", totalAmount);
        }

        public Result<decimal> AverageTransactionAmount(DateTime? from, DateTime? to, TransactionType transactionType)
        {
            logger.Information("Calculating average transaction amount.");

            var validationResult = DateTimeValidator.ValidateDateRange(from, to);
            if (!validationResult.Success)
            {
                logger.Warning($"Date range validation failed: {validationResult.Message}");
                return new Result<decimal>(false, validationResult.Message);
            }

            var prefilteredTransactions = FilterTransactionsByDay(from, to);
            if (prefilteredTransactions.Data == null || prefilteredTransactions.Data.Count == 0)
            {
                logger.Warning("No transactions found in the specified date range.");
                return new Result<decimal>(true, "No transactions found in the specified date range.", 0);
            }

            var filteredTransactions = FilterTransactionsByType(prefilteredTransactions.Data, transactionType);
            decimal averageAmount = filteredTransactions.Any() ? filteredTransactions.Average(t => t.Amount) : 0;
            logger.Information($"Average transaction amount calculated: {averageAmount}");

            return new Result<decimal>(true, "Average transaction amount calculated.", averageAmount);
        }

        [Obsolete]
        public string PlotData<T>(Dictionary<string, T> data, string title, ChartType chartType, int width = 800, int height = 800) where T : struct
        {
            logger.Information($"Plotting data with title: '{title}' and chart type: '{chartType}'");
            ChartManager.PlotData<T>(data, title, chartType,width,height);
            string fileName = $"{title.Replace(" ", "_")}_{chartType}.png";
            return fileName;
        }

        private static class ChartManager
        {
            [Obsolete]
            public static string PlotData<T>(Dictionary<string, T> data, string title, ChartType chartType, int width = 800, int height = 800)
        where T : struct
            {
                var plt = new Plot(width, height);
                var keys = data.Keys.ToArray();
                var values = data.Values.Select(v => Convert.ToDouble(v)).ToArray();

                if (values.Length == 0)
                {
                    logger.Warning("No data to plot.");
                    return string.Empty;
                }

                switch (chartType)
                {
                    case ChartType.Line:
                        plt.AddScatter(Enumerable.Range(0, values.Length).Select(x => (double)x).ToArray(), values);
                        plt.Title($"{title} (Line Chart)");
                        plt.XTicks(Enumerable.Range(0, keys.Length).Select(x => (double)x).ToArray(), keys);
                        break;
                    case ChartType.Bar:
                        plt.AddBar(values);
                        plt.Title($"{title} (Bar Chart)");
                        plt.XTicks(Enumerable.Range(0, keys.Length).Select(x => (double)x).ToArray(), keys);
                        break;
                    case ChartType.Pie:
                        var pie = plt.AddPie(values);
                        pie.SliceLabels = keys;
                        pie.ShowValues = false;
                        pie.ShowPercentages = true;
                        pie.OutlineSize = 0;
                        plt.Title($"{title} (Pie Chart)");
                        plt.Legend(location: ScottPlot.Alignment.LowerRight);
                        plt.SetAxisLimits(-1.5, 1.5, -1.5, 1.5);
                        plt.Layout(left: 0, right: 0, top: 50, bottom: 50);
                        plt.Grid(false);
                        plt.Frame(false);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(chartType), "Invalid chart type selected.");
                }

                if (chartType != ChartType.Pie)
                {
                    plt.SetAxisLimits(yMin: 0);
                }

                string fileName = $"{title.Replace(" ", "_")}_{chartType}.png";

                try
                {
                    plt.SaveFig(fileName);
                }
                catch (Exception ex)
                {
                    logger.Error($"Error saving plot: {ex.Message}");
                }

                return fileName;
            }
        }
    }
}