using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

namespace Labyrinth {
    class MainCompositeLayer : GameWindowLayer {
        protected MenuLayer Menu;
        protected Game Game;
        protected bool MenuIsActive;

        public MainCompositeLayer(GameWindow Window) {
            this.Window = Window;
            Menu = new MenuLayer(); Menu.Window = Window;
            Game = new Game(); Game.Window = Window;
            MenuIsActive = true;
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
                GL.PushAttrib(AttribMask.AllAttribBits);
                Game.Render();
                GL.PopAttrib();
            }
            if (MenuIsActive) {
                GL.PushAttrib(AttribMask.AllAttribBits);
                Menu.Render();
                GL.PopAttrib();
            }
        }

        public override void OnKeyPress(Key K) {
            if ((K.Equals(Key.Enter) && (Window.Keyboard[Key.AltLeft] || Window.Keyboard[Key.AltRight])) || K.Equals(Key.F11)) {
                if (WindowState.Fullscreen != Window.WindowState) {
                    Window.WindowState = WindowState.Fullscreen;
                } else {
                    Window.WindowState = WindowState.Normal;
                }
            } else if (K.Equals(Key.Escape)) {
                if (Game == null) {
                    Window.Exit();
                } else {
                    MenuIsActive = !MenuIsActive;
                }
            } else {
                if (MenuIsActive) {
                    Menu.OnKeyPress(K);
                } else if (Game != null) {
                    Game.OnKeyPress(K);
                }
            }
        }

    }
}