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
        }

        public void AddCombination(Combination combination)
        {
            _connection.Insert(combination);
        }

        public int GetCombinationsCount()
        {
            var entries = _connection.Table<Combination>().ToList();
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

    }
}
