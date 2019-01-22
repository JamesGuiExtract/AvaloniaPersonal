using Extract.Utilities.Forms;

namespace Extract.Imaging.Forms
{
    public interface IHasShortcuts
    {
        ShortcutsManager Shortcuts { get; }

        bool UseDefaultShortcuts { get; set; }
    }
}
