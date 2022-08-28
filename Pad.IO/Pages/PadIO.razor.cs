namespace Pad.IO.Pages
{
#nullable disable
    public partial class PadIO
    {
        string ParentStateHandler;

        public void AMethod(string option) =>
            ParentStateHandler = option;
    }
}
