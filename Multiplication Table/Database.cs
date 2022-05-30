using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Multiplication_Table
{
    public class Database
    {
        private SQLiteConnection _connection;

        public void InitialiseConnection()
        {
            var DataSource = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "combinations.db");
            SQLiteConnectionString options = new SQLiteConnectionString(DataSource, false);
            _connection = new SQLiteConnection(options);
            _connection.CreateTable<Combination>();
            _connection.CreateTable<Progress>();
        }

        public void AddCombination(Combination combination)
        {
            _connection.Insert(combination);
        }

        public int GetCombinationsCount()
        {
            var entries = _connection.Table<Combination>().ToList(); // Need to check if this is too expensive
            return entries.Count;
        }

        public Combination GetCombination(int factor1, int factor2) // Original method - need to search by factors
        {
            int combinationID = GetCombinationID(factor1, factor2);
            var combination = _connection.Get<Combination>(combinationID);
            return combination;
        }

        public Combination GetCombination(int combinationID) // Overload method if needed to search by ID
        {
            var combination = _connection.Get<Combination>(combinationID);
            return combination;
        }

        public int GetCombinationID(int factor1, int factor2)
        {
            int combinationID = int.Parse(factor1.ToString() + factor2.ToString());
            return combinationID;
        }

        public void UpdateCombination(Combination combination)
        {
            _connection.Update(combination);
        }

        public bool CheckIfCombinationExists(int factor1, int factor2)
        {
            int combinationID = int.Parse(factor1.ToString() + factor2.ToString());
            var combination = _connection.Find<Combination>(combinationID);

            if (combination == null)
            {
                return false;
            }

            else
            {
                return true;
            }
        }

        public Combination GetEasyCombination()
        {
            string query = "SELECT * FROM Combinations ORDER BY rightAnswerPercentage DESC LIMIT 20";
            var easyCombinations = _connection.Query<Combination>(query);
            Random rnd = new Random();
            var easyCombination = easyCombinations[rnd.Next(0, 20)];
            return easyCombination;
        }

        public Combination GetHardCombination()
        {
            string query = "SELECT * FROM Combinations ORDER BY rightAnswerPercentage ASC LIMIT 5";
            var hardCombinations = _connection.Query<Combination>(query);
            Random rnd = new Random();
            var hardCombination = hardCombinations[rnd.Next(0, 5)];
            return hardCombination;
        }

        public bool CheckProgressOnDay(DateTime date)
        {
            string dateCalendar = date.ToString("MM/dd/yyyy"); 
            string query = "SELECT date FROM Progress WHERE date = ?";
            var progressDate = _connection.Query<Progress>(query, dateCalendar);

            if (progressDate.Count == 0)
            {
                Console.WriteLine("Database - CheckProgressOnDay - returned false");
                return false;
            }


            else
            {
                Console.WriteLine("Database - CheckProgressOnDay - returned true");
                return true;
            }

        }

        public Progress GetProgressOnDay(DateTime date)
        {
            string dateCalendar = date.ToString("MM/dd/yyyy");
            string query = "SELECT * FROM Progress WHERE date = ?";
            List<Progress> progressDate = _connection.Query<Progress>(query, dateCalendar);
            Console.WriteLine("Code gets here 1");
            Console.WriteLine("GetProgressOnDay - Database - date: " + progressDate[0].date);
            Console.WriteLine("GetProgressOnDay - Database - day count: " + progressDate[0].dayCount);
            Console.WriteLine("GetProgressOnDay - Database - right answers: " + progressDate[0].rightAnswers);
            return progressDate[0]; // Less expensive and it will always be only 1 result anyways, so no need for list
        }

        public void UpdateProgressOnDay(DateTime date, int count, int rightAnswerCount)
        {
            string dateCalendar = date.ToString("MM/dd/yyyy");
            string query = "UPDATE Progress SET rightAnswers = ?, dayCount = ? WHERE date = ?";
            _connection.Query<Progress>(query, rightAnswerCount, count, dateCalendar); // Note that we're passing dateCalendar and not date
            Console.WriteLine("Updated progress on day. Date = " + dateCalendar);
            Console.WriteLine("Updated progress on day. DayCount = " + count);
            Console.WriteLine("Updated progress on day. RightAnswers = " + rightAnswerCount);

        }

        public void AddProgressOnDay(Progress progressOnDay)
        {
            _connection.Insert(progressOnDay);
            Console.WriteLine("Progress added on day (first time)");
            Console.WriteLine("Date: " + progressOnDay.date);
            Console.WriteLine("Day count: " + progressOnDay.dayCount);
            Console.WriteLine("Right answers: " + progressOnDay.rightAnswers);
        }

        // Consider a delete for a cleaner DB

    }
}
