using System.Windows;

namespace MineSweeper
{
    public partial class CustomInputDialog : Window
    {
        public int Rows { get; private set; }
        public int Columns { get; private set; }

        public CustomInputDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtRows.Text, out int rows) && int.TryParse(txtColumns.Text, out int columns) && rows > 4 && columns > 4 && rows == columns)
            {
                Rows = rows;
                Columns = columns;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Invalid input. Please enter valid integer values for rows and columns, that are 5 or larger and are the same");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
