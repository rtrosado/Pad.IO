using Microsoft.AspNetCore.Components.Web;

namespace Pad.IO.Handlers
{
#nullable disable
    class Coordinates 
    {
        public double x { get; set; }
        public double y { get; set; }

        public Coordinates() { }
        public Coordinates(double _x, double _y) 
        { 
            x = _x;
            y = _y;
        }
    }

    internal class Mouse
    {
        public Coordinates leftMouseClickPosition { get; set; }
        public bool wasLeftMouseClicked;

        public Mouse()
        {
            leftMouseClickPosition = new Coordinates(0.0, 0.0);
            wasLeftMouseClicked = false;
        }

        public void CanvasLeftClick(MouseEventArgs e) => 
            (wasLeftMouseClicked, leftMouseClickPosition) = 
                (true, new Coordinates(e.OffsetX, e.OffsetY));
    }
}



