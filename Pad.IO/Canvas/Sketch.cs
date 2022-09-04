namespace Pad.IO.Canvas
{
#nullable disable
    using Blazor.Extensions;

    using Blazor.Extensions.Canvas.Canvas2D;
    using Microsoft.AspNetCore.Components;
    using System;

    public class Margins
    {
        public int left;
        public int top;
    }

    public class Cursor
    {
        public class Offset
        {
            public int left;
            public int top;
        };

        public string color;
        public int beat;
        public int rate;
        public int lineWidth;
        public int fontTopMargin;
        public int cursorHeight;
        public Offset offset;
    }

    internal class Sketch
    {
        public Canvas2DContext _context;
        public Dimensions _dims;
        public List<Func<Task>> _actions;
        public Margins _margins;
        public Cursor _cursor;

        public int frame;

        public Sketch(Dimensions dims) 
        {
            _dims = dims;
            _actions = new List<Func<Task>>();

            _margins = new Margins()
            {
                left = 0, 
                top = 0 
            };

            _cursor = new Cursor() 
            { 
                color = "#323232",
                beat = 60,
                rate = 30,
                lineWidth = 2,
                fontTopMargin = 4,
                cursorHeight = 10,
                offset = new Cursor.Offset() { left = 50, top = 50 },
            };
            frame = 0;
        }

        public async Task Set2DContext(BECanvasComponent reference) =>
            this._context = await reference.CreateCanvas2DAsync();

        public void SetMargins(int left, int right) => 
            (_margins.left, _margins.top) = (left, right);

        public void Reset() => _actions = new List<Func<Task>>();

        public void DrawPage(int lineWidth)
        {
            _actions.Add(async () => await _context.SetLineWidthAsync(lineWidth));
            _actions.Add(async () => await _context.RectAsync(0, 0, _dims.width, _dims.height));
            _actions.Add(async () => await _context.StrokeAsync());
        }

        public void DrawMargins(int lineWidth)
        {
            _actions.Add(async () => await _context.SetLineWidthAsync(lineWidth));
            _actions.Add(async () => 
            await _context.RectAsync(
                _margins.left,
                _margins.top,
                _dims.width - (_margins.left * 2),
                _dims.height - (_margins.top * 2))
            );
            _actions.Add(async () => await _context.StrokeAsync());
        }

        public void DrawImage(ElementReference image, int top, int left, int width, int height)
        {
            _actions.Add(async () => await _context.DrawImageAsync(image, top, left, width, height));
            _actions.Add(async () => await _context.StrokeAsync());
        }

        public async Task<double> DrawText(string message, int left, int top)
        {
            _actions.Add(async () => await _context.SetFontAsync("bold 16px monospace"));
            _actions.Add(async () => await _context.FillTextAsync(message, left, top));

            var metrics = await _context.MeasureTextAsync(message);
            return metrics.Width;
        }

        public void Cursor()
        {
            if (_cursor.rate >= _cursor.beat) return;
            if (frame % _cursor.beat >= _cursor.rate)
            {
                _actions.Add(async () => await _context.SetLineWidthAsync(_cursor.lineWidth));
                _actions.Add(async () => await _context.BeginPathAsync());
                _actions.Add(async () => await _context.SetStrokeStyleAsync(_cursor.color));
                _actions.Add(async () => await _context.MoveToAsync(
                    _cursor.offset.left,
                    _cursor.offset.top - (_cursor.fontTopMargin + _cursor.cursorHeight))
                );
                _actions.Add(async () => await _context.LineToAsync(
                    _cursor.offset.left,
                    _cursor.offset.top + _cursor.fontTopMargin)
                );
                _actions.Add(async () => await _context.StrokeAsync());
            }
        }

        public async Task<bool> Draw()
        {
            await this._context.BeginBatchAsync();
            foreach (var action in _actions) await action();

            try { await this._context.EndBatchAsync(); }
            catch (Exception) { return false; }

            frame++;
            return true;
        }

        public void Clear()
        {
            var clear = new List<Func<Task>>();
            clear.Add(async () => await _context.ClearRectAsync(0, 0, _dims.width, _dims.height));
            clear.Add(async () => await _context.BeginPathAsync());
            foreach (var task in _actions) clear.Add(task);
            _actions = clear;
        }
    }
}
