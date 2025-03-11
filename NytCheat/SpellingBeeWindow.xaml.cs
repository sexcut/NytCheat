using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NytCheatMenu
{
    public partial class SpellingBeeWindow : Window
    {
       
        private List<string> dictionary;
        private List<string> validWords;
        private string currentDictionaryPath = "worsdlist.txt"; // Path to default dictionary

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
                    // Update status text instead of showing a MessageBox
                    DictionaryStatusText.Text = $"Dictionary file not found: {path}";
                    dictionary = new List<string>();

                    // Start a timer to fade the text to 'No dictionary loaded' after 5 seconds
                    var timer = new System.Windows.Threading.DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(5);
                    timer.Tick += (s, e) =>
                    {
                        timer.Stop();
                        FadeToNoDictionaryLoaded();
                    };
                    timer.Start();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dictionary: {ex.Message}");
                dictionary = new List<string>();
                DictionaryStatusText.Text = "Error loading dictionary";
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
                MessageBox.Show("Please enter a center letter.");
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

            // Sort alphabetically
            validWords.Sort();
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

            // Distribute words across the 4 columns
            int wordsPerColumn = (int)Math.Ceiling(validWords.Count / 4.0);

            for (int i = 0; i < validWords.Count; i++)
            {
                string word = validWords[i];

                // Calculate points
                int points = (word.Length == 4) ? 1 : word.Length;

                // Check if it's a pangram (uses all letters)
                string centerLetter = CenterLetterInput.Text.Trim().ToUpper();
                string outerLetters = OuterLettersInput.Text.Trim().ToUpper();
                string allLetters = centerLetter + outerLetters;

                bool isPangram = true;
                foreach (char c in allLetters)
                {
                    if (!word.Contains(c))
                    {
                        isPangram = false;
                        break;
                    }
                }

                if (isPangram)
                {
                    pangrams++;
                    points += 7; // Pangrams get 7 bonus points
                }

                totalPoints += points;

                // Create word display
                TextBlock wordBlock = new TextBlock();
                wordBlock.Text = word;
                wordBlock.Foreground = isPangram ? new SolidColorBrush(Colors.Orange) : new SolidColorBrush(Colors.White);
                wordBlock.FontSize = 14;
                wordBlock.Margin = new Thickness(0, 0, 0, 5);

                // Add to appropriate column
                if (i < wordsPerColumn)
                    WordList1.Children.Add(wordBlock);
                else if (i < wordsPerColumn * 2)
                    WordList2.Children.Add(wordBlock);
                else if (i < wordsPerColumn * 3)
                    WordList3.Children.Add(wordBlock);
                else
                    WordList4.Children.Add(wordBlock);
            }

            // Update stats
            TotalWordsText.Text = validWords.Count.ToString();
            PointsText.Text = totalPoints.ToString();
            PangramsText.Text = pangrams.ToString();

            // Perfect score if all pangrams are found
            PerfectScoreText.Text = (pangrams > 0) ? "Yes" : "No";
            PerfectScoreText.Foreground = (pangrams > 0) ?
                new SolidColorBrush(Colors.LimeGreen) :
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AAAAAA"));
        }
    }
}
