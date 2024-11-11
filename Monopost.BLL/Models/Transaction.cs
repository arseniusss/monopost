using System.Globalization;
using System.Text.RegularExpressions;

namespace Monopost.BLL.Models
{
    public class Transaction
    {
        public DateTime DateTime { get; private set; }
        public string Category { get; private set; }
        public decimal Amount { get; private set; }
        public string Currency { get; private set; }
        public decimal Balance { get; private set; }
        public string From { get; private set; }
        public string Comment { get; private set; }
        public bool IsWithdrawal { get; set; }
        public string FromJar { get; set; }

        public override string ToString()
        {
            return $"{DateTime:dd.MM.yyyy HH:mm} - {Amount} {Currency} from {From}, Comment: {Comment}";
        }

        public Transaction(DateTime date, string category, decimal amount, string currency, string from, string comment, decimal balance, string fromJar = "")
        {
            DateTime = date;
            Category = category;
            Amount = amount;
            Currency = currency;
            From = from;
            Comment = comment;
            Balance = balance;
            FromJar = fromJar;
        }

        public static Transaction ParseFromCsv(string csvLine)
        {
            var fields = Regex.Split(csvLine, ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

            if (fields.Length < 7)
                throw new ArgumentException("Invalid CSV line format");

            for (int i = 0; i < fields.Length; i++)
            {
                fields[i] = fields[i].Trim().Trim('"');
            }

            try
            {
                return new Transaction(
                    DateTime.ParseExact(fields[0], "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture),
                    fields[1],
                    decimal.Parse(fields[2], CultureInfo.InvariantCulture),
                    fields[3],
                    fields[4],
                    fields[5],
                    decimal.Parse(fields[6], CultureInfo.InvariantCulture)
                );
            }
            catch (Exception ex)
            {
                throw new FormatException($"Error parsing transaction: {ex.Message}, Input: {csvLine}");
            }
        }

        public void SetJarName(string jarName)
        {
            FromJar = jarName;
        }
    }
}