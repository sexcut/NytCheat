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
        private string _selectedGame = null;    

        public MainWindow()
        {
            InitializeComponent();

            LaunchText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AAAAAA"));

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
                    SolidColorBrush newBrush = new SolidColorBrush(Colors.Transparent);
                    hoverBorder.BorderBrush = newBrush;
                    hoverBorder.Opacity = 0;

                    ColorAnimation colorAnimation = new ColorAnimation();
                    colorAnimation.To = (Color)ColorConverter.ConvertFromString("#3366BB");
                    colorAnimation.Duration = TimeSpan.FromSeconds(0.2);

                    DoubleAnimation opacityAnimation = new DoubleAnimation();
                    opacityAnimation.To = 0.5;   
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
                    if (_selectedGame != game)
                    {
                        SolidColorBrush newBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3366BB"));
                        hoverBorder.BorderBrush = newBrush;

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
            if (_selectedGame != null)
            {
                SolidColorBrush newBrush = new SolidColorBrush(Colors.Transparent);
                LaunchHoverBorder.BorderBrush = newBrush;
                LaunchHoverBorder.Opacity = 0;

                ColorAnimation colorAnimation = new ColorAnimation();
                colorAnimation.To = (Color)ColorConverter.ConvertFromString("#3366BB");
                colorAnimation.Duration = TimeSpan.FromSeconds(0.2);

                DoubleAnimation opacityAnimation = new DoubleAnimation();
                opacityAnimation.To = 0.5;   
                opacityAnimation.Duration = TimeSpan.FromSeconds(0.2);

                LaunchHoverBorder.BeginAnimation(Border.OpacityProperty, opacityAnimation);
                newBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
            }
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_selectedGame != null)
            {
                SolidColorBrush newBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3366BB"));
                LaunchHoverBorder.BorderBrush = newBrush;

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

                    if (_selectedGame == clickedGame)
                    {
                        AnimateGameDeselection(_selectedGame);
                        _selectedGame = null;
                        AnimateLaunchButtonDeselection();
                    }
                    else
                    {
                        if (_selectedGame != null)
                        {
                            AnimateGameDeselection(_selectedGame);
                        }

                        _selectedGame = clickedGame;
                        AnimateGameSelection(_selectedGame);

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

            SolidColorBrush selectionBrush = new SolidColorBrush(Colors.Transparent);
            selectionBorder.BorderBrush = selectionBrush;

            ColorAnimation colorAnimation = new ColorAnimation();
            colorAnimation.To = (Color)ColorConverter.ConvertFromString("#4477FF");   
            colorAnimation.Duration = TimeSpan.FromSeconds(0.2);

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.To = 1.0;      
            opacityAnimation.Duration = TimeSpan.FromSeconds(0.2);

            selectionBorder.BeginAnimation(Border.OpacityProperty, opacityAnimation);
            selectionBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }

        private void AnimateGameDeselection(string game)
        {
            Border selectionBorder = game == "CS2" ? CS2SelectedBorder : VALSelectedBorder;
            Border hoverBorder = game == "CS2" ? CS2HoverBorder : VALHoverBorder;

            ColorAnimation colorAnimation = new ColorAnimation();
            colorAnimation.To = Colors.Transparent;
            colorAnimation.Duration = TimeSpan.FromSeconds(0.3);

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.To = 0;
            opacityAnimation.Duration = TimeSpan.FromSeconds(0.3);

            selectionBorder.BeginAnimation(Border.OpacityProperty, opacityAnimation);
            hoverBorder.BeginAnimation(Border.OpacityProperty, opacityAnimation);

            SolidColorBrush selBrush = new SolidColorBrush(Colors.Transparent);
            selectionBorder.BorderBrush = selBrush;
            selBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }


        private void AnimateLaunchButtonSelection()
        {
            SolidColorBrush selectionBrush = new SolidColorBrush(Colors.Transparent);
            LaunchSelectedBorder.BorderBrush = selectionBrush;

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

            if (_selectedGame == "CS2")
            {
                newButtonText = "Launch Spelling Bee";
            }
            else if (_selectedGame == "VAL")
            {
                newButtonText = "Launch LetterBoxed";
            }

            AnimateButtonTextChange(newButtonText);
        }

        private void AnimateButtonTextChange(string newText)
        {
            LaunchText.BeginAnimation(TextBlock.OpacityProperty, null);
            NewLaunchText.BeginAnimation(TextBlock.OpacityProperty, null);

            NewLaunchText.Text = newText;

            if (_selectedGame == null)
            {
                NewLaunchText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AAAAAA"));
            }
            else
            {
                NewLaunchText.Foreground = new SolidColorBrush(Colors.White);
            }

            DoubleAnimation fadeOutAnimation = new DoubleAnimation();
            fadeOutAnimation.From = LaunchText.Opacity;        
            fadeOutAnimation.To = 0.0;
            fadeOutAnimation.Duration = TimeSpan.FromSeconds(0.2);

            DoubleAnimation fadeInAnimation = new DoubleAnimation();
            fadeInAnimation.From = 0.0;
            fadeInAnimation.To = 1.0;
            fadeInAnimation.Duration = TimeSpan.FromSeconds(0.2);

            fadeOutAnimation.Completed += (s, e) => {
                LaunchText.Text = newText;
                LaunchText.Opacity = 1.0;
                NewLaunchText.Opacity = 0.0;

                LaunchText.Foreground = NewLaunchText.Foreground.Clone();
            };

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
                    SpellingBeeWindow spellingBeeWindow = new SpellingBeeWindow();
                    spellingBeeWindow.Owner = this;
                    spellingBeeWindow.Show();
                    this.Hide();           
                    break;
                case "VAL":
            LetterBoxedWindow letterBoxedWindow = new LetterBoxedWindow();
            letterBoxedWindow.Owner = this;
            letterBoxedWindow.Show();
            this.Hide();           
            break;
                default:
                    MessageBox.Show("Please select a game first.");
                    break;
            }
        }
    }
}
