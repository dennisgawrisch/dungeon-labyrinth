using OpenTK;
using OpenTK.Input;

namespace Labyrinth {
    class MainCompositeLayer : GameWindowLayer {
        protected Menu Menu;
        protected Game Game;
        protected bool MenuIsActive;

        public MainCompositeLayer(GameWindow Window) {
            this.Window = Window;
            Menu = new Menu(); Menu.Window = Window;
            Game = new Game(); Game.Window = Window;
            MenuIsActive = false;
        }

        public override void Tick() {
            if (MenuIsActive) {
                Menu.Tick();
            } else if (Game != null) {
                Game.Tick();
            }
        }

        public override void Render() {
            if (Game != null) {
                Game.Render();
            }
            if (MenuIsActive) {
                Menu.Render();
            }
        }

        public override void OnKeyPress(Key Key) {
            if (Key.Equals(OpenTK.Input.Key.Escape)) {
                if (Game == null) {
                    Window.Exit();
                } else {
                    MenuIsActive = !MenuIsActive;
                }
            } else {
                if (MenuIsActive) {
                    Menu.OnKeyPress(Key);
                } else if (Game != null) {
                    Game.OnKeyPress(Key);
                }
            }
        }

    }
}