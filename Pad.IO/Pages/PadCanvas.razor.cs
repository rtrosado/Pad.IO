#nullable disable

namespace Pad.IO.Pages
{
    using Microsoft.AspNetCore.Components;
    using Pad.IO.Canvas;
    using Pad.IO.Handlers;
    using Pad.IO.Handlers.Events;
    using System;

    public partial class PadCanvas
    {
        [Parameter] public string receive { get; set; }
        [Parameter] public EventCallback<string> receiveChanged { get; set; }

        protected string message { get; set; }
        protected float framerate { get; set; }

        internal Canvas _canvas { get; set; }
        internal Sketch _sketch { get; set; }

        internal Tempo _tempo { get; set; }
        internal Image _image { get; set; }
        internal Mouse _mouse { get; set; }
        internal Keyboard _keyboard { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _canvas = new Canvas(210, 297, 4.0);
            _sketch = new Sketch(_canvas._canvasDims);
            _sketch.SetMargins(50, 50);

            framerate = 61.0f;

            _tempo = new Tempo(framerate);
            _image = new Image(25, 25);
            _mouse = new Mouse();
            _keyboard = new Keyboard();

            message = ">> ";
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!(await eventListener())) return;

            await Task.Delay(TimeSpan.FromSeconds(1.0 / (framerate - 20.0)));

            try { await _sketch.Set2DContext(_canvas._canvasReference); }
            catch (Exception) { return; };

            _tempo.start();
            await _canvas.setFocus();

            _sketch.Reset();
            _sketch.Clear();
            _sketch.DrawPage(6);
            _sketch.DrawMargins(1);

            double textWidth = await _sketch.DrawText(message, 50, 70);

            _sketch._cursor.offset.left = 50 + (int)textWidth;
            _sketch._cursor.offset.top = 70;
            _sketch.Cursor();

            await _sketch.Draw();

            _tempo.waitConstrainFramerateLoop();
            _tempo.stop();

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
}
