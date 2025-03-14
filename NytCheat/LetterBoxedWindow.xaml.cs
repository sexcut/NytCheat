using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NytCheatMenu
{
    public partial class LetterBoxedWindow : Window
    {
        private List<string> dictionary;
        private Dictionary<char, HashSet<int>> letterSides;
        private List<string> validWords;
        private List<Tuple<string, string>> twoWordSolutions;

        public LetterBoxedWindow()
        {
            InitializeComponent();
            LoadDictionary("dictionary.txt");
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void LoadDictionary(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    dictionary = new List<string>();
                    foreach (string line in File.ReadLines(path))
                    {
                        // Add only words with letters (no numbers or symbols)
                        // and at least 3 letters long (LetterBoxed requirement)
                        if (line.Length >= 3 && line.All(c => char.IsLetter(c)))
                        {
                            dictionary.Add(line.Trim().ToUpper());
                        }
                    }

                    DictionaryStatusText.Text = $"Using default dictionary ({dictionary.Count:N0} words)";
                }
                else
                {
                    // Set initial text
                    DictionaryStatusText.Text = "Dictionary file not found";
                    dictionary = new List<string>();

                    // Create a timer to wait 2 seconds
                    System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(2);
                    timer.Tick += (sender, e) => {
                        timer.Stop();

                        // Create fade-out animation
                        DoubleAnimation fadeOutAnimation = new DoubleAnimation();
                        fadeOutAnimation.From = 1.0;
                        fadeOutAnimation.To = 0.0;
                        fadeOutAnimation.Duration = TimeSpan.FromSeconds(0.3);

                        // When fade-out completes, change text and fade back in
                        fadeOutAnimation.Completed += (s, _) => {
                            DictionaryStatusText.Text = "No dictionary loaded";

                            // Create fade-in animation
                            DoubleAnimation fadeInAnimation = new DoubleAnimation();
                            fadeInAnimation.From = 0.0;
                            fadeInAnimation.To = 1.0;
                            fadeInAnimation.Duration = TimeSpan.FromSeconds(0.3);

                            // Start fade-in animation
                            DictionaryStatusText.BeginAnimation(TextBlock.OpacityProperty, fadeInAnimation);
                        };

                        // Start fade-out animation
                        DictionaryStatusText.BeginAnimation(TextBlock.OpacityProperty, fadeOutAnimation);
                    };

                    // Start the timer
                    timer.Start();
                }
            }
            catch (Exception ex)
            {
                ShowAnimatedMessage($"Error loading dictionary: {ex.Message}", DictionaryStatusText, 5.0);
                dictionary = new List<string>();
            }
        }

        private void LoadCustomDictionary_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog to allow user to select a custom dictionary
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            dlg.Title = "Select Custom Dictionary";

            // Show the dialog and process the result
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                LoadDictionary(filename);
            }
        }

        private void FindSolutions_Click(object sender, RoutedEventArgs e)
        {
            // Get letters from each side of the box
            string topSide = TopSideInput.Text.Trim().ToUpper();
            string rightSide = RightSideInput.Text.Trim().ToUpper();
            string bottomSide = BottomSideInput.Text.Trim().ToUpper();
            string leftSide = LeftSideInput.Text.Trim().ToUpper();

            // Validate input
            if (string.IsNullOrEmpty(topSide) || string.IsNullOrEmpty(rightSide) ||
                string.IsNullOrEmpty(bottomSide) || string.IsNullOrEmpty(leftSide))
            {
                ShowAnimatedMessage("Please enter letters for all sides of the box.", StatusMessageText);
                return;
            }

            // Create a map of which side each letter belongs to
            letterSides = new Dictionary<char, HashSet<int>>();
            MapLettersToSides(topSide, 0);
            MapLettersToSides(rightSide, 1);
            MapLettersToSides(bottomSide, 2);
            MapLettersToSides(leftSide, 3);

            // All letters from the puzzle
            HashSet<char> allLetters = new HashSet<char>(topSide + rightSide + bottomSide + leftSide);

            // Start the timer
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Find valid words
            FindValidWords(allLetters);

            // Find two-word solutions
            FindTwoWordSolutions(allLetters);

            stopwatch.Stop();

            // Display results
            DisplayResults(stopwatch.ElapsedMilliseconds);
        }

        private void MapLettersToSides(string sideLetters, int sideIndex)
        {
            foreach (char letter in sideLetters)
            {
                if (!letterSides.ContainsKey(letter))
                {
                    letterSides[letter] = new HashSet<int>();
                }
                letterSides[letter].Add(sideIndex);
            }
        }

        private void FindValidWords(HashSet<char> allLetters)
        {
            validWords = new List<string>();

            foreach (string word in dictionary)
            {
                // Skip words with letters not in the puzzle
                if (word.Any(c => !allLetters.Contains(c)))
                    continue;

                // Check if consecutive letters are from different sides
                bool isValid = true;
                for (int i = 1; i < word.Length; i++)
                {
                    int prevLetterSide = letterSides[word[i - 1]].First();
                    int currLetterSide = letterSides[word[i]].First();

                    if (prevLetterSide == currLetterSide)
                    {
                        isValid = false;
                        break;
                    }
                }

                if (isValid)
                {
                    validWords.Add(word);
                }
            }

            // Sort words by length (longest first)
            validWords.Sort((a, b) => b.Length.CompareTo(a.Length));
        }

        private void FindTwoWordSolutions(HashSet<char> allLetters)
        {
            twoWordSolutions = new List<Tuple<string, string>>();

            // Create a dictionary of words starting with each letter
            Dictionary<char, List<string>> wordsByFirstLetter = new Dictionary<char, List<string>>();

            foreach (string word in validWords)
            {
                char firstLetter = word[0];
                if (!wordsByFirstLetter.ContainsKey(firstLetter))
                {
                    wordsByFirstLetter[firstLetter] = new List<string>();
                }
                wordsByFirstLetter[firstLetter].Add(word);
            }

            // Look for word pairs where last letter of first word matches first letter of second word
            foreach (string firstWord in validWords)
            {
                char lastLetter = firstWord[firstWord.Length - 1];

                if (wordsByFirstLetter.ContainsKey(lastLetter))
                {
                    foreach (string secondWord in wordsByFirstLetter[lastLetter])
                    {
                        // Skip if it's the same word
                        if (firstWord == secondWord)
                            continue;

                        // Check if all letters are used
                        HashSet<char> usedLetters = new HashSet<char>(firstWord + secondWord);
                        if (usedLetters.Count == allLetters.Count)
                        {
                            twoWordSolutions.Add(new Tuple<string, string>(firstWord, secondWord));
                        }
                    }
                }
            }

            // Sort solutions by total length (shortest first)
            twoWordSolutions.Sort((a, b) => (a.Item1.Length + a.Item2.Length).CompareTo(b.Item1.Length + b.Item2.Length));
        }

        private void DisplayResults(long elapsedMilliseconds)
        {
            // Clear previous results
            SolutionsList.Children.Clear();
            ValidWordsList.Children.Clear();

            // Update stats
            SolutionsFoundText.Text = twoWordSolutions.Count.ToString();
            ValidWordsText.Text = validWords.Count.ToString();
            SolveTimeText.Text = $"{elapsedMilliseconds}ms";

            // Display two-word solutions
            if (twoWordSolutions.Count == 0)
            {
                TextBlock noSolutionsText = new TextBlock
                {
                    Text = "No two-word solutions found",
                    Foreground = new SolidColorBrush(Colors.Gray),
                    FontSize = 14,
                    Margin = new Thickness(0, 0, 0, 5),
                    TextAlignment = TextAlignment.Center
                };
                SolutionsList.Children.Add(noSolutionsText);
            }
            else
            {
                int count = 0;
                foreach (var solution in twoWordSolutions)
                {
                    // Limit to top 50 solutions
                    if (count++ >= 50) break;

                    StackPanel solutionPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(0, 0, 0, 10)
                    };

                    TextBlock firstWordText = new TextBlock
                    {
                        Text = solution.Item1,
                        Foreground = new SolidColorBrush(Colors.LightGreen),
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 0, 5, 0)
                    };

                    TextBlock arrowText = new TextBlock
                    {
                        Text = "→",
                        Foreground = new SolidColorBrush(Colors.Gray),
                        FontSize = 14,
                        Margin = new Thickness(0, 0, 5, 0)
                    };

                    TextBlock secondWordText = new TextBlock
                    {
                        Text = solution.Item2,
                        Foreground = new SolidColorBrush(Colors.LightGreen),
                        FontSize = 14,
                        FontWeight = FontWeights.Bold
                    };

                    solutionPanel.Children.Add(firstWordText);
                    solutionPanel.Children.Add(arrowText);
                    solutionPanel.Children.Add(secondWordText);

                    SolutionsList.Children.Add(solutionPanel);
                }
            }

            // Display valid words (first 100)
            for (int i = 0; i < Math.Min(validWords.Count, 100); i++)
            {
                TextBlock wordBlock = new TextBlock
                {
                    Text = validWords[i],
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 14,
                    Margin = new Thickness(0, 0, 0, 5)
                };

                ValidWordsList.Children.Add(wordBlock);
            }
        }

        private void ShowAnimatedMessage(string message, TextBlock targetTextBlock, double displaySeconds = 3.0)
        {
            // Save original text
            string originalText = targetTextBlock.Text;

            // Step 1: Fade out current text
            DoubleAnimation initialFadeOut = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(0.3),
                AutoReverse = false
            };

            // When initial fade out completes, show new text with fade-in
            initialFadeOut.Completed += (s, args) =>
            {
                // Change text when fully faded out
                targetTextBlock.Text = message;

                // Create fade-in for new message
                DoubleAnimation fadeIn = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = TimeSpan.FromSeconds(0.3),
                    AutoReverse = false
                };

                targetTextBlock.BeginAnimation(OpacityProperty, fadeIn);

                // Create timer to show message for specified duration
                System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(displaySeconds);
                timer.Tick += (sender, e) => {
                    timer.Stop();

                    // Fade out new message
                    DoubleAnimation finalFadeOut = new DoubleAnimation
                    {
                        From = 1.0,
                        To = 0.0,
                        Duration = TimeSpan.FromSeconds(0.3),
                        AutoReverse = false
                    };

                    // When final fade out completes, restore original text with fade-in
                    finalFadeOut.Completed += (s2, args2) =>
                    {
                        // After fading out, restore original text
                        targetTextBlock.Text = originalText;

                        // Create fade-in for original text
                        DoubleAnimation finalFadeIn = new DoubleAnimation
                        {
                            From = 0.0,
                            To = 1.0,
                            Duration = TimeSpan.FromSeconds(0.3),
                            AutoReverse = false
                        };

                        targetTextBlock.BeginAnimation(OpacityProperty, finalFadeIn);
                    };

                    targetTextBlock.BeginAnimation(OpacityProperty, finalFadeOut);
                };

                // Start the timer
                timer.Start();
            };

            // Start the initial fade-out animation
            targetTextBlock.BeginAnimation(OpacityProperty, initialFadeOut);
        }
    }
}
