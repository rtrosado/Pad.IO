#nullable disable

namespace Pad.IO.Pages
{
    using Microsoft.AspNetCore.Components;
    using Pad.IO.Canvas;
    using Pad.IO.Handlers;
    using Pad.IO.Handlers.Events;
    using System;
    using System.Diagnostics;

    public partial class PadCanvas
    {
        [Parameter] public string receive { get; set; }
        [Parameter] public EventCallback<string> receiveChanged { get; set; }

        protected string message { get; set; }
        protected double framerate { get; set; }
        protected double measuredFramerate { get; set; } = 0;

        internal Canvas _canvas { get; set; }
        internal Sketch _sketch { get; set; }

        internal Image _image { get; set; }
        internal Mouse _mouse { get; set; }
        internal Keyboard _keyboard { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _canvas = new Canvas(210, 297, 4.0);
            _sketch = new Sketch(_canvas._canvasDims);
            _sketch.SetMargins(50, 50);

            framerate = 60.0;

            _image = new Image(25, 25);
            _mouse = new Mouse();
            _keyboard = new Keyboard();

            message = ">> ";
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!(await eventListener())) return;

            await Delay.Watch(1000 / framerate);
            //await Task.Delay(TimeSpan.FromSeconds(1.0 / (framerate * 2)));

            try { await _sketch.Set2DContext(_canvas._canvasReference); }
            catch (Exception) { return; };
;
            await _canvas.setFocus();

            _sketch.Reset();
            _sketch.Clear();
            _sketch.DrawPage(6);
            _sketch.DrawMargins(1);

            await _sketch.DrawText(measuredFramerate.ToString(), 50, 200);
            double textWidth = await _sketch.DrawText(message, 50, 70);

            _sketch._cursor.offset.left = 50 + (int)textWidth;
            _sketch._cursor.offset.top = 70;
            _sketch.Cursor();

            await _sketch.Draw();

            await InvokeAsync(() => this.StateHasChanged());
        }

        protected async Task<bool> eventListener()
        {
            bool reRender = true;

            if (_keyboard.wasAnyKeyPressed)
            {
                string formatKey = _keyboard.keyPressed.Length == 1 ? _keyboard.keyPressed : "";

                this.message += formatKey;
                _keyboard.wasAnyKeyPressed = false;
                reRender = false;
            }
            if (_mouse.wasLeftMouseClicked)
            {
                this.message += $"[{_mouse.leftMouseClickPosition.x}x, {_mouse.leftMouseClickPosition.x}y]";
                _mouse.wasLeftMouseClicked = false;
                reRender = false;
            }
            if (receive == "clear")
            {
                this.message = ">> ";
                await _canvas._wrapperReference.FocusAsync();
                receive = "";
                reRender = false;
            }

            return reRender;
        }
    }

    public static class Delay
    {
        public static async Task Watch(double ms)
            => await RunAsync<double>(() => WatchWait(ms));

        public static double WatchWait(double ms)
        {
            var timer = new Stopwatch();
            timer.Start();
            while (timer.Elapsed.TotalMilliseconds < ms) { };
            return 0;
        }

        public static Task<T> RunAsync<T>(Func<T> function)
        {
            if (function == null) throw new ArgumentNullException("function");
            var tcs = new TaskCompletionSource<T>();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    T result = function();
                    tcs.SetResult(result);
                }
                catch (Exception exc) { tcs.SetException(exc); }
            });
            return tcs.Task;
        }
    }
}
