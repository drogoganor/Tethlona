using Cosmopolis.SampleBase;
using Cosmopolis.UI;

namespace Cosmopolis
{
    public class Startup
    {
        private readonly MainMenu mainMenu;
        private readonly Game game;
        private readonly IApplicationWindow applicationWindow;

        public Startup(MainMenu mainMenu, Game game, IApplicationWindow applicationWindow)
        {
            this.mainMenu = mainMenu;
            this.game = game;
            this.applicationWindow = applicationWindow;
        }

        public void Run()
        {
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

            applicationWindow.Run();
        }
    }
}
