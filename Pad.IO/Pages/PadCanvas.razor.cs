#nullable disable

namespace Pad.IO.Pages
{
    using Blazor.Extensions;
    using Blazor.Extensions.Canvas.Canvas2D;
    using Microsoft.AspNetCore.Components;
    using Microsoft.JSInterop;
    using Pad.IO.Handlers;
    using System;
    using System.Diagnostics;

    public partial class PadCanvas
    {
        [Inject] IJSRuntime JSRuntime { get; set; }
        [Parameter] public string receive { get; set; }
        [Parameter] public EventCallback<string> receiveChanged { get; set; }

        protected double canvasScale = 4.0;

        protected Tuple<string, string> _wrapperDims;
        protected Tuple<long, long> _canvasDims;

        protected Canvas2DContext _context;
        protected BECanvasComponent _canvasReference;
        protected ElementReference _wrapperReference;

        internal Tempo _tempo { get; set; }
        internal Image _image { get; set; }
        internal Mouse _mouse { get; set; }
        internal Keyboard _keyboard { get; set; }

        protected string message { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _canvasDims = new Tuple<long, long>((long)(210 * canvasScale), (long)(297 * canvasScale));
            _wrapperDims = new Tuple<string, string>($"{_canvasDims.Item1}px", $"{_canvasDims.Item2}px");

            _tempo = new Tempo(62.5f);
            _image = new Image(25, 25);
            _mouse = new Mouse();
            _keyboard = new Keyboard();

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
            _tempo.start();

            if (receive == "clear")
            {
                this.message = ">> ";
                await _wrapperReference.FocusAsync();
                receive = "";
            }

            if (_mouse.wasLeftMouseClicked)
            {
                Console.WriteLine("state: ");
                this.message += $"[{_mouse.leftMouseClickPosition.x}x, {_mouse.leftMouseClickPosition.x}y]";
                _mouse.wasLeftMouseClicked = false;
            }

            if (_keyboard.wasAnyKeyPressed)
            {
                this.message += _keyboard.keyPressed;
                _keyboard.wasAnyKeyPressed = false;
            }

            if (_image.isLoaded)
            {
                Draw(Welcome());
            }

            _tempo.waitConstrainFramerateLoop();
            _tempo.stop();
        }
        protected List<Func<Task>> Welcome()
        {
            // TODO --> SEND THIS AS BATCH

            var actions = new List<Func<Task>>();
            actions.Add(async () => await _context.DrawImageAsync(_image.reference, 200, 200, 25, 25));
            actions.Add(async () => await _context.StrokeAsync());
            actions.Add(async () => await _context.RectAsync(0, 0, _canvasDims.Item1, _canvasDims.Item2));
            actions.Add(async () => await _context.StrokeAsync());
            actions.Add(async () => await _context.SetFontAsync("bold 16px monospace"));
            actions.Add(async () => await _context.FillTextAsync($"Hello, this is PadIO Editor", 50, 100));
            actions.Add(async () => await _context.FillTextAsync(_tempo.getActualFramerate(), 50, 120));
            actions.Add(async () => await _context.FillTextAsync(this.message, 50, 140));

            return Reset(actions);
        }

        protected async void Draw(List<Func<Task>> actions)
        {
            await this._context.BeginBatchAsync();
            foreach (var action in actions) await action();

            try { await this._context.EndBatchAsync(); }
            catch (Exception) { }
        }

        protected List<Func<Task>> Reset(List<Func<Task>> tasks)
        {
            var reset = new List<Func<Task>>();
            reset.Add(async () => await _context.ClearRectAsync(0, 0, _canvasDims.Item1, _canvasDims.Item2));
            reset.Add(async () => await _context.BeginPathAsync());
            foreach (var task in tasks) reset.Add(task);

            return reset;
        }

    }
}
