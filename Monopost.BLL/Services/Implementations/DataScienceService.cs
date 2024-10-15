using ScottPlot;
using System.Drawing;
using System.Text;
using Monopost.BLL.Enums;
using Monopost.BLL.Services.Interfaces;
using Monopost.BLL.Models;

using Transaction = Monopost.BLL.Models.Transaction;

namespace Monopost.BLL.Services.Implementations
{
    public static class DateTimeValidator
    {
        public static Result ValidateDateRange(DateTime? from, DateTime? to)
        {
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
                if (from < new DateTime(2000, 1, 1))
                {
                    return new Result(false, "'From' date must be after January 1, 2000.");
                }

                return new Result(true, "'To' date is not provided, only 'From' date will be considered.");
            }

            if (from > to)
            {
                return new Result(false, "'From' date must be earlier than or equal to the 'To' date.");
            }

            if (from < new DateTime(2000, 1, 1))
            {
                return new Result(false, "'From' date must be after January 1, 2000.");
            }

            return new Result(true, "Date range is valid.");
        }
    }
    public class DataScienceService : IDataScienceService
    {
        private List<Transaction> transactions;

        public DataScienceService(string filepath)
        {
            transactions = new List<Transaction>();
            LoadFromCsv(filepath);
            SetWithdrawals();
        }

        private void LoadFromCsv(string filePath)
        {
            var lines = File.ReadAllLines(filePath, Encoding.UTF8).Skip(1);
            foreach (var line in lines)
            {
                transactions.Add(Transaction.ParseFromCsv(line));
            }
        }
        private void SetWithdrawals()
        {
            if (transactions.Count < 2)
            {
                return;
            }

            transactions = transactions.OrderByDescending(t => t.DateTime).ToList();

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
                return new Result<List<Transaction>>(true, "Not enough transactions to detect withdrawals.", new List<Transaction>());
            }

            var withdrawals = transactions
                .Where(transaction => transaction.IsWithdrawal)
                .ToList();

            return new Result<List<Transaction>>(true, "Withdrawals retrieved.", withdrawals);
        }

        public Result<List<Transaction>> FilterTransactionsByDay(DateTime? from, DateTime? to)
        {
            var validationResult = DateTimeValidator.ValidateDateRange(from, to);
            if (!validationResult.Success)
            {
                return new Result<List<Transaction>>(false, validationResult.Message);
            }

            var filteredTransactions = transactions.Where(t =>
                (!from.HasValue || t.DateTime.Date >= from.Value.Date) &&
                (!to.HasValue || t.DateTime.Date <= to.Value.Date)
            ).ToList();

            return new Result<List<Transaction>>(true, "Transactions filtered by date range.", filteredTransactions);
        }

        public List<Transaction> FilterTransactionsByType(List<Transaction> transactions, TransactionType transactionType)
        {
            return transactions.Where(transaction =>
                transactionType switch
                {
                    TransactionType.Donation => !transaction.IsWithdrawal,
                    TransactionType.Withdrawal => transaction.IsWithdrawal,
                    TransactionType.Any => true,
                    _ => false,
                }).ToList();
        }

        public Result<Dictionary<string, List<Transaction>>> AggregateTransactions(DateTime? from, DateTime? to, AggregateBy aggregateBy = AggregateBy.HourOfDay, TransactionType transactionType = TransactionType.Donation)
        {
            var dateValidationResult = DateTimeValidator.ValidateDateRange(from, to);
            if (!dateValidationResult.Success)
            {
                return new Result<Dictionary<string, List<Transaction>>>(false, dateValidationResult.Message);
            }

            var aggregatedResults = new Dictionary<string, List<Transaction>>();
            var prefilteredTransactions = FilterTransactionsByDay(from, to);
            if (prefilteredTransactions.Data == null || prefilteredTransactions.Data.Count == 0)
            {
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

            return new Result<Dictionary<string, List<Transaction>>>(true, "Aggregation successful.", aggregatedResults.OrderBy(kvp => kvp.Key)
                                                                                                                   .ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
        }
        public Dictionary<string, decimal> ApplyAggregationOperation(
            Dictionary<string, List<Transaction>> aggregatedResults,
            AggregationOperation operation)
        {
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
                        throw new ArgumentException("Invalid aggregation operation");
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
                < 6 => "Ніч",
                < 12 => "Ранок",
                < 18 => "День",
                < 22 => "Вечір",
                _ => "Ніч",
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
            var validationResult = DateTimeValidator.ValidateDateRange(from, to);
            if (!validationResult.Success)
            {
                return new Result<List<Transaction>>(false, validationResult.Message);
            }

            var prefilteredTransactions = FilterTransactionsByDay(from, to);
            if (prefilteredTransactions.Data == null || prefilteredTransactions.Data.Count == 0)
            {
                return new Result<List<Transaction>>(true, "No transactions found in the specified date range.", new List<Transaction>());
            }
            var filteredTransactions = FilterTransactionsByType(prefilteredTransactions.Data, transactionType);

            return new Result<List<Transaction>>(true, $"Top {limit} transactions retrieved.", filteredTransactions.OrderByDescending(t => t.Amount).Take(limit).ToList());
        }

        public Result<List<Transaction>> GetSmallestTransactions(DateTime? from, DateTime? to, TransactionType transactionType = TransactionType.Donation, int limit = 1)
        {
            var validationResult = DateTimeValidator.ValidateDateRange(from, to);
            if (!validationResult.Success)
            {
                return new Result<List<Transaction>>(false, validationResult.Message);
            }

            var prefilteredTransactions = FilterTransactionsByDay(from, to);
            if (prefilteredTransactions.Data == null || prefilteredTransactions.Data.Count == 0)
            {
                return new Result<List<Transaction>>(true, "No transactions found in the specified date range.", new List<Transaction>());
            }
            var filteredTransactions = FilterTransactionsByType(prefilteredTransactions.Data, transactionType);

            return new Result<List<Transaction>>(true, $"Smallest {limit} transactions retrieved.", filteredTransactions.OrderBy(t => t.Amount).Take(limit).ToList());
        }

        public Result<decimal> TotalTransactionAmount(DateTime? from, DateTime? to, TransactionType transactionType)
        {
            var validationResult = DateTimeValidator.ValidateDateRange(from, to);
            if (!validationResult.Success)
            {
                return new Result<decimal>(false, validationResult.Message);
            }

            var prefilteredTransactions = FilterTransactionsByDay(from, to).Data;

            if (prefilteredTransactions == null)
            {
                return new Result<decimal>(true, "No transactions found in the specified date range.", 0);
            }

            var filteredTransactions = FilterTransactionsByType(prefilteredTransactions, transactionType);
            return new Result<decimal>(true, "Total transaction amount calculated.", filteredTransactions.Sum(t => t.Amount));
        }

        public Result<decimal> AverageTransactionAmount(DateTime? from, DateTime? to, TransactionType transactionType)
        {
            var validationResult = DateTimeValidator.ValidateDateRange(from, to);
            if (!validationResult.Success)
            {
                return new Result<decimal>(false, validationResult.Message);
            }

            var prefilteredTransactions = FilterTransactionsByDay(from, to);
            if (prefilteredTransactions.Data == null || prefilteredTransactions.Data.Count == 0)
            {
                return new Result<decimal>(true, "No transactions found in the specified date range.", 0);
            }
            var filteredTransactions = FilterTransactionsByType(prefilteredTransactions.Data, transactionType);
            return new Result<decimal>(true, "Average transaction amount calculated.", filteredTransactions.Any() ? filteredTransactions.Average(t => t.Amount) : 0);
        }
    }


    public static class ChartManager
    {
        [Obsolete]
        public static void PlotData<T>(Dictionary<string, T> data, string title, ChartType chartType)
            where T : struct
        {
            var plt = new Plot(800, 600);


            var keys = data.Keys.ToArray();
            var values = data.Values.Select(v => Convert.ToDouble(v)).ToArray();

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
                    plt.Title(title);
                    plt.Legend(location: ScottPlot.Alignment.LowerRight);
                    plt.SetAxisLimits(-1.5, 1.5, -1.5, 1.5);
                    plt.Layout(left: 0, right: 0, top: 50, bottom: 50);
                    plt.SaveFig("pie_chart.png", 800, 800);
                    plt.Grid(false);
                    plt.Frame(false);
                    break;


                default:
                    throw new ArgumentOutOfRangeException(nameof(chartType), "Invalid chart type selected.");
            }

            plt.SetAxisLimits(yMin: 0);

            plt.SaveFig($"{title.Replace(" ", "_")}.png");

            Console.WriteLine($"Chart saved as {title.Replace(" ", "_")}.png");
        }


        private static Color[] GenerateUniqueColors(int count)
        {
            return Enumerable.Range(0, count)
                .Select(i => ColorFromHSV((float)i / count, 0.7f, 0.9f))
                .ToArray();
        }

        private static Color ColorFromHSV(float hue, float saturation, float value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue * 6)) % 6;
            float f = hue * 6 - (float)Math.Floor(hue * 6);

            value *= 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            return hi switch
            {
                0 => Color.FromArgb(255, v, t, p),
                1 => Color.FromArgb(255, q, v, p),
                2 => Color.FromArgb(255, p, v, t),
                3 => Color.FromArgb(255, p, q, v),
                4 => Color.FromArgb(255, t, p, v),
                _ => Color.FromArgb(255, v, p, q)
            };
        }
    }
}