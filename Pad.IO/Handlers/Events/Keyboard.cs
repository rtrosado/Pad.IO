using Microsoft.AspNetCore.Components.Web;

namespace Pad.IO.Handlers.Events
{
#nullable disable
    internal class Keyboard
    {
        public string keyPressed { get; set; }
        public bool wasAnyKeyPressed { get; set; }

        public Keyboard()
        {
            keyPressed = null;
            wasAnyKeyPressed = false;
        }

        public void CanvasEvent(KeyboardEventArgs args) =>
            (keyPressed, wasAnyKeyPressed) = (args.Key, true);
    }
}



