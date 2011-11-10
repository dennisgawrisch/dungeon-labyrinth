using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Labyrinth.Gui.Menu;

namespace Labyrinth {
    class MenuLayer : GameWindowLayer {
        protected Menu CurrentMenu, MainMenu, QuitConfirmationMenu;

        public MenuLayer() {
            ConstructMainMenu();
            ConstructQuitConfirmationMenu();
            CurrentMenu = MainMenu;
        }

        public void ConstructMainMenu() {
            MainMenu = new Menu();

            var NewGame = new Button("New game");
            MainMenu.Add(NewGame);

            var Quit = new Button("Quit");
            MainMenu.Add(Quit);
            Quit.Enter += (Sender, E) => {
                CurrentMenu = QuitConfirmationMenu;
            };

        }

        public void ConstructQuitConfirmationMenu() {
            QuitConfirmationMenu = new Menu();

            var Question = new Gui.Text("This is no true exit. Are you fleeing?");
            QuitConfirmationMenu.Add(Question);

            var Yes = new Button("Yes");
            QuitConfirmationMenu.Add(Yes);
            Yes.Enter += (Sender, E) => {
                Window.Exit();
            };

            var No = new Button("No");
            QuitConfirmationMenu.Add(No);
            No.Enter += (Sender, E) => {
                CurrentMenu = MainMenu;
            };
        }

        public override void Render() {
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Lighting);

            var Projection = Matrix4.CreateOrthographic(-(float)Window.Width, -(float)Window.Height, -1, 1);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref Projection);
            GL.Translate(Window.Width / 2, -Window.Height / 2, 0);

            var Modelview = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref Modelview);

            // Draw alpha-blended overlay
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Color4(new Color4(0, 0, 0, 100));
            
            GL.Begin(BeginMode.Quads);
            GL.Vertex2(0, 0);
            GL.Vertex2(Window.Width, 0);
            GL.Vertex2(Window.Width, Window.Height);
            GL.Vertex2(0, Window.Height);
            GL.End();
            
            GL.Disable(EnableCap.Blend);

            // Render the menu
            CurrentMenu.ResetDimensions();

            int MenuWidth = Math.Min(CurrentMenu.Width.Value, Window.Width - 50);
            CurrentMenu.Left = (Window.Width - MenuWidth) / 2;
            CurrentMenu.Right = Window.Width - CurrentMenu.Left.Value;
            CurrentMenu.Top = 25; // TODO put logo on top
            CurrentMenu.Bottom = Window.Height - 25;

            CurrentMenu.Render();
        }

        public override void OnKeyPress(Key K) {
            CurrentMenu.OnKeyPress(K);
        }
    }
}