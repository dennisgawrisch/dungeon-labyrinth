using OpenTK;

namespace Labyrinth {
    abstract class GameWindowLayer {
        abstract public void OnUpdateFrame(GameWindow window);
        abstract public void OnRenderFrame(GameWindow window);
    }
}