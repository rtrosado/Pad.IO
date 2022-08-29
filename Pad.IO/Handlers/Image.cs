namespace Pad.IO.Handlers
{
    using Microsoft.AspNetCore.Components;
    using static System.Net.Mime.MediaTypeNames;

    internal class Image
    {
        public ElementReference reference { get; set; }
        public int _width { get; set; }
        public int _height { get; set; }
        public bool isLoaded { get; set; }

        public Image(int width, int height)
        {
            _width = width;
            _height = height;
            isLoaded = false;
        }

        public string getWidth => $"{25}px";
        public string getHeight => $"{25}px";
        public void ImageLoaded() => this.isLoaded = true;
    }
}



