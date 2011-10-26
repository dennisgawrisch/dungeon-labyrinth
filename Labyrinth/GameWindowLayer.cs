using OpenTK;
using OpenTK.Input;

namespace Labyrinth {
    abstract class GameWindowLayer {
        public GameWindow Window;

        public virtual void Tick() {
        }

        public abstract void Render();

        public virtual void OnKeyPress(Key K) {
        }
    }
}