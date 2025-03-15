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
        private string currentDictionaryPath = "wordlist.txt";     

        public SpellingBeeWindow()
        {
            InitializeComponent();
            LoadDictionary(currentDictionaryPath);    
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
                        if (line.Length >= 4 && line.All(c => char.IsLetter(c)))
                        {
                            dictionary.Add(line.Trim().ToUpper());
                        }
                    }

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
                    DictionaryStatusText.Text = "dictionary file not found";
                    dictionary = new List<string>();

                    System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(2.5);
                    timer.Tick += (sender, e) => {
                        timer.Stop();

                        DoubleAnimation fadeOutAnimation = new DoubleAnimation();
                        fadeOutAnimation.From = 1.0;
                        fadeOutAnimation.To = 0.0;
                        fadeOutAnimation.Duration = TimeSpan.FromSeconds(0.3);

                        fadeOutAnimation.Completed += (s, _) => {
                            DictionaryStatusText.Text = "no dictionary loaded";

                            DoubleAnimation fadeInAnimation = new DoubleAnimation();
                            fadeInAnimation.From = 0.0;
                            fadeInAnimation.To = 1.0;
                            fadeInAnimation.Duration = TimeSpan.FromSeconds(0.3);

                            DictionaryStatusText.BeginAnimation(TextBlock.OpacityProperty, fadeInAnimation);
                        };

                        DictionaryStatusText.BeginAnimation(TextBlock.OpacityProperty, fadeOutAnimation);
                    };

                    timer.Start();
                }




            }
            catch (Exception ex)
            {
                MessageBox.Show($"error loading dictionary: {ex.Message}");
                dictionary = new List<string>();
                ShowAnimatedMessage($"error loading dictionary: {ex.Message}", DictionaryStatusText, 2.3);

            }
        }

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
                DictionaryStatusText.Text = "no dictionary loaded";
                DictionaryStatusText.BeginAnimation(OpacityProperty, null);   
                DictionaryStatusText.Opacity = 1.0;   
            };

            DictionaryStatusText.BeginAnimation(OpacityProperty, fadeOutAnimation);
        }
        private void ShowAnimatedMessage(string message, TextBlock targetTextBlock, double displaySeconds = 3.0)
        {
            string originalText = targetTextBlock.Text;

            DoubleAnimation initialFadeOut = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(0.3),
                AutoReverse = false
            };

            initialFadeOut.Completed += (s, args) =>
            {
                targetTextBlock.Text = message;

                DoubleAnimation fadeIn = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = TimeSpan.FromSeconds(0.3),
                    AutoReverse = false
                };

                targetTextBlock.BeginAnimation(OpacityProperty, fadeIn);

                System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(displaySeconds);
                timer.Tick += (sender, e) => {
                    timer.Stop();

                    DoubleAnimation finalFadeOut = new DoubleAnimation
                    {
                        From = 1.0,
                        To = 0.0,
                        Duration = TimeSpan.FromSeconds(0.3),
                        AutoReverse = false
                    };

                    finalFadeOut.Completed += (s2, args2) =>
                    {
                        targetTextBlock.Text = originalText;

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

                timer.Start();
            };

            targetTextBlock.BeginAnimation(OpacityProperty, initialFadeOut);
        }

        private void LoadCustomDictionary_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            dlg.Title = "select Custom wordlist";

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

            if (string.IsNullOrEmpty(centerLetter))
            {
                ShowAnimatedMessage("please enter a center letter.", DictionaryStatusText, 2.3);

                return;
            }

            if (outerLetters.Length > 6)
            {
                MessageBox.Show("you can only have up to 6 outer letters.");
                return;
            }

            ClearResults();

            FindValidWords(centerLetter[0], outerLetters);

            DisplayResults();
        }
        private void SaveResults_Click(object sender, RoutedEventArgs e)
        {
            // Initialize collections if null to prevent NullReferenceException
            if (validWords == null)
                validWords = new List<string>();

            // Check if there's anything to save
            if (validWords.Count == 0)
            {
                ShowAnimatedMessage("No results to save", DictionaryStatusText, 2.3);
                return;
            }

            // Create SaveFileDialog
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "Text Files (*.txt)|*.txt";
            saveFileDialog.DefaultExt = "txt";
            saveFileDialog.Title = "save Spelling Bee results";
            saveFileDialog.FileName = "spelling bee results";

            // Show dialog and process result
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Get the letters for determining pangrams
                    string centerLetter = CenterLetterInput.Text.Trim().ToUpper();
                    string outerLetters = OuterLettersInput.Text.Trim().ToUpper();
                    string allLetters = centerLetter + outerLetters;

                    using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                    {
                        // Write header
                        writer.WriteLine("Spelling Bee results - " + DateTime.Now.ToString());
                        writer.WriteLine($"center letter: {centerLetter}");
                        writer.WriteLine($"outer letters: {outerLetters}");
                        writer.WriteLine();

                        // Write statistics
                        writer.WriteLine("STATISTICS:");
                        writer.WriteLine("-----------");
                        writer.WriteLine($"total words: {validWords.Count}");

                        // Calculate total points
                        int totalPoints = 0;
                        int pangrams = 0;
                        foreach (string word in validWords)
                        {
                            bool isPangram = IsPangram(word, allLetters);
                            int points = (word.Length == 4) ? 1 : word.Length;
                            if (isPangram)
                            {
                                points += 7;
                                pangrams++;
                            }
                            totalPoints += points;
                        }

                        writer.WriteLine($"total Points: {totalPoints}");
                        writer.WriteLine($"pangrams: {pangrams}");
                        writer.WriteLine();

                        // Write all words, marking pangrams
                        writer.WriteLine("WORDS:");
                        writer.WriteLine("------");

                        if (validWords.Count > 0)
                        {
                            foreach (string word in validWords)
                            {
                                bool isPangram = IsPangram(word, allLetters);
                                if (isPangram)
                                    writer.WriteLine($"{word} (pangram)");
                                else
                                    writer.WriteLine(word);
                            }
                        }
                        else
                        {
                            writer.WriteLine("no words found.");
                        }
                    }

                    ShowAnimatedMessage("results saved successfully", DictionaryStatusText, 2.3);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"fatal error saving file! {ex.Message}");
                    ShowAnimatedMessage($"fatal error saving file! {ex.Message}", DictionaryStatusText, 2.3);
                }
            }
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
        }

        private void FindValidWords(char centerLetter, string outerLetters)
        {
            string allLetters = centerLetter + outerLetters;
            validWords = new List<string>();

            foreach (string word in dictionary)
            {
                if (word.Length < 4)
                    continue;

                if (!word.Contains(centerLetter))
                    continue;

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

            SortWordsByPangramAndLength(allLetters);
        }

        private void SortWordsByPangramAndLength(string allLetters)
        {
            validWords.Sort((a, b) =>
            {
                bool aIsPangram = IsPangram(a, allLetters);
                bool bIsPangram = IsPangram(b, allLetters);

                if (aIsPangram && !bIsPangram) return -1;
                if (!aIsPangram && bIsPangram) return 1;

                return b.Length.CompareTo(a.Length);    
            });
        }

        private bool IsPangram(string word, string allLetters)
        {
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
                MessageBox.Show("no words found.");
                return;
            }

            int totalPoints = 0;
            int pangrams = 0;

            WordList1.Children.Clear();
            WordList2.Children.Clear();
            WordList3.Children.Clear();
            WordList4.Children.Clear();

            int wordsPerColumn = (int)Math.Ceiling(validWords.Count / 4.0);

            for (int i = 0; i < validWords.Count; i++)
            {
                string word = validWords[i];

                int points = (word.Length == 4) ? 1 : word.Length;

                string allLetters = CenterLetterInput.Text.Trim().ToUpper() + OuterLettersInput.Text.Trim().ToUpper();
                bool isPangram = IsPangram(word, allLetters);

                if (isPangram)
                {
                    pangrams++;
                    points += 7;       
                }

                totalPoints += points;

                TextBlock wordBlock = new TextBlock();
                wordBlock.Text = word;
                wordBlock.Foreground = isPangram ? new SolidColorBrush(Colors.Orange) : new SolidColorBrush(Colors.White);
                wordBlock.FontSize = 14;
                wordBlock.Margin = new Thickness(0, 0, 0, 5);

                if (i < wordsPerColumn)
                    WordList1.Children.Add(wordBlock);
                else if (i < wordsPerColumn * 2)
                    WordList2.Children.Add(wordBlock);
                else if (i < wordsPerColumn * 3)
                    WordList3.Children.Add(wordBlock);
                else
                    WordList4.Children.Add(wordBlock);
            }

            TotalWordsText.Text = validWords.Count.ToString();
            PointsText.Text = totalPoints.ToString();
            PangramsText.Text = pangrams.ToString();
        }

    }
}
