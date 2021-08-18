using MvvmGen;
using MvvmGen.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Extract.Utilities.SqlCompactToSqliteConverter
{
    [Inject(typeof(DatabaseConverterViewModel), PropertyAccessModifier = AccessModifier.Public)]
    [Inject(typeof(IEventAggregator))]
    [ViewModel]
    public partial class MainViewModel : IEventSubscriber<ArgumentsEvent>
    {
        /// Read the application arguments and publish an event with the supplied input/output paths, if any
        public void OnEvent(ArgumentsEvent eventData)
        {
            try
            {
                _ = eventData ?? throw new ArgumentNullException(nameof(eventData));

                IList<string> args = eventData.Args;
                if (args.Any())
                {
                    string inputPath = Path.GetFullPath(args[0].TrimPath());
                    string outputPath = args.Count > 1
                        ? Path.GetFullPath(args[1].TrimPath())
                        : Path.ChangeExtension(inputPath, ".sqlite");

                    EventAggregator.Publish(new DatabaseInputOutputEvent(inputPath, outputPath));
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI51792");
            }
        }
    }
}
