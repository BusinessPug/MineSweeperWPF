using System.Windows.Controls;

namespace MineSweeper
{
    public class MineSweeperButton : Button
    {
        public States ButtonState { get; set; } // Property to represent the state of the button

        public MineSweeperButton()
        {
            ButtonState = States.unrevealed; // Initialize button state as unrevealed
        }
    }
}
