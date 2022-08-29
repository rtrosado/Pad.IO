namespace Pad.IO.Canvas
{
    using Blazor.Extensions;
    using Microsoft.AspNetCore.Components;

    public class Dimensions
    {
        public long width { get; set; }
        public long height { get; set; }
        public Dimensions(long _width, long _height) => 
            (width, height) = (_width, _height);

        public string getWidthToString() => width.ToString();
        public string getHeightToString() => height.ToString();
    }

    internal class Canvas
    {
        public double _scale = 4.0;

        public Dimensions _wrapperDims;
        public Dimensions _canvasDims;

        public BECanvasComponent? _canvasReference;
        public ElementReference _wrapperReference;

        public Canvas(long width, long height, double scale)
        {
            _wrapperDims = new Dimensions((long)(width*scale), (long)(height* scale));
            _canvasDims = new Dimensions((long)(width * scale), (long)(height * scale));
            _scale = scale;
        }

        public Tuple<string, string> wrapperDimsToString() => 
            new Tuple<string, string>(_wrapperDims.getWidthToString(), _wrapperDims.getHeightToString());

        public async Task setFocus() =>
            await _wrapperReference.FocusAsync();
            
    }
}
