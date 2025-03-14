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
        private string defaultDictionaryPath = "wordlist.txt";


        public LetterBoxedWindow()
        {
            InitializeComponent();
            LoadDictionary(defaultDictionaryPath);
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
                        
                        if (line.Length >= 3 && line.All(c => char.IsLetter(c)))
                        {
                            dictionary.Add(line.Trim().ToUpper());
                        }
                    }

                    bool isCustomDictionary = path != defaultDictionaryPath;

                    if (isCustomDictionary)
                    {
                        DictionaryStatusText.Text = $"Using custom dictionary \n ({dictionary.Count:N0} words)";
                    }
                    else
                    {
                        DictionaryStatusText.Text = $"Using default dictionary \n ({dictionary.Count:N0} words)";
                    }
                }
                else
                {
                    DictionaryStatusText.Text = "Dictionary file not found";
                    dictionary = new List<string>();

                    System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(2);
                    timer.Tick += (sender, e) => {
                        timer.Stop();

                        DoubleAnimation fadeOutAnimation = new DoubleAnimation();
                        fadeOutAnimation.From = 1.0;
                        fadeOutAnimation.To = 0.0;
                        fadeOutAnimation.Duration = TimeSpan.FromSeconds(0.3);

                        fadeOutAnimation.Completed += (s, _) => {
                            DictionaryStatusText.Text = "No dictionary loaded";

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
                ShowAnimatedMessage($"Error loading dictionary: {ex.Message}", DictionaryStatusText, 5.0);
                dictionary = new List<string>();
            }
        }

        private void LoadCustomDictionary_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            dlg.Title = "Select Custom Dictionary";

            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                LoadDictionary(filename);
            }
        }

        private void FindSolutions_Click(object sender, RoutedEventArgs e)
        {
            string topSide = TopSideInput.Text.Trim().ToUpper();
            string rightSide = RightSideInput.Text.Trim().ToUpper();
            string bottomSide = BottomSideInput.Text.Trim().ToUpper();
            string leftSide = LeftSideInput.Text.Trim().ToUpper();

            if (string.IsNullOrEmpty(topSide) || string.IsNullOrEmpty(rightSide) ||
                string.IsNullOrEmpty(bottomSide) || string.IsNullOrEmpty(leftSide))
            {
                
                return;
            }

            letterSides = new Dictionary<char, HashSet<int>>();
            MapLettersToSides(topSide, 0);
            MapLettersToSides(rightSide, 1);
            MapLettersToSides(bottomSide, 2);
            MapLettersToSides(leftSide, 3);

            HashSet<char> allLetters = new HashSet<char>(topSide + rightSide + bottomSide + leftSide);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            FindValidWords(allLetters);

            FindTwoWordSolutions(allLetters);

            stopwatch.Stop();

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
                if (word.Any(c => !allLetters.Contains(c)))
                    continue;

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

            validWords.Sort((a, b) => b.Length.CompareTo(a.Length));
        }

        private void FindTwoWordSolutions(HashSet<char> allLetters)
        {
            twoWordSolutions = new List<Tuple<string, string>>();

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

            foreach (string firstWord in validWords)
            {
                char lastLetter = firstWord[firstWord.Length - 1];

                if (wordsByFirstLetter.ContainsKey(lastLetter))
                {
                    foreach (string secondWord in wordsByFirstLetter[lastLetter])
                    {
                        if (firstWord == secondWord)
                            continue;

                        HashSet<char> usedLetters = new HashSet<char>(firstWord + secondWord);
                        if (usedLetters.Count == allLetters.Count)
                        {
                            twoWordSolutions.Add(new Tuple<string, string>(firstWord, secondWord));
                        }
                    }
                }
            }

            twoWordSolutions.Sort((a, b) => (a.Item1.Length + a.Item2.Length).CompareTo(b.Item1.Length + b.Item2.Length));
        }

        private void DisplayResults(long elapsedMilliseconds)
        {
            SolutionsList.Children.Clear();
            ValidWordsList.Children.Clear();

            SolutionsFoundText.Text = twoWordSolutions.Count.ToString();
            ValidWordsText.Text = validWords.Count.ToString();

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
    }
}
