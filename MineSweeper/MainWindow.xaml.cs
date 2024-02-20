using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MineSweeper
{
    public partial class MainWindow : Window
    {
        private List<MineSweeperButton> FieldButtons;
        private Random rnd = new Random();
        private Dictionary<MineSweeperButton, States> buttonStates = new Dictionary<MineSweeperButton, States>();
        private int totalSafeSquares;
        private int revealedSquares;
        private bool isFirstMove;
        private int rows = 16;
        private int columns = 16;
        static SolidColorBrush Redish = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FB007E"));
        static SolidColorBrush GreyColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A9A9A9"));
        SolidColorBrush selectedBrush = Redish;
        private bool SoundOn = true;

        public MainWindow()
        {
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.Fant); // this somehow didn't work with the scaling of the icon
            // Load the icon image file
            BitmapImage icon = new BitmapImage();
            icon.BeginInit();
            icon.UriSource = new Uri("Sprites/MineIcon.png", UriKind.Relative);
            icon.EndInit();
            Icon = icon;

            this.SizeChanged += Window_SizeChanged;
            PATH_DataGrid.Loaded += PATH_DataGrid_Loaded; // Handle the Loaded event
            Sounds.PreloadSounds();
        }

        private void PATH_DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize fields after the PATH_DataGrid is loaded and rendered
            InitializeFields();
        }

        private void InitializeFields()
        {
            // Clear the buttonStates dictionary before reinitializing
            buttonStates.Clear();

            // Reinitialize FieldButtons list
            FieldButtons = new List<MineSweeperButton>();

            // Calculate an appropriate font size based on button dimensions
            double buttonWidth = PATH_DataGrid.ActualWidth / columns;
            double buttonHeight = PATH_DataGrid.ActualHeight / rows;
            double fontSize = Math.Min(buttonWidth, buttonHeight) / 1.5;

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    MineSweeperButton button = new MineSweeperButton();
                    button.Style = (Style)Resources["RoundedButton"];
                    button.Click += LeftClick;
                    button.MouseRightButtonDown += ButtonRightClick;
                    Grid.SetRow(button, row);
                    Grid.SetColumn(button, column);
                    PATH_DataGrid.Children.Add(button);
                    FieldButtons.Add(button);
                    button.FontSize = fontSize;
                    button.Foreground = Brushes.White;
                    button.Background = selectedBrush;

                    // Initialize button state and add it to the dictionary
                    buttonStates.Add(button, States.unrevealed);
                }
            }

            totalSafeSquares = 0;
            revealedSquares = 0;
            isFirstMove = true;
        }



        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var clientAreaWidth = this.ActualWidth - SystemParameters.WindowNonClientFrameThickness.Left - SystemParameters.WindowNonClientFrameThickness.Right;
            var clientAreaHeight = this.ActualHeight - SystemParameters.WindowNonClientFrameThickness.Top - SystemParameters.WindowNonClientFrameThickness.Bottom - 10;

            double minDimension = Math.Min(clientAreaWidth, clientAreaHeight);

            PATH_DataGrid.Width = minDimension;
            PATH_DataGrid.Height = minDimension;
        }

        private void FillWithMines(MineSweeperButton firstClickButton)
        {
            List<MineSweeperButton> excludedButtons = GetNeighbors(firstClickButton);

            foreach (var FieldButton in FieldButtons)
            {
                if (!excludedButtons.Contains(FieldButton) && GetRandomInt() == 3)
                {
                    FieldButton.ButtonState = States.bomb;
                }
            }
            totalSafeSquares = (rows * columns) - FieldButtons.Count(b => b.ButtonState == States.bomb);
        }

        private void LeftClick(object sender, RoutedEventArgs e)
        {
            MineSweeperButton clickedButton = (MineSweeperButton)sender;

            if (isFirstMove)
            {
                isFirstMove = false;
                FillWithMines(clickedButton);
            }

            if (buttonStates[clickedButton] == States.flagged)
            {
                return;
            }

            if (clickedButton.ButtonState == States.bomb)
            {
                EndGame();
            }
            else
            {
                clickedButton.Background = null;
                ShowAdjacentInitial(clickedButton);
            }
        }

        private void ButtonRightClick(object sender, MouseButtonEventArgs e)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string FlagPath = System.IO.Path.Combine(baseDirectory, "Sprites", "Flag.png");
            MineSweeperButton clickedButton = (MineSweeperButton)sender;

            if (buttonStates[clickedButton] == States.flagged)
            {
                clickedButton.Content = "";
                buttonStates[clickedButton] = States.unrevealed;
            }
            else
            {
                Image FlagImage = new Image();
                FlagImage.Source = new BitmapImage(new Uri(FlagPath));
                clickedButton.Content = FlagImage;
                buttonStates[clickedButton] = States.flagged;
                if (SoundOn) Sounds.PlayFlagSound();
            }

            e.Handled = true;
        }

        private List<MineSweeperButton> GetNeighbors(MineSweeperButton button)
        {
            var neighbors = new List<MineSweeperButton>();
            int row = Grid.GetRow(button);
            int column = Grid.GetColumn(button);

            for (int i = row - 1; i <= row + 1; i++)
            {
                for (int j = column - 1; j <= column + 1; j++)
                {
                    if (i >= 0 && i < rows && j >= 0 && j < columns)
                    {
                        MineSweeperButton neighborButton = GetButtonAtPosition(i, j);
                        if (neighborButton != null)
                        {
                            neighbors.Add(neighborButton);
                        }
                    }
                }
            }
            return neighbors;
        }

        private int GetRandomInt()
        {
            return rnd.Next(1, 8);
        }

        // Initial method
        private void ShowAdjacentInitial(MineSweeperButton clickedButton)
        {
            // Initialize a queue to handle the recursion
            Queue<MineSweeperButton> queue = new Queue<MineSweeperButton>();
            queue.Enqueue(clickedButton);

            // Track whether the recursive call is made
            bool isRecursiveCallMade = false;

            // While there are buttons in the queue
            while (queue.Count > 0)
            {
                // Dequeue the button
                MineSweeperButton currentButton = queue.Dequeue();
                int row = Grid.GetRow(currentButton);
                int column = Grid.GetColumn(currentButton);
                int minesCount = 0;

                if (!currentButton.IsEnabled)
                    continue;

                for (int i = row - 1; i <= row + 1; i++)
                {
                    for (int j = column - 1; j <= column + 1; j++)
                    {
                        if (i >= 0 && i < rows && j >= 0 && j < columns && !(i == row && j == column))
                        {
                            MineSweeperButton neighborButton = GetButtonAtPosition(i, j);
                            if (neighborButton != null && neighborButton.ButtonState == States.bomb)
                            {
                                minesCount++;
                            }
                        }
                    }
                }

                currentButton.Content = minesCount > 0 ? minesCount.ToString() : "";
                currentButton.IsEnabled = false;
                revealedSquares++;

                if (minesCount == 0)
                {
                    // Check if any adjacent squares are already revealed
                    for (int i = row - 1; i <= row + 1; i++)
                    {
                        for (int j = column - 1; j <= column + 1; j++)
                        {
                            if (i >= 0 && i < rows && j >= 0 && j < columns && !(i == row && j == column))
                            {
                                MineSweeperButton neighborButton = GetButtonAtPosition(i, j);
                                if (neighborButton != null && neighborButton.IsEnabled)
                                {
                                    if (buttonStates[neighborButton] == States.flagged)
                                    {
                                        continue;
                                    }
                                    neighborButton.Background = null;
                                    queue.Enqueue(neighborButton); // Enqueue adjacent buttons for further processing
                                    isRecursiveCallMade = true;
                                }
                            }
                        }
                    }
                }
            }

            // Check if all safe squares are revealed and win the game
            if (revealedSquares == totalSafeSquares)
            {
                WinGame();
            }

            // Play the sound based on whether the recursive call is made
            if (SoundOn && isRecursiveCallMade)
            {
                Sounds.PlayManyBlockBreakSound(); // Play the many blocks sound
            }
            else if (SoundOn)
            {
                Sounds.PlaySingleBlockBreakSound(); // Play the single block sound
            }
        }

        // Recursive method
        private void ShowAdjacentRecursive(MineSweeperButton clickedButton)
        {
            int row = Grid.GetRow(clickedButton);
            int column = Grid.GetColumn(clickedButton);
            int minesCount = 0;

            if (!clickedButton.IsEnabled)
                return;

            for (int i = row - 1; i <= row + 1; i++)
            {
                for (int j = column - 1; j <= column + 1; j++)
                {
                    if (i >= 0 && i < rows && j >= 0 && j < columns && !(i == row && j == column))
                    {
                        MineSweeperButton neighborButton = GetButtonAtPosition(i, j);
                        if (neighborButton != null && neighborButton.ButtonState == States.bomb)
                        {
                            minesCount++;
                        }
                    }
                }
            }

            clickedButton.Content = minesCount > 0 ? minesCount.ToString() : "";
            clickedButton.IsEnabled = false;
            revealedSquares++;

            if (minesCount == 0)
            {
                for (int i = row - 1; i <= row + 1; i++)
                {
                    for (int j = column - 1; j <= column + 1; j++)
                    {
                        if (i >= 0 && i < rows && j >= 0 && j < columns && !(i == row && j == column))
                        {
                            MineSweeperButton neighborButton = GetButtonAtPosition(i, j);
                            if (neighborButton != null && neighborButton.IsEnabled)
                            {
                                if (buttonStates[neighborButton] == States.flagged)
                                {
                                    continue;
                                }
                                neighborButton.Background = null;
                                ShowAdjacentRecursive(neighborButton); // Recursive call here
                            }
                        }
                    }
                }
            }
        }


        private MineSweeperButton GetButtonAtPosition(int row, int column)
        {
            foreach (var button in FieldButtons)
            {
                if (Grid.GetRow(button) == row && Grid.GetColumn(button) == column)
                {
                    return button;
                }
            }
            return null;
        }

        private void EndGame()
        {
            if (SoundOn) Sounds.PlayBombSound();
            Task.Delay(200).Wait();
            MessageBox.Show("Game Over. Click Okay to restart");
            PATH_DataGrid.Children.Clear();
            buttonStates.Clear();

            RestartGame();
            InitializeFields();
        }

        private void WinGame()
        {
            MessageBox.Show("Congratulations! You've cleared all safe squares!");
            PATH_DataGrid.Children.Clear();
            buttonStates.Clear();

            RestartGame();
            InitializeFields();
        }

        private void RestartGame()
        {
            foreach (var FieldButton in FieldButtons)
            {
                FieldButton.ButtonState = States.unrevealed;
                FieldButton.Content = "";
                FieldButton.IsEnabled = true;
                FieldButton.Background = selectedBrush;
            }

            // Reset other game-related variables
            isFirstMove = true;
            revealedSquares = 0;
        }

        private void GridSizeSelection_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = e.OriginalSource as MenuItem;
            if (menuItem != null)
            {
                string name = menuItem.Name;
                switch (name)
                {
                    case "Small":
                        rows = 8;
                        columns = 8;
                        break;
                    case "Medium":
                        rows = 16;
                        columns = 16;
                        break;
                    case "Large":
                        rows = 24;
                        columns = 24;
                        break;
                    case "Custom":
                        ShowCustomGridSizeDialog();
                        return; // Exit the event handler after showing the dialog
                }

                PATH_DataGrid.Children.Clear();
                buttonStates.Clear();

                RestartGame();
                InitializeFields();
            }
        }

        private void ShowCustomGridSizeDialog()
        {
            // Instantiate the custom input dialog
            CustomInputDialog customDialog = new CustomInputDialog();

            // Show the dialog modally
            bool? result = customDialog.ShowDialog();

            // Check if the user clicked OK and provided valid input
            if (result == true)
            {
                // Update rows and columns with custom values
                rows = customDialog.Rows;
                columns = customDialog.Columns;
                PATH_DataGrid.Children.Clear();
                buttonStates.Clear();

                RestartGame();
                InitializeFields();
            }
        }

        private void ThemeSelection_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = e.OriginalSource as MenuItem;
            if (menuItem != null)
            {
                string name = menuItem.Name;
                switch (name)
                {
                    case "Main":
                        SwitchTheme(Redish);
                        selectedBrush = Redish;
                        break;
                    case "Grey":
                        SwitchTheme(GreyColor);
                        selectedBrush = GreyColor;
                        break;
                }
            }
        }


        private void SwitchTheme(SolidColorBrush newBrush)
        {
            foreach (var button in FieldButtons)
            {
                if (button.Background != null)
                {
                    button.Background = newBrush;
                }
            }
        }

        private void Sound_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = e.OriginalSource as MenuItem;
            if (menuItem != null)
            {
                string name = menuItem.Name;
                switch (name)
                {
                    case "On":
                        SoundOn = true;
                        break;
                    case "Off":
                        SoundOn = false;
                        break;
                }
            }
        }
    }
}