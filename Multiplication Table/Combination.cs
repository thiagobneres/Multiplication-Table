using SQLite;
using System;

namespace Multiplication_Table
{
        [Table("Combinations")]
        public class Combination
        {

            [PrimaryKey, Column("ID")]
            public int id
            { get; set; }

            [Column("FirstFactor")]
            public int firstFactor
            { get; set; }

            [Column("SecondFactor")]
            public int secondFactor
            { get; set; }

            [Column("Count")]
            public int count
            { get; set; }

            [Column("RightAnswers")]
            public int rightAnswersCount
            { get; set; }

            [Column("WrongAnswers")]
            public int wrongAnswersCount
            { get; set; }

            [Column("RightAnswerPercentage")]
            public float rightAnswerPercentage
            { get; set; }

    }
}
