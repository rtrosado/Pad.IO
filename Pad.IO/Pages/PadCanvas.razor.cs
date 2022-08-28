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

    public partial class PadCanvas
    {
        [Inject]
        IJSRuntime JSRuntime { get; set; }

        [Parameter] 
        public string _stateMenu { get; set; }

        private double canvasScale = 4.0;

        private Tuple<string, string> _wrapperDims;
        private Tuple<long, long> _canvasDims;

        private Canvas2DContext _context;
        private BECanvasComponent _canvasReference;
        private ElementReference _wrapperReference;

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

        private class KeyboardState
        {
            public string keyPressed { get; set; }
            public bool wasAnyKeyPressed { get; set; }
        }
        private KeyboardState _keyboardState { get; set; }

        private string message { get; set; }


        protected override async Task OnInitializedAsync()
        {
            _canvasDims = new Tuple<long, long>((long)(210 * canvasScale), (long)(297 * canvasScale));
            _wrapperDims = new Tuple<string, string>($"{_canvasDims.Item1}px", $"{_canvasDims.Item2}px");

            _stateMenu = "";

            _renderTempo = new RenderTempo();
            _renderTempo.stopwatch = Stopwatch.StartNew();
            _renderTempo.lastRenderTime = _renderTempo.stopwatch.ElapsedTicks;
            _renderTempo.thisRenderTime = _renderTempo.stopwatch.ElapsedTicks;
            _renderTempo.lapseRenderTime = _renderTempo.thisRenderTime - _renderTempo.lastRenderTime;
            _renderTempo.framerate = 1.0f / 62.5f * Stopwatch.Frequency;
            _renderTempo.frameCount = 0;

            _appleSvgState = new AppleSvgState();
            _appleSvgState.dims = new Tuple<string, string>($"{25}px", $"{25}px");
            _appleSvgState.isLoaded = false;

            _mouseState = new MouseState();
            _mouseState.leftMouseClickPosition = new Tuple<double, double>(0, 0);
            _mouseState.wasLeftMouseClicked = false;

            _keyboardState = new KeyboardState();
            _keyboardState.keyPressed = null;
            _keyboardState.wasAnyKeyPressed = false;

            message = ">> ";
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            this._context = await this._canvasReference.CreateCanvas2DAsync();
            await _wrapperReference.FocusAsync();

            await JSRuntime.InvokeAsync<object>("loop", DotNetObjectReference.Create(this));
        }

        [JSInvokable]
        public async ValueTask MainLoop(float timeStamp)
        {
            _renderTempo.thisRenderTime = _renderTempo.stopwatch.ElapsedTicks;
            _renderTempo.frameCount++;

            if (_stateMenu != "")
            {
                Console.WriteLine("state: " + _stateMenu);

                this.message = ">> ";
                _stateMenu = "";
            }

            if (_mouseState.wasLeftMouseClicked)
            {
                Console.WriteLine("state: " + _stateMenu);
                this.message += _mouseState.leftMouseClickPosition;
                _mouseState.wasLeftMouseClicked = false;
            }

            if (_keyboardState.wasAnyKeyPressed)
            {
                this.message += _keyboardState.keyPressed;
                _keyboardState.wasAnyKeyPressed = false;
            }         

            if (_appleSvgState.isLoaded)
            {
                Draw(Welcome());
            }

            _renderTempo.lapseRenderTime = _renderTempo.stopwatch.ElapsedTicks - _renderTempo.thisRenderTime;
            while (_renderTempo.lapseRenderTime / _renderTempo.framerate < 1)
                _renderTempo.lapseRenderTime = _renderTempo.stopwatch.ElapsedTicks - _renderTempo.thisRenderTime;

            _renderTempo.lastRenderTime = _renderTempo.thisRenderTime;
        }

        protected List<Func<Task>> Welcome()
        {
            // TODO --> SEND THIS AS BATCH

            var actions = new List<Func<Task>>();
            actions.Add(async () => await _context.DrawImageAsync(_appleSvgState.reference, 200, 200, 25, 25));
            actions.Add(async () => await _context.StrokeAsync());
            actions.Add(async () => await _context.RectAsync(0, 0, _canvasDims.Item1, _canvasDims.Item2));
            actions.Add(async () => await _context.StrokeAsync());
            actions.Add(async () => await _context.SetFontAsync("bold 16px monospace"));
            actions.Add(async () => await _context.FillTextAsync($"Hello, this is PadIO Editor", 50, 100));
            actions.Add(async () => await _context.FillTextAsync(((int)(1 / (((_renderTempo.thisRenderTime - _renderTempo.lastRenderTime) / Stopwatch.Frequency)))).ToString(), 50, 120));
            actions.Add(async () => await _context.FillTextAsync(this.message, 50, 140));

            return Reset(actions);
        }

        protected async void Draw(List<Func<Task>> actions)
        {
            await this._context.BeginBatchAsync();
            foreach (var action in actions) await action();
            await this._context.EndBatchAsync();
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

        protected void CanvasKeyEvent(KeyboardEventArgs args)
        {
            _keyboardState.keyPressed = args.Key;
            _keyboardState.wasAnyKeyPressed = true;
        }

        private void AppleSvgLoaded() => _appleSvgState.isLoaded = true;
    }
}
