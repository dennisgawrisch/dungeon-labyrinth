using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Labyrinth.Gui.Menu;

namespace Labyrinth {
    class MenuLayer : GameWindowLayer {
        private MainCompositeLayer MainLayer;
        private Menu CurrentMenuValue;
        public Menu CurrentMenu {
            get {
                return CurrentMenuValue;
            }

            set {
                CurrentMenuValue = value;
                CurrentMenuValue.ResetState();
            }
        }
        public Menu MainMenu;
        private Menu NewGameMenu, HelpMenu, QuitConfirmationMenu;

        public MenuLayer(MainCompositeLayer Composite)
            : base(Composite.Window) {
            MainLayer = Composite;
            ConstructMainMenu();
            CurrentMenu = MainMenu;
        }

        public void ConstructMainMenu() {
            MainMenu = new Menu();
            MainMenu.Exit += (Sender, E) => {
                CurrentMenu = QuitConfirmationMenu;
            };

            var NewGame = new Button("New game");
            MainMenu.Add(NewGame);
            NewGame.Enter += (Sender, E) => {
                CurrentMenu = NewGameMenu;
            };

            var Help = new Button("Help");
            MainMenu.Add(Help);
            Help.Enter += (Sender, E) => {
                CurrentMenu = HelpMenu;
            };

            var Quit = new Button("Quit");
            MainMenu.Add(Quit);
            Quit.Enter += (Sender, E) => {
                CurrentMenu = QuitConfirmationMenu;
            };

            ConstructNewGameMenu();
            ConstructHelpMenu();
            ConstructQuitConfirmationMenu();
        }

        public void ConstructNewGameMenu() {
            NewGameMenu = new Menu();
            NewGameMenu.Exit += (Sender, E) => {
                CurrentMenu = MainMenu;
            };

            var Easy = new Button("Easy");
            NewGameMenu.Add(Easy);
            Easy.Enter += (Sender, E) => {
                MainLayer.Game = new Game(Window, Game.DifficultyLevel.Easy);
            };

            var Normal = new Button("Normal");
            NewGameMenu.Add(Normal);
            Normal.Enter += (Sender, E) => {
                MainLayer.Game = new Game(Window, Game.DifficultyLevel.Normal);
            };

            var Hard = new Button("Hard");
            NewGameMenu.Add(Hard);
            Hard.Enter += (Sender, E) => {
                MainLayer.Game = new Game(Window, Game.DifficultyLevel.Hard);
            };
        }

        public void ConstructHelpMenu() {
            HelpMenu = new Menu();
            HelpMenu.Exit += (Sender, E) => {
                CurrentMenu = MainMenu;
            };

            HelpMenu.Add(new Text("Use W A S D or arrow keys to move."));
            HelpMenu.Add(new Text("Press C to toggle third person view."));
            HelpMenu.Add(new Text("Press F11 or Alt + Enter to toggle fullscreen mode."));
            HelpMenu.Add(new Text("You have to find exit from labyrinth."));
            HelpMenu.Add(new Text("Collect all keys to unlock the exit."));
            HelpMenu.Add(new Text("Make it before your torch goes out."));
            HelpMenu.Add(new Text("Press F to leave marks (amount is limited)."));
            HelpMenu.Add(new Text("Collect treasures to gain extra score."));
            HelpMenu.Add(new Text("Avoid ghosts."));

            var Back = new Button("Back");
            HelpMenu.Add(Back);
            Back.Enter += (Sender, E) => {
                CurrentMenu = MainMenu;
            };
        }

        public void ConstructQuitConfirmationMenu() {
            QuitConfirmationMenu = new Menu();
            QuitConfirmationMenu.Exit += (Sender, E) => {
                CurrentMenu = MainMenu;
            };

            QuitConfirmationMenu.Add(new Text("This is no true exit. Are you fleeing?"));

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