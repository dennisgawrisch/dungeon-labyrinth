using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Labyrinth.Menu {
    class WindowLayer : GameWindowLayer {
        protected Menu CurrentMenu, MainMenu;
        protected Button NewGame, Quit;

        public WindowLayer() {
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

            CurrentMenu.Render(new Box2(0, 0, Window.Width, Window.Height));
        }

        public override void OnKeyPress(Key K) {
            CurrentMenu.OnKeyPress(K);
        }
    }

    abstract class Element {
        public abstract void Render(Box2 Box);

        public virtual void OnKeyPress(Key K) {
        }
    }

    class Menu : Element {
        public List<Item> Items = new List<Item>();

        public void Add(Item Item) {
            Items.Add(Item);
        }

        public override void Render(Box2 Box) {
            foreach (var Item in Items) {
                Item.Render(Box);
            }
        }
    }

    abstract class Item : Element {
    }

    class Button : Item {
        public bool Enabled = true;

        public Button(string Label) {
        }

        public override void Render(Box2 Box) {
            GL.PushAttrib(AttribMask.AllAttribBits);

            GL.Color4(Color4.ForestGreen);
            GL.Begin(BeginMode.Quads);
            GL.Vertex2(Box.Left, Box.Top);
            GL.Vertex2(Box.Right, Box.Top);
            GL.Vertex2(Box.Right, Box.Bottom);
            GL.Vertex2(Box.Left, Box.Bottom);
            GL.End();

            GL.PopAttrib();

            // TODO

            // TODO respect Enabled
        }

        public event EventHandler<EventArgs> Enter;

        protected virtual void OnEnter(EventArgs E) {
            if (Enter != null) {
                Enter(this, E);
            }
        }

        public override void OnKeyPress(Key K) {
            if (Enabled && K.Equals(Key.Enter)) {
                OnEnter(EventArgs.Empty);
            }
        }
    }

    class Text : Item {
        public Text(string Label) {
        }

        public override void Render(Box2 Box) {
            // TODO
        }
    }

    class Input : Item {
        public override void Render(Box2 Box) {
            // TODO
        }
    }
}