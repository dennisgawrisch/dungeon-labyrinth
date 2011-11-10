using System;
using OpenTK.Input;

namespace Labyrinth.Gui.Menu {
    class Button : Text {
        public bool Enabled = true; // TODO respect while rendering

        public Button(string Label) 
            : base(Label) {
        }

        public event EventHandler<EventArgs> Enter;

        protected virtual void OnEnter(EventArgs E) {
            if (Enter != null) {
                Enter(this, E);
            }
        }

        public override void OnKeyPress(Key K) {
            if (Enabled && (K.Equals(Key.Enter) || K.Equals(Key.KeypadEnter))) {
                OnEnter(EventArgs.Empty);
            }
        }
    }
}