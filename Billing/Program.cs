using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Billing
{
    public class BillingDay
    {
        public required string Day { get; set; }
        public double Billing { get; set; }
        public required string DayOfWeek { get; set; }
    }

    public class BillingService
    {
        private readonly List<BillingDay> _billings;
        private readonly HashSet<DateTime> _holidays;

        public BillingService(string filePath, List<DateTime> holidays)
        {
            _billings = ReadBilling(filePath);
            _holidays = new HashSet<DateTime>(holidays) ?? [];
        }

        private static List<BillingDay> ReadBilling(string filePath)
        {
            try
            {
                var json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<BillingDay>>(json) ?? [];
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("File not found.");
                return new List<BillingDay>();
            }
            catch (JsonException)
            {
                Console.WriteLine("Invalid JSON.");
                return new List<BillingDay>();
            }
        }

        private bool isWeekend(string day)
        {
            var date = DateTime.Parse(day);
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }

        public double CalculateLowestBilling()
        {
            return _billings.Min(b => b.Billing > 0 ? b.Billing : double.MaxValue);
        }

        public double CalculateHighestBilling()
        {
            return _billings.Max(b => b.Billing);
        }

        public double CalculateMonthlyAverage()
        {
            var days_with_billings = _billings.Where(b => b.Billing > 0 && !isWeekend(b.Day) && !_holidays.Contains(DateTime.Parse(b.Day))).ToList();
            return days_with_billings.Any() ? days_with_billings.Average(b => b.Billing) : 0;
        }

        public int CountDaysAboveAverage()
        {
            var monthly_average = CalculateMonthlyAverage();
            return _billings.Count(b => b.Billing > monthly_average);
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            string filePath = "billing.json";

            var holidays = new List<DateTime> { }; // insert holidays here

            // Initialize the billing service
            BillingService billingService = new BillingService(filePath, holidays);

            Console.WriteLine($"Menor faturamento: {billingService.CalculateLowestBilling()}");
            Console.WriteLine($"Maior faturamento: {billingService.CalculateHighestBilling()}");
            Console.WriteLine($"Média mensal: {billingService.CalculateMonthlyAverage():F2}");
            Console.WriteLine($"Número de dias com faturamento acima da média: {billingService.CountDaysAboveAverage()}");
        }
    }
}