using Monopost.BLL.Enums;
using Monopost.BLL.Models;

namespace Monopost.BLL.Services.Interfaces
{
    public interface IDataScienceService
    {
        Result<List<Transaction>> GetWithdrawals();
        Result<List<Transaction>> FilterTransactionsByDay(DateTime? from, DateTime? to);
        List<Transaction> FilterTransactionsByType(List<Transaction> transactions, TransactionType transactionType);
        Result<Dictionary<string, List<Transaction>>> AggregateTransactions(DateTime? from, DateTime? to, AggregateBy aggregateBy = AggregateBy.HourOfDay, TransactionType transactionType = TransactionType.Donation);
        Dictionary<string, decimal> ApplyAggregationOperation(Dictionary<string, List<Transaction>> aggregatedResults, AggregationOperation operation);
        Result<List<Transaction>> GetBiggestTransactions(DateTime? from, DateTime? to, TransactionType transactionType = TransactionType.Donation, int limit = 1);
        Result<List<Transaction>> GetSmallestTransactions(DateTime? from, DateTime? to, TransactionType transactionType = TransactionType.Donation, int limit = 1);
        Result<decimal> TotalTransactionAmount(DateTime? from, DateTime? to, TransactionType transactionType);
        Result<decimal> AverageTransactionAmount(DateTime? from, DateTime? to, TransactionType transactionType);
    }
}
