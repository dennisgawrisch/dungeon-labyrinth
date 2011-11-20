using System;
using System.Collections.Generic;
using OpenTK.Input;

namespace Labyrinth.Gui.Menu {
    class Menu : Element {
        public List<Element> Items = new List<Element>();
        protected int? FocusedItemIndex;
        protected Control FocusedItem {
            get {
                return FocusedItemIndex.HasValue ? (Items[FocusedItemIndex.Value] as Control) : null;
            }
        }
        const int ItemsSpacing = 6;

        public void Add(Element Item) {
            if (!FocusedItemIndex.HasValue && (Item is Control) && (Item as Control).Enabled) {
                FocusedItemIndex = Items.Count;
                (Item as Control).Focused = true;
            }
            Items.Add(Item);
        }

        protected override int GetRequiredWidth() {
            int Result = 0;
            foreach (var Item in Items) {
                Item.ResetDimensions();
                Result = Math.Max(Result, Item.Width.Value);
            }
            return Result;
        }

        protected override int GetRequiredHeight() {
            int Result = 0;
            foreach (var Item in Items) {
                Item.ResetDimensions();
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

                Item.Render();

                CurrentTop += Item.Height.Value + ItemsSpacing;
            }
        }

        public event EventHandler<EventArgs> Exit;

        protected virtual void OnExit(EventArgs E) {
            if (Exit != null) {
                Exit(this, E);
            }
        }

        public void OnKeyPress(Key K) {
            if (K.Equals(Key.Escape)) {
                OnExit(EventArgs.Empty);
            } else if ((K.Equals(Key.Down) || K.Equals(Key.Up)) && (Items.Count > 0)) {
                if (FocusedItemIndex.HasValue) {
                    FocusedItem.Focused = false;
                } else {
                    FocusedItemIndex = 0;
                }

                do {
                    FocusedItemIndex = FocusedItemIndex + (K.Equals(Key.Down) ? +1 : -1);
                    if (FocusedItemIndex >= Items.Count) {
                        FocusedItemIndex = 0;
                    }
                    if (FocusedItemIndex < 0) {
                        FocusedItemIndex = Items.Count - 1;
                    }
                } while (!(Items[FocusedItemIndex.Value] is Control) || !FocusedItem.Enabled);

                FocusedItem.Focused = true;
            } else if (FocusedItemIndex.HasValue && FocusedItem.Enabled) {
                FocusedItem.OnKeyPress(K);
            }
        }
    }
}