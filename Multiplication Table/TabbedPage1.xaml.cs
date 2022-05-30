using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Multiplication_Table
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TabbedPage1 : TabbedPage
    {

        public int factor1;
        public int factor2;

        public bool currentCombinationExists;
        public int currentCombinationID;
        public int currentCombinationCount;
        public int currentCombinationRightAnswers;
        public int currentCombinationWrongAnswers;

        public int rightAnswer;
        public int userAnswer;

        public string message;

        public int combinationsCount;

        public int sequence = 1;

        private string context = "waitForAnswer";

        ObservableCollection<CarouselImagesModel> images;

        private int dayPracticeCount = 0;
        private float percentage;

        public TabbedPage1()
        {
            App.Database.InitialiseConnection();
            InitializeComponent();

            images = new ObservableCollection<CarouselImagesModel>
            {
                new CarouselImagesModel{Image=ImageSource.FromResource("Multiplication_Table.Images.table_one.png")},
                new CarouselImagesModel{Image=ImageSource.FromResource("Multiplication_Table.Images.table_two.png")},
                new CarouselImagesModel{Image=ImageSource.FromResource("Multiplication_Table.Images.table_three.png")},
                new CarouselImagesModel{Image=ImageSource.FromResource("Multiplication_Table.Images.table_four.png")},
                new CarouselImagesModel{Image=ImageSource.FromResource("Multiplication_Table.Images.table_five.png")},
                new CarouselImagesModel{Image=ImageSource.FromResource("Multiplication_Table.Images.table_six.png")},
                new CarouselImagesModel{Image=ImageSource.FromResource("Multiplication_Table.Images.table_seven.png")},
                new CarouselImagesModel{Image=ImageSource.FromResource("Multiplication_Table.Images.table_eight.png")},
                new CarouselImagesModel{Image=ImageSource.FromResource("Multiplication_Table.Images.table_nine.png")},
                new CarouselImagesModel{Image=ImageSource.FromResource("Multiplication_Table.Images.table_zero.png")},
            };

            _myCarouselView.ItemsSource = images;

            UpdateProgressTabContent(); // Need this here to load existing progress when user opens the app
            CreateNewTest();
            
        }

        public void CreateNewTest()
        {

            combinationsCount = App.Database.GetCombinationsCount();
            Console.WriteLine("Combinations count: " + combinationsCount);

            if (combinationsCount < 20)
            {
                ForceNewCombination();
            }

            else
            {

                if (sequence < 3) // 1 or 2 in this case
                {
                    ForceNewCombination(); // currentCombinationExists can be true or false
                    sequence++;
                }

                else if (sequence == 3)
                {
                    ForceEasyCombination();
                    currentCombinationExists = true; // Always true
                    sequence++;
                    
                }

                else if (sequence == 4)
                {
                    ForceHardCombination();
                    currentCombinationExists = true; // Always true
                    sequence++;
                    
                }

                else if (sequence == 5)
                {
                    RandomizeCombination(); // currentCombinationExists can be true or false
                    sequence = 1; // Reset the sequence
                    
                }
            }

            currentCombinationExists = App.Database.CheckIfCombinationExists(factor1, factor2);
            UpdateContent();
        }
        public void RandomizeCombination()
        {
            Random rnd = new Random();
            factor1 = rnd.Next(0, 10);
            factor2 = rnd.Next(0, 10);
            currentCombinationExists = App.Database.CheckIfCombinationExists(factor1, factor2);

        }

        public void ForceNewCombination()
        {
            RandomizeCombination();

            while (currentCombinationExists && combinationsCount < 100)
            {
                // Need to check performance once combination count is higher
                // Will this freeze the app while trying to find a new combination?
                RandomizeCombination();

            }
        }

        public void ForceEasyCombination()
        {
            Combination combination = App.Database.GetEasyCombination();

            factor1 = combination.firstFactor;
            factor2 = combination.secondFactor;

        }

        public void ForceHardCombination()
        {
            Combination combination = App.Database.GetHardCombination();

            factor1 = combination.firstFactor;
            factor2 = combination.secondFactor;

        }

        public void UpdateContent()
        {
            string combinationString = factor1.ToString() + " x " + factor2.ToString();
            _combination.Text = combinationString;
        }

        public void NextStep()
        {
            if (context == "waitForAnswer")
            {
                context = "readyForNext";

                rightAnswer = factor1 * factor2;

                bool wasProgressLoggedToday = App.Database.CheckProgressOnDay(DateTime.Today);
                Console.WriteLine("wasProgressLoggedToday = " + wasProgressLoggedToday);

                if (!currentCombinationExists) // New Combination
                {
                    currentCombinationCount = 1;
                    currentCombinationRightAnswers = 0;
                    currentCombinationWrongAnswers = 0;
                    currentCombinationID = App.Database.GetCombinationID(factor1, factor2); // Generate ID for new combination
                }

                else // Existing Combination
                {
                    var combination = App.Database.GetCombination(factor1, factor2);
                    currentCombinationCount = combination.count + 1;
                    currentCombinationRightAnswers = combination.rightAnswersCount;
                    currentCombinationWrongAnswers = combination.wrongAnswersCount;
                    currentCombinationID = combination.id; // Existing combination; ID lookup
                }

                if (userAnswer == rightAnswer)
                {
                    _submitCopy.TextColor = Color.Blue;
                    _submitCopy.Text = "You gave the correct answer!";
                    _submitCopy.IsVisible = true;
                    currentCombinationRightAnswers++;

                    if (!wasProgressLoggedToday)
                    {

                        Progress progress = new Progress()
                        {
                            date = DateTime.Today.ToString("MM/dd/yyyy"),
                            dayCount = 1,
                            rightAnswers = 1 // Because the user submitted a right answer
                        };

                        App.Database.AddProgressOnDay(progress);
                        Console.WriteLine("Added progress on day - first entry - right answer");
                    }

                    else // Not the fist combination today
                    {
                        var existingDayProgress = App.Database.GetProgressOnDay(DateTime.Today);

                        App.Database.UpdateProgressOnDay(DateTime.Today, existingDayProgress.dayCount + 1, existingDayProgress.rightAnswers + 1);
                        Console.WriteLine("Updated progress on day - there was an entry already - right answer");
                    }

                }

                if (userAnswer != rightAnswer)
                {
                    _submitCopy.TextColor = Color.Red;
                    _submitCopy.Text = "The correct answer was " + rightAnswer.ToString();
                    _submitCopy.IsVisible = true;
                    currentCombinationWrongAnswers++;

                    if (!wasProgressLoggedToday)
                    {

                        Progress progress = new Progress()
                        {
                            date = DateTime.Today.ToString("MM/dd/yyyy"),
                            dayCount = 1,
                            rightAnswers = 0 // Because the user submitted a WRONG answer
                        };

                        App.Database.AddProgressOnDay(progress);
                        Console.WriteLine("Added progress on day - first entry - wrong answer");
                    }

                    else // Not the fist combination today
                    {
                        var existingDayProgress = App.Database.GetProgressOnDay(DateTime.Today);

                        App.Database.UpdateProgressOnDay(DateTime.Today, existingDayProgress.dayCount + 1, existingDayProgress.rightAnswers);
                        // Note that we're not changing the number of right answers because this was a wrong answer
                        Console.WriteLine("Updated progress on day - there was an entry already - wrong answer");
                    }
                }

                UpdateCombinationStats();
                UpdateProgressTabContent();

                _nextButton.IsVisible = true;
                _userInput.IsEnabled = false;

                return; // To avoid running the code below before user taps the button
            }

            if (context == "readyForNext")
            {
                context = "waitForAnswer";

                _nextButton.IsVisible = false;
                _userInput.IsEnabled = true;

                CreateNewTest();

            }
        }

        public void UpdateCombinationStats()
        {
            Combination updatedCombination = new Combination()
            {
                id = currentCombinationID,
                firstFactor = factor1,
                secondFactor = factor2,
                count = currentCombinationCount,
                rightAnswersCount = currentCombinationRightAnswers,
                wrongAnswersCount = currentCombinationWrongAnswers,
                rightAnswerPercentage = (currentCombinationRightAnswers / currentCombinationCount) * 100,
            };

            if (currentCombinationExists)
            {
                App.Database.UpdateCombination(updatedCombination);
            }

            else
            {
                App.Database.AddCombination(updatedCombination);
            }
        }

        private void OnClickNext(object sender, EventArgs e)
        {
            _prompt.FontAttributes = FontAttributes.Bold;
            _userInput.Text = "";
            _submitCopy.IsVisible = false;
            NextStep();
        }

        private void OnEntrySubmit(object sender, EventArgs e)
        {
            string input = _userInput.Text;

            if (input != null) // Prevent crash on Regex.Replace
            {
                string inputDigitsOnly = Regex.Replace(input, "[^0-9]", ""); // Prevents app crashing if users submits dot or dash
                if (inputDigitsOnly != "" && inputDigitsOnly == input)
                {
                    userAnswer = int.Parse(inputDigitsOnly);
                    _prompt.FontAttributes = FontAttributes.None;
                    _submitCopy.IsVisible = true;
                    NextStep();
                }

                else
                {
                    _userInput.Text = ""; // Erase input (in case they have special chars)
                }
            }

        }

        public void GetDayPracticeCount(DateTime day)
        {
                Console.WriteLine("Trying GetProgressOnDay() - Inside GetDayPracticeCount");
                var dayProgress = App.Database.GetProgressOnDay(day);
                int rightAnswers = dayProgress.rightAnswers;
                dayPracticeCount = dayProgress.dayCount;
                percentage = (float)rightAnswers / (float)dayPracticeCount * 100; // Need to multiply by 100 to get percentage
            Console.WriteLine("Day Progress Count: " + dayPracticeCount);
            Console.WriteLine("Day Progress Right Answers: " + rightAnswers);
            Console.WriteLine("Day Progress Percentage: " + percentage);

        }

        public void UpdateProgressTabContent()
        {
            if (App.Database.CheckProgressOnDay(DateTime.Today))
            {
                GetDayPracticeCount(DateTime.Today);
            }

            else
            {
                dayPracticeCount = 0;
            }

            if (dayPracticeCount < 20)
            {
                _sameDayProgress.Text = "N/A";
                _sameDayProgress.TextColor = Color.Gray;
                _progressMessage.Text = "You need to solve at least 20 multiplication problems to see your daily results";
                _progressMessage.TextColor = Color.Gray;
                Console.WriteLine("Day Practice Count < 20 - Won't update progress tab");
            }

            else
            {
                Console.WriteLine("Day Practice Count > 20 - Will update progress tab");
                _sameDayProgress.Text = percentage.ToString("0.0") + "%"; // This means this will be the string format. E.g: 25.5%


                if (percentage < 50)
                {
                    _progressMessage.TextColor = _sameDayProgress.TextColor = Color.Red;
                    _progressMessage.Text = "Keep practicing!";
                }

                else if (percentage >= 50 && percentage < 80)
                {
                    _progressMessage.TextColor = _sameDayProgress.TextColor = Color.Orange;
                    _progressMessage.Text = "You're getting there!";
                }

                else if (percentage >= 80 && percentage < 100)
                {
                    _progressMessage.TextColor = _sameDayProgress.TextColor = Color.Blue;
                    _progressMessage.Text = "Well done!";
                }

                else if (percentage == 100)
                {
                    _progressMessage.TextColor = _sameDayProgress.TextColor = Color.Blue;
                    _progressMessage.Text = "You're a pro!";
                }

            }
        }

    }
}