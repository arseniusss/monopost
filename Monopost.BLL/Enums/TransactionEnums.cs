namespace Monopost.BLL.Enums
{
    public enum AggregateBy
    {
        HourOfDay,
        DayOfWeek,
        TimeOfDay,
        DayOfMonth,
        Month,
        Year,
        DonationAmount
    }

    public enum AggregationOperation
    {
        Sum,
        Count,
        Average,
        Max,
        Min
    }

    public enum ChartType
    {
        Line,
        Bar,
        Pie
    }
    public enum TransactionType
    {
        Donation,
        Withdrawal,
        Any
    }
}