using System;
using System.Collections.Generic;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Labyrinth.Gui.Menu {
    class Menu : Element {
        public List<Element> Items = new List<Element>();
        protected int FocusedItemIndex = 0;
        const int ItemsSpacing = 10;

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
            int CurrentTop = Top.Value + ItemsSpacing / 2;
            foreach (var Item in Items) {
                Item.ResetDimensions();

                Item.Left = Left;
                Item.Top = CurrentTop;
                Item.Width = Width;

                GL.PushAttrib(AttribMask.AllAttribBits);

                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

                if (Items[FocusedItemIndex] == Item) {
                    GL.Color4(new Color4(255, 255, 255, 150));
                } else {
                    GL.Color4(new Color4(255, 255, 255, 70));
                }

                var Padding = ItemsSpacing / 3;

                GL.Begin(BeginMode.Quads);
                GL.Vertex2(Item.Left.Value - Padding, Item.Top.Value - Padding);
                GL.Vertex2(Item.Right.Value + Padding, Item.Top.Value - Padding);
                GL.Vertex2(Item.Right.Value + Padding, Item.Bottom.Value + Padding);
                GL.Vertex2(Item.Left.Value - Padding, Item.Bottom.Value + Padding);
                GL.End();

                GL.Disable(EnableCap.Blend);

                GL.PopAttrib();

                Item.Render();

                CurrentTop += Item.Height.Value + ItemsSpacing;
            }
        }

        public override void OnKeyPress(Key K) {
            if (K.Equals(Key.Escape)) {
                // TODO exit from menu
            } else if (K.Equals(Key.Down)) {
                ++FocusedItemIndex;
                if (FocusedItemIndex >= Items.Count) {
                    FocusedItemIndex = 0;
                }
            } else if (K.Equals(Key.Up)) {
                --FocusedItemIndex;
                if (FocusedItemIndex < 0) {
                    FocusedItemIndex = Items.Count - 1;
                }
            } else if (Items.Count > 0) {
                Items[FocusedItemIndex].OnKeyPress(K);
            }
        }
    }
}