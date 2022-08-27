#nullable disable

namespace Pad.IO.Pages
{
    using Blazor.Extensions;
    using Blazor.Extensions.Canvas.Canvas2D;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.JSInterop;
    using System;
    using System.Diagnostics;

    public partial class PadIO
    {
        [Inject]
        IJSRuntime JSRuntime { get; set; }

        private double canvasScale = 4.0;

        private Tuple<string, string> _wrapperDims;
        private Tuple<long, long> _canvasDims;

        private Canvas2DContext _context;
        private BECanvasComponent _canvasReference;

        private class RenderTempo
        {
            public float thisRenderTime { get; set; }
            public float lastRenderTime { get; set; }
            public float lapseRenderTime { get; set; }
            public Stopwatch stopwatch { get; set; }
            public float framerate { get; set; }
            public int frameCount { get; set; }
        }
        private RenderTempo _renderTempo { get; set; }

        private class AppleSvgState
        {
            public ElementReference reference { get; set; }
            public Tuple<string, string> dims { get; set; }
            public bool isLoaded { get; set; }
        }
        private AppleSvgState _appleSvgState { get; set; }

        private class MouseState
        {
            public Tuple<double, double> leftMouseClickPosition { get; set; }
            public bool wasLeftMouseClicked = false;
        }
        private MouseState _mouseState { get; set; }


        protected override async Task OnInitializedAsync()
        {
            _canvasDims = new Tuple<long, long>((long)(210 * canvasScale), (long)(297 * canvasScale));
            _wrapperDims = new Tuple<string, string>($"{_canvasDims.Item1}px", $"{_canvasDims.Item2}px");

            _renderTempo = new RenderTempo();
            _renderTempo.stopwatch = Stopwatch.StartNew();
            _renderTempo.lastRenderTime = _renderTempo.stopwatch.ElapsedTicks;
            _renderTempo.thisRenderTime = _renderTempo.stopwatch.ElapsedTicks;
            _renderTempo.lapseRenderTime = _renderTempo.thisRenderTime - _renderTempo.lastRenderTime;
            _renderTempo.framerate = 1.0f / 60.0f * Stopwatch.Frequency;
            _renderTempo.frameCount = 0;

            _appleSvgState = new AppleSvgState();
            _appleSvgState.dims = new Tuple<string, string>($"{25}px", $"{25}px");
            _appleSvgState.isLoaded = false;

            _mouseState = new MouseState();
            _mouseState.leftMouseClickPosition = new Tuple<double, double>(0, 0);
            _mouseState.wasLeftMouseClicked = false;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            this._context = await this._canvasReference.CreateCanvas2DAsync();

            await JSRuntime.InvokeAsync<object>("loop", DotNetObjectReference.Create(this));
        }

        [JSInvokable]
        public async ValueTask MainLoop(float timeStamp)
        {
            _renderTempo.thisRenderTime = _renderTempo.stopwatch.ElapsedTicks;
            _renderTempo.lapseRenderTime = _renderTempo.thisRenderTime - _renderTempo.lastRenderTime;
            _renderTempo.frameCount++;

            while (_renderTempo.lapseRenderTime / _renderTempo.framerate <= 1)
                _renderTempo.lapseRenderTime = _renderTempo.stopwatch.ElapsedTicks - _renderTempo.lastRenderTime;

            if (_appleSvgState.isLoaded)
            {
                Draw(Welcome($"{_renderTempo.frameCount}", _renderTempo.frameCount));
            }

            _renderTempo.lastRenderTime = _renderTempo.thisRenderTime;
        }

        protected List<Func<Task>> Welcome(string message, int frameCount)
        {
            // TODO --> SEND THIS AS BATCH

            var actions = new List<Func<Task>>();
            actions.Add(async () => await _context.DrawImageAsync(_appleSvgState.reference, 200, 200, 25, 25));
            actions.Add(async () => await _context.StrokeAsync());
            actions.Add(async () => await _context.RectAsync(0, 0, _canvasDims.Item1, _canvasDims.Item2));
            actions.Add(async () => await _context.StrokeAsync());
            actions.Add(async () => await _context.SetFontAsync("bold 18px monospace"));
            actions.Add(async () => await _context.FillTextAsync($"{message}", 50, 100 + frameCount * 10));

            return actions;
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

        protected void CanvasLeftClick(MouseEventArgs e)
        {
            _mouseState.wasLeftMouseClicked = true;
            _mouseState.leftMouseClickPosition = new Tuple<double, double> (e.OffsetX, e.OffsetY);
        } 

        private void AppleSvgLoaded() => _appleSvgState.isLoaded = true;
    }
}
