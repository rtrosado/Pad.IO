using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Pad.IO.Pages
{
    public partial class PadMenu
    {
        [Parameter] public string send { get; set; }
        [Parameter] public EventCallback<string> sendChanged { get; set; }

        async void doClear() =>
            await sendChanged.InvokeAsync("clear");
    }
}
