#nullable disable

namespace Pad.IO.Pages
{
    using Microsoft.AspNetCore.Components;
    using Microsoft.JSInterop;
    using Pad.IO.Canvas;
    using Pad.IO.Handlers;
    using Pad.IO.Handlers.Events;
    using System;

    public partial class PadCanvas
    {
        [Inject] IJSRuntime JSRuntime { get; set; }
        [Parameter] public string receive { get; set; }
        [Parameter] public EventCallback<string> receiveChanged { get; set; }

        protected string message { get; set; }

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

            await _sketch.setContext(_canvas._canvasReference);
            await _canvas.setFocus();

            await JSRuntime.InvokeAsync<object>("loop", DotNetObjectReference.Create(this));
        }

        [JSInvokable]
        public async ValueTask MainLoop(float timeStamp)
        {
            _tempo.start();

            if (receive == "clear")
            {
                this.message = ">> ";
                await _canvas._wrapperReference.FocusAsync();
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
                await _sketch.Welcome(_image.reference, _tempo.getActualFramerate(), this.message);
            }

            _tempo.waitConstrainFramerateLoop();
            _tempo.stop();
        }
    }
}
