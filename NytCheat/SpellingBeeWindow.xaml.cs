using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NytCheatMenu
{
    public partial class SpellingBeeWindow : Window
    {
       
        private List<string> dictionary;
        private List<string> validWords;
        private string currentDictionaryPath = "wordlist.txt"; // Path to default dictionary

        public SpellingBeeWindow()
        {
            InitializeComponent();
            LoadDictionary(currentDictionaryPath); // Load default dictionary
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
                        // and at least 4 letters long (Spelling Bee requirement)
                        if (line.Length >= 4 && line.All(c => char.IsLetter(c)))
                        {
                            dictionary.Add(line.Trim().ToUpper());
                        }
                    }

                    // Update status
                    if (path == currentDictionaryPath)
                    {
                        DictionaryStatusText.Text = $"using default dictionary \n ({dictionary.Count:N0} words)";
                    }
                    else
                    {
                        DictionaryStatusText.Text = $"using custom dictionary \n ({dictionary.Count:N0} words)";
                        currentDictionaryPath = path;
                    }
                }
                else
                {
                    // Set initial text
                    DictionaryStatusText.Text = "Dictionary file not found";
                    dictionary = new List<string>();

                    // Create a timer to wait 2 seconds
                    System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(2.5);
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
                MessageBox.Show($"Error loading dictionary: {ex.Message}");
                dictionary = new List<string>();
                ShowAnimatedMessage($"Error loading dictionary: {ex.Message}", DictionaryStatusText, 5.0);

            }
        }

        // Method to fade the text to 'No dictionary loaded'
        private void FadeToNoDictionaryLoaded()
        {
            var fadeOutAnimation = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(1),
                AutoReverse = false
            };

            fadeOutAnimation.Completed += (s, e) =>
            {
                DictionaryStatusText.Text = "No dictionary loaded";
                DictionaryStatusText.BeginAnimation(OpacityProperty, null); // Reset animation
                DictionaryStatusText.Opacity = 1.0; // Reset opacity
            };

            DictionaryStatusText.BeginAnimation(OpacityProperty, fadeOutAnimation);
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

        private void FindWords_Click(object sender, RoutedEventArgs e)
        {
            string centerLetter = CenterLetterInput.Text.Trim().ToUpper();
            string outerLetters = OuterLettersInput.Text.Trim().ToUpper();

            // Validate input
            if (string.IsNullOrEmpty(centerLetter))
            {
                ShowAnimatedMessage("Please enter a center letter.", DictionaryStatusText);

                return;
            }

            if (outerLetters.Length > 6)
            {
                MessageBox.Show("You can only have up to 6 outer letters.");
                return;
            }

            // Clear previous results
            ClearResults();

            // Find valid words
            FindValidWords(centerLetter[0], outerLetters);

            // Display results
            DisplayResults();
        }

        private void ClearResults()
        {
            WordList1.Children.Clear();
            WordList2.Children.Clear();
            WordList3.Children.Clear();
            WordList4.Children.Clear();

            TotalWordsText.Text = "0";
            PointsText.Text = "0";
            PangramsText.Text = "0";
            PerfectScoreText.Text = "No";
        }

        private void FindValidWords(char centerLetter, string outerLetters)
        {
            string allLetters = centerLetter + outerLetters;
            validWords = new List<string>();

            foreach (string word in dictionary)
            {
                // Must be at least 4 letters
                if (word.Length < 4)
                    continue;

                // Must contain center letter
                if (!word.Contains(centerLetter))
                    continue;

                // All letters must be in our allowed set
                bool isValid = true;
                foreach (char c in word)
                {
                    if (!allLetters.Contains(c))
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

            // Sort words: pangrams first (sorted by length), then other words by length
            SortWordsByPangramAndLength(allLetters);
        }

        private void SortWordsByPangramAndLength(string allLetters)
        {
            // Create a comparer that prioritizes pangrams, then word length
            validWords.Sort((a, b) =>
            {
                bool aIsPangram = IsPangram(a, allLetters);
                bool bIsPangram = IsPangram(b, allLetters);

                // If one is a pangram and the other isn't, pangram comes first
                if (aIsPangram && !bIsPangram) return -1;
                if (!aIsPangram && bIsPangram) return 1;

                // Both are pangrams or both are not pangrams, so sort by length
                return b.Length.CompareTo(a.Length); // Longer words first
            });
        }

        private bool IsPangram(string word, string allLetters)
        {
            // A pangram uses all the letters in the puzzle
            foreach (char c in allLetters)
            {
                if (!word.Contains(c))
                    return false;
            }
            return true;
        }

        private void DisplayResults()
        {
            if (validWords.Count == 0)
            {
                MessageBox.Show("No words found with these letters.");
                return;
            }

            int totalPoints = 0;
            int pangrams = 0;

            // Clear previous results
            WordList1.Children.Clear();
            WordList2.Children.Clear();
            WordList3.Children.Clear();
            WordList4.Children.Clear();

            // Distribute words across the 4 columns
            int wordsPerColumn = (int)Math.Ceiling(validWords.Count / 4.0);

            for (int i = 0; i < validWords.Count; i++)
            {
                string word = validWords[i];

                // calculate points
                int points = (word.Length == 4) ? 1 : word.Length;

                // check if itss a pangram thingy
                string allLetters = CenterLetterInput.Text.Trim().ToUpper() + OuterLettersInput.Text.Trim().ToUpper();
                bool isPangram = IsPangram(word, allLetters);

                if (isPangram)
                {
                    pangrams++;
                    points += 7; // pangrams are 7 points.. i think
                }

                totalPoints += points;

                // create word display
                TextBlock wordBlock = new TextBlock();
                wordBlock.Text = word;
                wordBlock.Foreground = isPangram ? new SolidColorBrush(Colors.Orange) : new SolidColorBrush(Colors.White);
                wordBlock.FontSize = 14;
                wordBlock.Margin = new Thickness(0, 0, 0, 5);

                // add to appropriate colllumn
                if (i < wordsPerColumn)
                    WordList1.Children.Add(wordBlock);
                else if (i < wordsPerColumn * 2)
                    WordList2.Children.Add(wordBlock);
                else if (i < wordsPerColumn * 3)
                    WordList3.Children.Add(wordBlock);
                else
                    WordList4.Children.Add(wordBlock);
            }

            // update all stats
            TotalWordsText.Text = validWords.Count.ToString();
            PointsText.Text = totalPoints.ToString();
            PangramsText.Text = pangrams.ToString();

            // Perfect score if all words are found (this might need refinement)
            PerfectScoreText.Text = "No";
            PerfectScoreText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AAAAAA"));
        }

    }
}
