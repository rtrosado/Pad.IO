using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Pad.IO.Pages
{
    public partial class PadMenu
    {
        [Parameter] 
        public EventCallback<string> AMenuEvent { get; set; }

        public void PadStateMenu(MouseEventArgs e)
        {
            AMenuEvent.InvokeAsync("clear");
        }
    }
}
