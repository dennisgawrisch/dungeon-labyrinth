using OpenTK.Input;

namespace Labyrinth.Gui {
    public interface Control {
        bool Enabled { get; set; }
        bool Focused { get; set; }
        void OnKeyPress(Key K);
    }
}