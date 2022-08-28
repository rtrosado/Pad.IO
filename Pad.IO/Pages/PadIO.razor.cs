namespace Pad.IO.Pages
{
#nullable disable
    public partial class PadIO
    {
        public Content Message { get; set; } = new PadIO.Content();
        public class Content 
        {
            public string Command { get; set; }
        }
    }
}
