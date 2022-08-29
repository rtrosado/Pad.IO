namespace Pad.IO.Pages
{
    using Microsoft.AspNetCore.Components;

    public partial class PadMenu
    {
        [Parameter] public string send { get; set; }
        [Parameter] public EventCallback<string> sendChanged { get; set; }

        async void doClear() =>
            await sendChanged.InvokeAsync("clear");
    }
}
