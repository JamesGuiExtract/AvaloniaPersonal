using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace AlertManager.Models.AllDataClasses
{
    /// <summary>
    /// public IDataTemplate used to create a executable value for a Template Column in a GridTreeTable
    /// returns a textBlock
    /// </summary
    public class ContextMenuTextBlock : IDataTemplate
    {
        //field
        public TextBlock Text_Block_To_Return = new TextBlock();

        /// <summary>
        /// Constructor that initializes the context menu from parameter
        /// </summary>
        /// <param name="textBlock"></param>
        public ContextMenuTextBlock(TextBlock textBlock)
        {
            Text_Block_To_Return = textBlock;
        }

        /// <summary>
        /// Returns the Text block when the table is built
        /// *Needed to impliment IDataTemplate interface
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public IControl Build(object param)
        {
            return Text_Block_To_Return;
        }

        /// <summary>
        /// Returns true to always continue
        /// Needed to impliment IDataTemplateInterface
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool Match(object data)
        {
            return true;
        }
    }
}
