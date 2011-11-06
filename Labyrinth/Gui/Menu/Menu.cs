using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Labyrinth.Gui;

namespace Labyrinth.Gui.Menu {
    class Menu : Element {
        public List<Element> Items = new List<Element>();

        public void Add(Element Item) {
            Items.Add(Item);
        }

        protected override int GetRequiredWidth() {
            int Result = 0;
            foreach (var Item in Items) {
                Item.ResetDimensions(); // in order to calculate required width/height; TODO: check if it leads to error
                Result = Math.Max(Result, Item.Width.Value);
            }
            return Result;
        }

        protected override int GetRequiredHeight() {
            int Result = 0;
            foreach (var Item in Items) {
                Item.ResetDimensions(); // in order to calculate required width/height; TODO: check if it leads to error
                Result += Item.Height.Value;
            }
            return Result;
        }

        public override void Render() {
            int CurrentTop = Top.Value;
            foreach (var Item in Items) {
                Item.ResetDimensions();

                Item.Left = Left;
                Item.Top = CurrentTop;
                Item.Width = Width;

                Item.Render();

                CurrentTop += Item.Height.Value;
            }
        }

        public override void OnKeyPress(Key K) {
            // TODO
        }
    }
}