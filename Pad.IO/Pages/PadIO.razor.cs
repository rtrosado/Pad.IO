#nullable disable

namespace Pad.IO.Pages
{
    using Blazor.Extensions;
    using Blazor.Extensions.Canvas.Canvas2D;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.JSInterop;
    using System;

    public partial class PadIO
    {
        [Inject]
        IJSRuntime JSRuntime { get; set; }

        private double canvasScale = 4.0;

        private Tuple<string, string> _wrapperDims;
        private Tuple<long, long> _canvasDims;

        private string _debug;

        private Tuple<double, double> _position =
            new Tuple<double, double>(0.0, 0.0);

        private Canvas2DContext _context;
        private BECanvasComponent _canvasReference;

        private ElementReference _appleIconReference;
        private Tuple<string, string> _appleSvgDims;

        private bool appleSvgLoaded = false;

        protected override async Task OnInitializedAsync()
        {
            _canvasDims = new Tuple<long, long>((long)(210 * canvasScale), (long)(297 * canvasScale));
            _wrapperDims = new Tuple<string, string>($"{_canvasDims.Item1}px", $"{_canvasDims.Item2}px");

            _appleSvgDims = new Tuple<string, string>($"{25}px", $"{25}px");

            _debug = $"Initializing...";
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            this._context = await this._canvasReference.CreateCanvas2DAsync();
            await JSRuntime.InvokeAsync<object>("loop", DotNetObjectReference.Create(this));

            // Draw(Welcome());
        }

        [JSInvokable]
        public async ValueTask MainLoop(float timeStamp)
        {
            Draw(Welcome(timeStamp));
        }

        protected List<Func<Task>> Welcome(float timeStamp)
        {
            var actions = new List<Func<Task>>();

            if (appleSvgLoaded)
            {
                actions.Add(async () => await _context.DrawImageAsync(_appleIconReference, 200 + timeStamp / 100, 200, 25, 25));
                actions.Add(async () => await _context.StrokeAsync());
            }

            actions.Add(async () => await _context.RectAsync(0, 0, _canvasDims.Item1, _canvasDims.Item2));
            actions.Add(async () => await _context.StrokeAsync());
            actions.Add(async () => await _context.SetFontAsync("bold 18px monospace"));
            actions.Add(async () => await _context.FillTextAsync($"Hello, this is Pad.IO Editor!!! {timeStamp}", 50, 100));
            return Reset(actions);
        }

        protected async void Draw(List<Func<Task>> actions)
        {
            foreach (var action in actions) await action();
        }

        protected List<Func<Task>> Reset(List<Func<Task>> tasks)
        {
            var reset = new List<Func<Task>>();
            reset.Add(async () => await _context.ClearRectAsync(0, 0, _canvasDims.Item1, _canvasDims.Item2));
            reset.Add(async () => await _context.BeginPathAsync());
            foreach (var task in tasks) reset.Add(task);

            return reset;
        }

        protected async void CanvasLeftClick(MouseEventArgs e)
        {
            this._position = new Tuple<double, double>(e.OffsetX, e.OffsetY);
            _debug = $"Click on {this._position.Item1}x, {this._position.Item2}y";
        }

        private void AppleSvgLoaded() => appleSvgLoaded = true;
    }
}
