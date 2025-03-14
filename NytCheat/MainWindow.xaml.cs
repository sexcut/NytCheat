using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NytCheatMenu
{
    public partial class MainWindow : Window
    {
        private string _selectedGame = null; // No default selection

        public MainWindow()
        {
            InitializeComponent();

            // Ensure the launch text starts with gray color
            LaunchText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AAAAAA"));

            // Initialize borders with transparent brushes that can be animated
            CS2SelectedBorder.BorderBrush = new SolidColorBrush(Colors.Transparent);
            VALSelectedBorder.BorderBrush = new SolidColorBrush(Colors.Transparent);
            LaunchSelectedBorder.BorderBrush = new SolidColorBrush(Colors.Transparent);

            UpdateSelectedGameVisuals();
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

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            Border border = sender as Border;
            if (border != null)
            {
                Border hoverBorder = null;

                if (border.Name == "CS2Border")
                    hoverBorder = CS2HoverBorder;
                else if (border.Name == "VALBorder")
                    hoverBorder = VALHoverBorder;

                if (hoverBorder != null)
                {
                    // Create a new mutable brush instead of trying to animate the existing one
                    SolidColorBrush newBrush = new SolidColorBrush(Colors.Transparent);
                    hoverBorder.BorderBrush = newBrush;
                    hoverBorder.Opacity = 0;

                    // Create and start fade-in animation
                    ColorAnimation colorAnimation = new ColorAnimation();
                    colorAnimation.To = (Color)ColorConverter.ConvertFromString("#3366BB");
                    colorAnimation.Duration = TimeSpan.FromSeconds(0.2);

                    DoubleAnimation opacityAnimation = new DoubleAnimation();
                    opacityAnimation.To = 0.5; // Dim blue
                    opacityAnimation.Duration = TimeSpan.FromSeconds(0.2);

                    hoverBorder.BeginAnimation(Border.OpacityProperty, opacityAnimation);
                    newBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
                }
            }
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            Border border = sender as Border;
            if (border != null)
            {
                Border hoverBorder = null;
                string game = null;

                if (border.Name == "CS2Border")
                {
                    hoverBorder = CS2HoverBorder;
                    game = "CS2";
                }
                else if (border.Name == "VALBorder")
                {
                    hoverBorder = VALHoverBorder;
                    game = "VAL";
                }

                if (hoverBorder != null)
                {
                    // Only fade out if this isn't the selected game
                    if (_selectedGame != game)
                    {
                        // Create a new mutable brush instead of trying to animate the existing one
                        SolidColorBrush newBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3366BB"));
                        hoverBorder.BorderBrush = newBrush;

                        // Create and start fade-out animation
                        ColorAnimation colorAnimation = new ColorAnimation();
                        colorAnimation.To = Colors.Transparent;
                        colorAnimation.Duration = TimeSpan.FromSeconds(0.3);

                        DoubleAnimation opacityAnimation = new DoubleAnimation();
                        opacityAnimation.To = 0;
                        opacityAnimation.Duration = TimeSpan.FromSeconds(0.3);

                        hoverBorder.BeginAnimation(Border.OpacityProperty, opacityAnimation);
                        newBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
                    }
                }
            }
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            // Only show hover effect if a game is selected
            if (_selectedGame != null)
            {
                // Create a new mutable brush instead of trying to animate the existing one
                SolidColorBrush newBrush = new SolidColorBrush(Colors.Transparent);
                LaunchHoverBorder.BorderBrush = newBrush;
                LaunchHoverBorder.Opacity = 0;

                ColorAnimation colorAnimation = new ColorAnimation();
                colorAnimation.To = (Color)ColorConverter.ConvertFromString("#3366BB");
                colorAnimation.Duration = TimeSpan.FromSeconds(0.2);

                DoubleAnimation opacityAnimation = new DoubleAnimation();
                opacityAnimation.To = 0.5; // Dim blue
                opacityAnimation.Duration = TimeSpan.FromSeconds(0.2);

                LaunchHoverBorder.BeginAnimation(Border.OpacityProperty, opacityAnimation);
                newBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
            }
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_selectedGame != null)
            {
                // Create a new mutable brush instead of trying to animate the existing one
                SolidColorBrush newBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3366BB"));
                LaunchHoverBorder.BorderBrush = newBrush;

                // Create and start fade-out animation
                ColorAnimation colorAnimation = new ColorAnimation();
                colorAnimation.To = Colors.Transparent;
                colorAnimation.Duration = TimeSpan.FromSeconds(0.3);

                DoubleAnimation opacityAnimation = new DoubleAnimation();
                opacityAnimation.To = 0;
                opacityAnimation.Duration = TimeSpan.FromSeconds(0.3);

                LaunchHoverBorder.BeginAnimation(Border.OpacityProperty, opacityAnimation);
                newBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
            }
        }

        private void GameSelection_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Border clickedBorder = sender as Border;
                if (clickedBorder != null)
                {
                    string clickedGame = clickedBorder.Tag.ToString();

                    // If clicking the already selected game, deselect it
                    if (_selectedGame == clickedGame)
                    {
                        AnimateGameDeselection(_selectedGame);
                        _selectedGame = null;
                        AnimateLaunchButtonDeselection();
                    }
                    else
                    {
                        // IMPORTANT: If another game was previously selected, explicitly deselect it
                        if (_selectedGame != null)
                        {
                            AnimateGameDeselection(_selectedGame);
                        }

                        // Select the new game
                        _selectedGame = clickedGame;
                        AnimateGameSelection(_selectedGame);

                        // If this is the first selection, animate the launch button
                        if (_selectedGame != null)
                        {
                            AnimateLaunchButtonSelection();
                        }
                    }

                    UpdateSelectedGameVisuals();
                }
            }
        }


        private void AnimateGameSelection(string game)
        {
            Border selectionBorder = game == "CS2" ? CS2SelectedBorder : VALSelectedBorder;

            // Create a new brush for animation with a brighter, more pronounced color
            SolidColorBrush selectionBrush = new SolidColorBrush(Colors.Transparent);
            selectionBorder.BorderBrush = selectionBrush;

            // Animate from hover glow to more pronounced glow
            ColorAnimation colorAnimation = new ColorAnimation();
            colorAnimation.To = (Color)ColorConverter.ConvertFromString("#4477FF"); // Brighter blue
            colorAnimation.Duration = TimeSpan.FromSeconds(0.2);

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.To = 1.0; // Full opacity for selected state
            opacityAnimation.Duration = TimeSpan.FromSeconds(0.2);

            selectionBorder.BeginAnimation(Border.OpacityProperty, opacityAnimation);
            selectionBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }

        private void AnimateGameDeselection(string game)
        {
            Border selectionBorder = game == "CS2" ? CS2SelectedBorder : VALSelectedBorder;
            Border hoverBorder = game == "CS2" ? CS2HoverBorder : VALHoverBorder;

            // Ensure both hover and selection effects are removed
            ColorAnimation colorAnimation = new ColorAnimation();
            colorAnimation.To = Colors.Transparent;
            colorAnimation.Duration = TimeSpan.FromSeconds(0.3);

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.To = 0;
            opacityAnimation.Duration = TimeSpan.FromSeconds(0.3);

            selectionBorder.BeginAnimation(Border.OpacityProperty, opacityAnimation);
            hoverBorder.BeginAnimation(Border.OpacityProperty, opacityAnimation);

            // Make sure the brushes are not frozen
            SolidColorBrush selBrush = new SolidColorBrush(Colors.Transparent);
            selectionBorder.BorderBrush = selBrush;
            selBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }


        private void AnimateLaunchButtonSelection()
        {
            // Create a new brush for animation
            SolidColorBrush selectionBrush = new SolidColorBrush(Colors.Transparent);
            LaunchSelectedBorder.BorderBrush = selectionBrush;

            // Create and start animations with brighter blue for pronounced effect
            ColorAnimation colorAnimation = new ColorAnimation();
            colorAnimation.To = (Color)ColorConverter.ConvertFromString("#4477FF");
            colorAnimation.Duration = TimeSpan.FromSeconds(0.3);

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.To = 0.9;
            opacityAnimation.Duration = TimeSpan.FromSeconds(0.3);

            LaunchSelectedBorder.BeginAnimation(Border.OpacityProperty, opacityAnimation);
            selectionBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }


        private void AnimateLaunchButtonDeselection()
        {
            // Create fade-out animations
            ColorAnimation colorAnimation = new ColorAnimation();
            colorAnimation.To = Colors.Transparent;
            colorAnimation.Duration = TimeSpan.FromSeconds(0.3);

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.To = 0;
            opacityAnimation.Duration = TimeSpan.FromSeconds(0.3);

            LaunchSelectedBorder.BeginAnimation(Border.OpacityProperty, opacityAnimation);
            (LaunchSelectedBorder.BorderBrush as SolidColorBrush).BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }

        private void UpdateSelectedGameVisuals()
        {
            string newButtonText = "Select a game";

            // update based on selection
            if (_selectedGame == "CS2")
            {
                newButtonText = "Launch Spelling Bee";
            }
            else if (_selectedGame == "VAL")
            {
                newButtonText = "Launch LetterBoxed";
            }

            // animate the text change
            AnimateButtonTextChange(newButtonText);
        }

        private void AnimateButtonTextChange(string newText)
        {
            // stop running animations first
            LaunchText.BeginAnimation(TextBlock.OpacityProperty, null);
            NewLaunchText.BeginAnimation(TextBlock.OpacityProperty, null);

            // set up the new text with no opacity
            NewLaunchText.Text = newText;

            // Set the appropriate color for the new text
            if (_selectedGame == null)
            {
                NewLaunchText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AAAAAA"));
            }
            else
            {
                NewLaunchText.Foreground = new SolidColorBrush(Colors.White);
            }

            // create fade out animation for current text
            DoubleAnimation fadeOutAnimation = new DoubleAnimation();
            fadeOutAnimation.From = LaunchText.Opacity; // use current opacity instead of hardcoded value
            fadeOutAnimation.To = 0.0;
            fadeOutAnimation.Duration = TimeSpan.FromSeconds(0.2);

            // create fade in animation for new text
            DoubleAnimation fadeInAnimation = new DoubleAnimation();
            fadeInAnimation.From = 0.0;
            fadeInAnimation.To = 1.0;
            fadeInAnimation.Duration = TimeSpan.FromSeconds(0.2);

            // set up completed event to switch the texts
            fadeOutAnimation.Completed += (s, e) => {
                // Swap text and reset animation
                LaunchText.Text = newText;
                LaunchText.Opacity = 1.0;
                NewLaunchText.Opacity = 0.0;

                // Transfer the foreground color as well
                LaunchText.Foreground = NewLaunchText.Foreground.Clone();
            };

            // Start the animations
            LaunchText.BeginAnimation(TextBlock.OpacityProperty, fadeOutAnimation);
            NewLaunchText.BeginAnimation(TextBlock.OpacityProperty, fadeInAnimation);
        }


        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedGame == null)
            {
                MessageBox.Show("Please select a game first.");
                return;
            }

            switch (_selectedGame)
            {
                case "CS2":
                    // Launch Spelling Bee window
                    SpellingBeeWindow spellingBeeWindow = new SpellingBeeWindow();
                    spellingBeeWindow.Owner = this;
                    spellingBeeWindow.Show();
                    this.Hide(); // Optional: hide the main window while cheat window is open
                    break;
                case "VAL":
                    // Launch LetterBoxed window
            LetterBoxedWindow letterBoxedWindow = new LetterBoxedWindow();
            letterBoxedWindow.Owner = this;
            letterBoxedWindow.Show();
            this.Hide(); // Optional: hide the main window while cheat window is open
            break;
                default:
                    MessageBox.Show("Please select a game first.");
                    break;
            }
        }
    }
}
