using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Labyrinth.Gui.Menu;

namespace Labyrinth {
    class MenuLayer : GameWindowLayer {
        protected Menu CurrentMenu, MainMenu;
        protected Button NewGame, Quit;

        public MenuLayer() {
            MainMenu = new Menu();

            NewGame = new Button("New game");
            MainMenu.Add(NewGame);
            NewGame.Enter += OnNewGameEnter;

            Quit = new Button("Quit");
            MainMenu.Add(Quit);
            Quit.Enter += OnQuitEnter;

            CurrentMenu = MainMenu;
        }

        protected void OnNewGameEnter(object Sender, EventArgs E) {
            // TODO
        }

        protected void OnQuitEnter(object Sender, EventArgs E) {
            Window.Exit(); // TODO confirmation dialog
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

            // debug
            GL.Color4(Color4.Red);
            GL.Begin(BeginMode.Quads);
            GL.Vertex2(0, 0);
            GL.Vertex2(10, 0);
            GL.Vertex2(10, 10);
            GL.Vertex2(0, 10);
            GL.End();

            GL.Color4(Color4.Green);
            GL.Begin(BeginMode.Quads);
            GL.Vertex2(Window.Width - 10, 0);
            GL.Vertex2(Window.Width, 0);
            GL.Vertex2(Window.Width, 10);
            GL.Vertex2(Window.Width - 10, 10);
            GL.End();

            GL.Color4(Color4.Blue);
            GL.Begin(BeginMode.Quads);
            GL.Vertex2(Window.Width - 10, Window.Height - 10);
            GL.Vertex2(Window.Width, Window.Height - 10);
            GL.Vertex2(Window.Width, Window.Height);
            GL.Vertex2(Window.Width - 10, Window.Height);
            GL.End();
            // end debug

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