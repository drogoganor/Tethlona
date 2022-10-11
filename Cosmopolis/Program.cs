using Cosmopolis;
using Cosmopolis.SampleBase;

var window = new VeldridStartupWindow("Cosmopolis");
var mainMenu = new MainMenu(window);
var game = new Game(window);

mainMenu.Show();
mainMenu.OnNewGame += () =>
{
    mainMenu.Hide();
    game.Show();
    game.OnEndGame += () =>
    {
        game.Hide();
        mainMenu.Show();
    };
};

window.Run();
