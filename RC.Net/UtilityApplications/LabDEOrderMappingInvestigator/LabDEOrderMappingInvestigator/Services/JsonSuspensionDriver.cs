using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;

namespace LabDEOrderMappingInvestigator.Services
{
    /// <summary>
    /// Interface for a factory to support <see cref="JsonSuspensionDriver{TViewModel, TModel}"/>
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model to be serialized</typeparam>
    /// <typeparam name="TModel">The type that represents the serializable part of the view model</typeparam>
    public interface IAutoSuspendViewModelFactory<TViewModel, TModel>
    {
        /// <summary>
        /// Create a <see cref="TViewModel"/> from a <see cref="TModel"/> instance
        /// </summary>
        TViewModel CreateViewModel(TModel? model);

        /// <summary>
        /// Create a <see cref="TModel"/> from a <see cref="TViewModel"/> instance
        /// </summary>
        TModel CreateModel(TViewModel viewModel);
    }

    /// <summary>
    /// Class to support saving/loading the state, <see cref="TModel"/>, of a <see cref="TViewModel"/>
    /// </summary>
    /// <typeparam name="TViewModel">The view model type</typeparam>
    /// <typeparam name="TModel">The type of the serializable class to represent the view model state</typeparam>
    public class JsonSuspensionDriver<TViewModel, TModel> : ISuspensionDriver
    {
        readonly string _file;
        readonly IAutoSuspendViewModelFactory<TViewModel, TModel> _viewModelFactory;
        readonly JsonSerializerSettings _settings = new()
        {
            TypeNameHandling = TypeNameHandling.None,
            Formatting = Formatting.Indented
        };

        /// <summary>
        /// Create a suspension driver
        /// </summary>
        /// <param name="file">The file path to save/load the suspended state to/from</param>
        /// <param name="viewModelFactory">A class with methods to create a view model from a serialized model
        /// and to create a model to be serialized from a view model</param>
        public JsonSuspensionDriver( string file, IAutoSuspendViewModelFactory<TViewModel, TModel> viewModelFactory)
        {
            _file = file;
            _viewModelFactory = viewModelFactory;
        }

        /// <inheritdoc/>
        public IObservable<Unit> InvalidateState()
        {
            if (File.Exists(_file))
            {
                File.Delete(_file);
            }
            return Observable.Return(Unit.Default);
        }

        /// <inheritdoc/>
        public IObservable<object> LoadState()
        {
            File.Exists(_file).Assert("Application trace: app state file does not exist");

            string json = File.ReadAllText(_file);
            TModel? model = JsonConvert.DeserializeObject<TModel>(json, _settings);
            (model is not null).Assert("Could not deserialize app state!");

            object? viewModel = _viewModelFactory.CreateViewModel(model);
            (viewModel is not null).Assert("Could not create view model!");

            return Observable.Return(viewModel);
        }

        /// <inheritdoc/>
        public IObservable<Unit> SaveState(object state)
        {
            TViewModel viewModel = (TViewModel)state;

            string json = JsonConvert.SerializeObject(_viewModelFactory.CreateModel(viewModel), _settings);
            File.WriteAllText(_file, json);

            return Observable.Return(Unit.Default);
        }
    }
}
