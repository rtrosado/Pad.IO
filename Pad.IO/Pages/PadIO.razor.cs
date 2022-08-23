﻿#nullable disable

namespace Pad.IO.Pages
{
    using Blazor.Extensions;
    using Blazor.Extensions.Canvas.Canvas2D;

    public partial class PadIO
    {
        private Canvas2DContext _context;
        protected BECanvasComponent _canvasReference;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            this._context = await this._canvasReference.CreateCanvas2DAsync();
            await this._context.SetFillStyleAsync("green");
            await this._context.FillRectAsync(10, 100, 100, 100);
            await this._context.SetFontAsync("14px serif");
            await this._context.StrokeTextAsync("Hello PADIO!!!", 10, 100);
        }
    }
}