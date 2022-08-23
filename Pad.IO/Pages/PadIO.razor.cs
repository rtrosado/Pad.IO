#nullable disable

namespace Pad.IO.Pages
{
    using Blazor.Extensions;
    using Blazor.Extensions.Canvas.Canvas2D;
    using Microsoft.AspNetCore.Components.Web;

    public partial class PadIO
    {
        private double canvasScale = 4.0;

        private Tuple<string, string> _wrapperDims;
        private Tuple<long, long> _canvasDims;

        private string _debug;

        private Tuple<double, double> _position =
            new Tuple<double, double>(0.0, 0.0);

        private Canvas2DContext _context;
        protected BECanvasComponent _canvasReference;

        protected override async Task OnInitializedAsync()
        {
            _canvasDims = new Tuple<long, long>((long)(210 * canvasScale), (long)(297 * canvasScale));
            _wrapperDims = new Tuple<string, string>($"{_canvasDims.Item1}px", $"{_canvasDims.Item2}px");

            _debug = $"Initializing...";
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                this._context = await this._canvasReference.CreateCanvas2DAsync();

                var actions = new List<Func<Task>>();
                actions.Add(async () => await _context.ClearRectAsync(0, 0, _canvasDims.Item1, _canvasDims.Item2));
                actions.Add(async () => await _context.BeginPathAsync());
                actions.Add(async () => await _context.RectAsync(0, 0, _canvasDims.Item1, _canvasDims.Item2));
                actions.Add(async () => await _context.StrokeAsync());
                actions.Add(async () => await _context.SetFontAsync("bold 18px monospace"));
                actions.Add(async () => await _context.FillTextAsync("Hello, this is Pad.IO Editor!!!", 10, 100));

                Draw(actions);
            }
        }

        protected async void Draw(List<Func<Task>> actions)
        {
            foreach (var action in actions) await action();
        }

        protected async void CanvasLeftClick(MouseEventArgs e)
        {
            this._position = new Tuple<double, double>(e.OffsetX, e.OffsetY);
            _debug = $"Click on {this._position.Item1}x, {this._position.Item2}y";
        }
    }
}
