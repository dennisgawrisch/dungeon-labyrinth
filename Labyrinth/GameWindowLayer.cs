using OpenTK;
using OpenTK.Input;

namespace Labyrinth {
    abstract class GameWindowLayer {
        public GameWindow Window { get; set; }

        public virtual void Tick() {
        }

        public virtual void Render() {
        }

        public virtual void OnKeyPress(Key Key) {
        }
    }
}