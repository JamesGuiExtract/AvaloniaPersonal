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
                    DatabaseInputOutputEvent paths = new() { InputDatabasePath = args[0].TrimPath() };
                    if (args.Count > 1)
                    {
                        paths.OutputDatabasePath = args[1].TrimPath();
                    }
                    else
                    {
                        paths.OutputDatabasePath = Path.ChangeExtension(paths.InputDatabasePath, ".sqlite");
                    }

                    EventAggregator.Publish(paths);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI51792");
            }
        }
    }
}
