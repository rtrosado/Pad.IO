#nullable disable

namespace Pad.IO.Pages
{
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Options;
    using Microsoft.JSInterop;
    using Pad.IO.Canvas;
    using Pad.IO.Handlers;
    using Pad.IO.Handlers.Events;
    using System;
    using static Pad.IO.Pages.PadIO;

    public partial class PadCanvas
    {
        [Inject] IJSRuntime JSRuntime { get; set; }
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

            framerate = 62.0f;

            _tempo = new Tempo(framerate);
            _image = new Image(25, 25);
            _mouse = new Mouse();
            _keyboard = new Keyboard();

            message = ">> ";
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {

            _tempo.start();

            if (_keyboard.wasAnyKeyPressed)
            {
                this.message += _keyboard.keyPressed;
                _keyboard.wasAnyKeyPressed = false;
                return;
            }
            if (receive == "clear")
            {
                this.message = ">> ";
                await _canvas._wrapperReference.FocusAsync();
                receive = "";
                return;
            }
            if (_mouse.wasLeftMouseClicked)
            {
                Console.WriteLine("state: ");
                this.message += $"[{_mouse.leftMouseClickPosition.x}x, {_mouse.leftMouseClickPosition.x}y]";
                _mouse.wasLeftMouseClicked = false;
                return;
            }

            await _sketch.set2DContext(_canvas._canvasReference);
            await _canvas.setFocus();

            await Task.Delay(TimeSpan.FromSeconds(1.0 / framerate));
            await InvokeAsync(() => this.StateHasChanged());

            await _sketch.Welcome(_image.reference, _tempo.getActualFramerate(), this.message);

            _tempo.waitConstrainFramerateLoop();
            _tempo.stop();
        }
    }
}
