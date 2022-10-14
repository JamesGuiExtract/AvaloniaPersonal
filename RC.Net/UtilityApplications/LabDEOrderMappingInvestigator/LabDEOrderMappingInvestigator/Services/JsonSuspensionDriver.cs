using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;

namespace LabDEOrderMappingInvestigator.Services
{
    public class JsonSuspensionDriver : ISuspensionDriver
    {
        readonly string _file;
        readonly JsonSerializerSettings _settings = new()
        {
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.Indented
        };

        public JsonSuspensionDriver(string file) => _file = file;

        public IObservable<Unit> InvalidateState()
        {
            if (File.Exists(_file))
            {
                File.Delete(_file);
            }
            return Observable.Return(Unit.Default);
        }

        public IObservable<object> LoadState()
        {
            string json = File.ReadAllText(_file);
            object? state = JsonConvert.DeserializeObject<object>(json, _settings);
            (state is not null).Assert("Could not deserialize app state!");

            return Observable.Return(state);
        }

        public IObservable<Unit> SaveState(object state)
        {
            string json = JsonConvert.SerializeObject(state, _settings);
            File.WriteAllText(_file, json);

            return Observable.Return(Unit.Default);
        }
    }
}
