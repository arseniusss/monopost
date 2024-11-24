using Monopost.BLL.Enums;
using Monopost.BLL.Models;
using Monopost.BLL.Services.Implementations;
using Serilog;

namespace Monopost.UnitTests.Services
{
    public class TransactionServiceTests : IDisposable
    {
        private readonly DataScienceService _service;
        private readonly string _testFilePath = "test_transactions.csv";

        public TransactionServiceTests()
        {
            _service = new DataScienceService();
            SetupTestFile();
            _service.LoadFromCSVs(new List<Tuple<string, string>> { new Tuple<string, string> (_testFilePath, string.Empty) });
        
        }

        public void Dispose()
        {
            Log.CloseAndFlush();
        }

        private void SetupTestFile()
        {
            File.WriteAllLines(_testFilePath, new[]
            {
                "DateTime,Category,Amount,Currency,From,Comment,Balance",
                "15.10.2023 14:00,Donation,100.00,USD,Jane Doe,Second donation,620.00",
                "15.10.2023 12:00,Withdrawal,30.00,USD,ATM,ATM withdrawal,520.00",
                "15.10.2023 10:00,Donation,50.00,USD,John Doe,First donation,550.00",
                "14.10.2023 18:00,Donation,200.00,USD,Jane Doe,Generous donation,500.00",
                "13.10.2023 09:00,Withdrawal,20.00,USD,Supermarket,Groceries,300.00",
                "13.10.2023 08:00,Donation,150.00,USD,John Doe,Early morning donation,320.00",
                "12.10.2023 20:00,Withdrawal,75.00,USD,Online Shop,Purchase,170.00",
                "12.10.2023 15:00,Donation,120.00,USD,Jane Doe,Afternoon donation,245.00",
                "11.10.2023 09:30,Donation,80.00,USD,John Doe,Morning donation,125.00"
            });
        }

        [Fact]
        public void FilterTransactionsByDay_ValidDateRange_ReturnsFilteredTransactions()
        {
            var from = new DateTime(2023, 10, 12);
            var to = new DateTime(2023, 10, 15);
            var result = _service.FilterTransactionsByDay(from, to);

            Assert.True(result.Success);
            Assert.Equal(8, result.Data?.Count);
        }

        [Fact]
        public void FilterTransactionsByDay_InvalidDateRange_ReturnsErrorMessage()
        {
            var from = new DateTime(2023, 10, 15);
            var to = new DateTime(2023, 10, 10);
            var result = _service.FilterTransactionsByDay(from, to);

            Assert.False(result.Success);
            Assert.Equal("'From' date must be earlier than or equal to the 'To' date.", result.Message);
        }

        [Fact]
        public void FilterTransactionsByType_ReturnsCorrectDonations()
        {
            var prefilteredResult = _service.FilterTransactionsByDay(null, null);
            List<Transaction> transactions = new();
            if (!prefilteredResult.Success || prefilteredResult.Data == null)
            {
                transactions = new List<Transaction>();
            }
            else
            {
                transactions = _service.FilterTransactionsByType(prefilteredResult.Data, TransactionType.Donation);
            }
            Assert.NotEmpty(transactions);
            Assert.All(transactions, t => Assert.True(!t.IsWithdrawal));
        }

        [Fact]
        public void FilterTransactionsByType_ReturnsCorrectWithdrawals()
        {
            var withdrawalsResult = _service.GetWithdrawals();

            Assert.True(withdrawalsResult.Success, withdrawalsResult.Message);

            var transactions = _service.FilterTransactionsByType(withdrawalsResult.Data ?? new List<Transaction>(), TransactionType.Withdrawal);

            Assert.NotEmpty(transactions);
            Assert.All(transactions, t => Assert.True(t.IsWithdrawal));
        }

        [Fact]
        public void AggregateTransactions_ValidData_ReturnsCorrectAggregatedResults()
        {
            var from = new DateTime(2023, 10, 10);
            var to = new DateTime(2023, 10, 15);
            var result = _service.AggregateTransactions(from, to, AggregateBy.DayOfMonth, TransactionType.Donation);

            Assert.True(result.Success);
            Assert.Equal(5, result.Data?.Count);
        }

        [Fact]
        public void AggregateTransactions_InvalidDateRange_ReturnsError()
        {
            var from = new DateTime(2023, 10, 15);
            var to = new DateTime(2023, 10, 10);
            var result = _service.AggregateTransactions(from, to);

            Assert.False(result.Success);
            Assert.Equal("'From' date must be earlier than or equal to the 'To' date.", result.Message);
        }

        [Fact]
        public void GetBiggestTransactions_ValidData_ReturnsBiggestTransaction()
        {
            var from = new DateTime(2023, 10, 10);
            var to = new DateTime(2023, 10, 15);
            var result = _service.GetBiggestTransactions(from, to, TransactionType.Donation, 1);

            Assert.True(result.Success);
            Assert.Equal(200.00m, result.Data?[0].Amount);
        }

        [Fact]
        public void GetBiggestTransactions_InvalidDateRange_ReturnsError()
        {
            var from = new DateTime(2023, 10, 15);
            var to = new DateTime(2023, 10, 10);
            var result = _service.GetBiggestTransactions(from, to);

            Assert.False(result.Success);
            Assert.Equal("'From' date must be earlier than or equal to the 'To' date.", result.Message);
        }

        [Fact]
        public void GetSmallestTransactions_ValidData_ReturnsSmallestTransaction()
        {
            var from = new DateTime(2023, 10, 10);
            var to = new DateTime(2023, 10, 15);
            var result = _service.GetSmallestTransactions(from, to, TransactionType.Donation, 1);

            Assert.True(result.Success);
            Assert.Equal(50.00m, result.Data?[0].Amount);
        }

        [Fact]
        public void TotalTransactionAmount_ReturnsCorrectSum()
        {
            var from = new DateTime(2023, 10, 10);
            var to = new DateTime(2023, 10, 15);
            var result = _service.TotalTransactionAmount(from, to, TransactionType.Donation);

            Assert.True(result.Success);
            Assert.Equal(700m, result.Data);
        }

        [Fact]
        public void AverageTransactionAmount_ReturnsCorrectAverage()
        {
            var from = new DateTime(2023, 10, 10);
            var to = new DateTime(2023, 10, 15);
            var result = _service.AverageTransactionAmount(from, to, TransactionType.Donation);

            Assert.True(result.Success);
            Assert.Equal(116.66666666666666666666666667m, result.Data);
        }

        [Fact]
        public void TestEdgeCase_TransactionsAreClassifiedCorrectly()
        {
            var transactionsResult = _service.FilterTransactionsByDay(new DateTime(2023, 10, 10), new DateTime(2023, 10, 15));

            Assert.True(transactionsResult.Success, transactionsResult.Message);

            var transactions = transactionsResult.Data ?? new List<Transaction>();

            Assert.Contains(transactions, t => t.Comment == "Morning donation" && !t.IsWithdrawal);
            Assert.Contains(transactions, t => t.Comment == "ATM withdrawal" && t.IsWithdrawal);
        }
    }
}