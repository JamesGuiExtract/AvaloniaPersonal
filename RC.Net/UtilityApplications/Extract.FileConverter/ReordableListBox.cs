using System.Reflection;

namespace Extract.FileConverter
{
    public class ItemDragAndDropListBox : Extract.Utilities.Forms.DragAndDropListBox<IConverter>
    {
        public ItemDragAndDropListBox() : base(Assembly.GetExecutingAssembly())
        {
        }
    }
}
