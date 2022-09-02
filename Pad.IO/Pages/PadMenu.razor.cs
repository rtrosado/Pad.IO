#nullable disable

namespace Pad.IO.Pages
{
    using Microsoft.AspNetCore.Components;
    using Pad.IO.Menu;
    using System.Collections.Generic;

    public partial class PadMenu
    {
        [Parameter] public string send { get; set; }
        [Parameter] public EventCallback<string> sendChanged { get; set; }

        internal List<string> _collapses { get; set; }
        public Main? _menu { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _collapses = new List<string>();
            _menu = new Main();
            
            _menu.text = new List<Item>()
            {
                new Item(){ svg = "", name = "create", on = false, shortcut = ""},
                new Item(){ svg = "", name = "edit", on = false, shortcut = ""},
                new Item(){ svg = "", name = "title", on = false, shortcut = ""},
                new Item(){ svg = "", name = "body", on = false, shortcut = ""},
                new Item(){ svg = "", name = "clear", on = false, shortcut = ""},
            };

            _menu.draw = new List<Item>()
                {
                new Item(){ svg = "", name = "create", on = false, shortcut = ""},
                new Item(){ svg = "", name = "edit", on = false, shortcut = ""},
                new Item(){ svg = "", name = "decision", on = false, shortcut = ""},
                new Item(){ svg = "", name = "three", on = false, shortcut = ""},
                new Item(){ svg = "", name = "clear", on = false, shortcut = ""},
                };

            _menu.navigate = new List<Item>() {
                new Item() { svg = "", name = "next", on = false, shortcut = ""},
                new Item() { svg = "", name = "previous", on = false, shortcut = "" },
                new Item() { svg = "", name = "last", on = false, shortcut = "" },
                new Item() { svg = "", name = "first", on = false, shortcut = "" },
                };
        }

        public void doCollapse(int index) =>
            _collapses[index] = _collapses[index] == "none" ? "block" : "none";

        public object GetPropValue(object src, string propName) => 
            src.GetType().GetProperty(propName).GetValue(src, null);

        async void doClear() =>
            await sendChanged.InvokeAsync("clear");
    }
}
