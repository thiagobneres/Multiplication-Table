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

        public string context = "waitForAnswer";

        ObservableCollection<CarouselImagesModel> images;

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

            CreateNewTest();
        }

        public void CreateNewTest()
        {

            combinationsCount = App.Database.GetCombinationsCount();
            Console.WriteLine("Combinations count: " + combinationsCount);

            if (combinationsCount < 20)
            {
                Console.WriteLine("Forcing a new combination - Less than 20 combinations unlocked");
                ForceNewCombination();
            }

            else
            {
                Console.WriteLine("Sequence: " + sequence);

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

            Console.WriteLine("Forcing new combination in ForceNewCombination()");

            while (currentCombinationExists && combinationsCount < 100)
            {
                Console.WriteLine("Combination already exists. F1: " + factor1 + " / F2: " + factor2);
                // Need to check performance once combination count is higher
                // Will this freeze the app while trying to find a new combination?
                RandomizeCombination();

            }
        }

        public void ForceEasyCombination()
        {
            Combination combination = App.Database.GetEasyCombination();

            Console.WriteLine("Selected combination ID: " + combination.id);

            factor1 = combination.firstFactor;
            factor2 = combination.secondFactor;

            Console.WriteLine("Forced easy combination. F1: " + factor1 + " / F2: " + factor2 + " / Percentage: " + combination.rightAnswerPercentage);

        }

        public void ForceHardCombination()
        {
            Combination combination = App.Database.GetHardCombination();

            Console.WriteLine("Selected combination ID: " + combination.id);

            factor1 = combination.firstFactor;
            factor2 = combination.secondFactor;

            Console.WriteLine("Forced hard combination. F1: " + factor1 + " / F2: " + factor2 + " / Percentage: " + combination.rightAnswerPercentage);

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
                }

                if (userAnswer != rightAnswer)
                {
                    _submitCopy.TextColor = Color.Red;
                    _submitCopy.Text = "The correct answer was " + rightAnswer.ToString();
                    _submitCopy.IsVisible = true;
                    currentCombinationWrongAnswers++;
                }

                UpdateCombinationStats();

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

                // Some code to change button text (submit)

                // Hide Message with comments on result (wrong or right)

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

            Console.WriteLine("id: " + updatedCombination.id);
            Console.WriteLine("F1: " + updatedCombination.firstFactor);
            Console.WriteLine("F2: " + updatedCombination.secondFactor);
            Console.WriteLine("Comb count: " + updatedCombination.count);
            Console.WriteLine("R answer C: " + updatedCombination.rightAnswersCount);
            Console.WriteLine("W answer C: " + updatedCombination.wrongAnswersCount);
            Console.WriteLine("R answer P: " + updatedCombination.rightAnswerPercentage);
            Console.WriteLine("Percentage math: " + (float)currentCombinationRightAnswers / (float)currentCombinationCount * 100);

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
}