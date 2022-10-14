using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using LabDEOrderMappingInvestigator.ViewModels;

namespace LabDEOrderMappingInvestigator
{
    public class ViewLocator : IDataTemplate
    {
        [RequiresUnreferencedCode("This method will not work correctly if Views/ViewModels are trimmed when this is published")]
        public IControl Build(object param)
        {
            string name = "null";

            if (param is not null)
            {
                name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
                var type = Type.GetType(name);

                if (type != null)
                {
                    return (Control)Activator.CreateInstance(type)!;
                }
            }
            
            return new TextBlock { Text = "Not Found: " + name };
        }

        public bool Match(object data)
        {
            return data is ViewModelBase;
        }
    }
}