namespace Pad.IO.Canvas
{
    using Blazor.Extensions;
    using Blazor.Extensions.Canvas;
#nullable disable

    using Blazor.Extensions.Canvas.Canvas2D;
    using Microsoft.AspNetCore.Components;
    internal class Sketch
    {
        public Canvas2DContext _context;
        public Dimensions _dims;
        
        public Sketch(Dimensions dims) => _dims = dims;
        public async Task setContext(BECanvasComponent reference) =>
            this._context = await reference.CreateCanvas2DAsync();

        public async Task<bool> Run(List<Func<Task>> actions)
        {
            await this._context.BeginBatchAsync();
            foreach (var action in actions) await action();

            try { await this._context.EndBatchAsync(); }
            catch (Exception) { return false; }

            return true;
        }
        public async Task<bool> Welcome(ElementReference image, string firstMessage, string secondMessage)
        {
            var actions = new List<Func<Task>>();
            actions.Add(async () => await _context.DrawImageAsync(image, 200, 200, 25, 25));
            actions.Add(async () => await _context.StrokeAsync());
            actions.Add(async () => await _context.RectAsync(0, 0, _dims.width, _dims.height));
            actions.Add(async () => await _context.StrokeAsync());
            actions.Add(async () => await _context.SetFontAsync("bold 16px monospace"));
            actions.Add(async () => await _context.FillTextAsync($"Hello, this is PadIO Editor", 50, 100));
            actions.Add(async () => await _context.FillTextAsync(firstMessage, 50, 120));
            actions.Add(async () => await _context.FillTextAsync(secondMessage, 50, 140));

            return await Run(Reset(actions));
        }

        public List<Func<Task>> Reset(List<Func<Task>> tasks)
        {
            var reset = new List<Func<Task>>();
            reset.Add(async () => await _context.ClearRectAsync(0, 0, _dims.width, _dims.height));
            reset.Add(async () => await _context.BeginPathAsync());
            foreach (var task in tasks) reset.Add(task);

            return reset;
        }
    }


}
