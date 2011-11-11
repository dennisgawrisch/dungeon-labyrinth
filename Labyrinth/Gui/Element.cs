namespace Labyrinth.Gui {
    abstract class Element {
        private int? DefinedLeft        = null;
        private int? DefinedRight       = null;
        private int? DefinedTop         = null;
        private int? DefinedBottom      = null;
        private int? DefinedWidth       = null;
        private int? DefinedHeight      = null;

        abstract protected int GetRequiredWidth();
        abstract protected int GetRequiredHeight();

        public int? Width {
            get {
                return DefinedWidth ?? ((DefinedLeft.HasValue && DefinedRight.HasValue) ? (DefinedRight - DefinedLeft) : GetRequiredWidth());
            }

            set {
                DefinedWidth = value;
            }
        }

        public int? Height {
            get {
                return DefinedHeight ?? ((DefinedTop.HasValue && DefinedBottom.HasValue) ? (DefinedBottom - DefinedTop) : GetRequiredHeight());
            }

            set {
                DefinedHeight = value;
            }
        }

        public int? Left {
            get {
                return DefinedLeft ?? (DefinedRight.HasValue ? (DefinedRight - Width) : 0);
            }

            set {
                DefinedLeft = value;
            }
        }

        public int? Top {
            get {
                return DefinedTop ?? (DefinedBottom.HasValue ? (DefinedBottom - Height) : 0);
            }

            set {
                DefinedTop = value;
            }
        }

        public int? Right {
            get {
                return DefinedRight ?? (DefinedLeft.HasValue ? (DefinedLeft + Width) : 0);
            }

            set {
                DefinedRight = value;
            }
        }

        public int? Bottom {
            get {
                return DefinedBottom ?? (DefinedTop.HasValue ? (DefinedTop + Height) : 0);
            }

            set {
                DefinedBottom = value;
            }
        }

        public void ResetDimensions() {
            Left    = null;
            Right   = null;
            Top     = null;
            Bottom  = null;
            Width   = null;
            Height  = null;
        }

        abstract public void Render();
    }
}