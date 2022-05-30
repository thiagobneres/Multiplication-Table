using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace Multiplication_Table
{
    [Table("Progress")]
    public class Progress
    {

        [Column("Date")]
        public string date
        { get; set; }

        [Column("DayCount")]
        public int dayCount
        { get; set; }

        [Column("RightAnswers")]
        public int rightAnswers
        { get; set; }

    }
}
