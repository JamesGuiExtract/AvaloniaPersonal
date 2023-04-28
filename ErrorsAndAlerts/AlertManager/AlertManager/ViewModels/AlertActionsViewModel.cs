using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Models.AllEnums;
using Extract.ErrorHandling;
using Extract.ErrorsAndAlerts.ElasticDTOs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;

namespace AlertManager.ViewModels
{
    public class AlertActionsViewModel : ViewModelBase
    {
        #region Fields

        public IAlertActionsView? View { get; set; }
        private readonly IAlertActionLogger? _actionLogger;
        private readonly IElasticSearchLayer _elasticSearch;
        private readonly IDBService _dbService;

        [Reactive]
        private bool SnoozeDateEnabled { get; set; } = false;

        [Reactive]
        public string ActionTypeSelection { get; set; } = RESOLVE_ACTION;

        private const string RESOLVE_ACTION = "Resolve";
        private const string SNOOZE_ACTION = "Snooze";
        private const string MUTE_ACTION = "Mute";
        private const string COMMENT_ACTION = "Comment";

        public List<string> ActionsOptions { get; set; } = new()
        { RESOLVE_ACTION, SNOOZE_ACTION, MUTE_ACTION, COMMENT_ACTION};

        public DateTimeOffset? SnoozeUntilDate { get; set; } = null;

        [Reactive]
        public string AlertActionComment { get; set; } = "";

        [Reactive]
        public List<AlertsObject>? AlertList { get; set; }

        [Reactive]
        public AlertsObject? ThisAlert { get; set; }

        [Reactive]
        public AssociatedFilesViewModel? AssociatedFilesVM { get; set; }

        #endregion Fields

        #region Constructors

        public AlertActionsViewModel(
            AlertsObject alertObjectToDisplay,
            IAlertActionLogger alertActionLogger,
            IElasticSearchLayer elasticSearch,
            IDBService dBService)
        {
            try
            {
                //Enable or disable controls based on action type combo box selection
                this.WhenAnyValue(x => x.ActionTypeSelection).Subscribe(action => SetControlEnablings(action));

                _actionLogger = alertActionLogger;
                _elasticSearch = elasticSearch;
                _dbService = dBService;

                RefreshScreen(alertObjectToDisplay);
            }
            catch (Exception e)
            {
                throw new ExtractException("ELI53870", "Issue with initializing values", e);
            }
        }

        #endregion Constructors

        #region Methods

        private void SetControlEnablings(string actionType)
        {
            SnoozeDateEnabled = (actionType == SNOOZE_ACTION);
        }

        public void CommitAction()
        {
            try
            {
                if (ThisAlert == null)
                {
                    throw new Exception("current Alert is null");
                }

                AlertActionDto newAction = new();
                newAction.ActionTime = DateTime.Now;
                newAction.ActionComment = AlertActionComment;

                //this seems like a bad design, suggestions please
                switch (ActionTypeSelection)
                {
                    case RESOLVE_ACTION:
                        newAction.ActionType = AlertActionType.Resolve.ToString();
                        break;
                    case SNOOZE_ACTION:
                        newAction.ActionType = AlertActionType.Snooze.ToString();
                        break;
                    case MUTE_ACTION:
                        newAction.ActionType = AlertActionType.Mute.ToString();
                        break;
                    case COMMENT_ACTION:
                        newAction.ActionType = AlertActionType.Comment.ToString();
                        break;
                }

                if (ActionTypeSelection == SNOOZE_ACTION)
                {
                    if (SnoozeUntilDate == null)
                    {
                        throw new Exception("Snooze action requires a snooze until date");
                    }
                    newAction.SnoozeDuration = SnoozeUntilDate.Value.DateTime;
                }

                ThisAlert.Actions.Add(newAction);
                _elasticSearch.AddAlertAction(
                    newAction,
                    ThisAlert.AlertId
                );

                View?.Close("Refresh");
            }
            catch (Exception e)
            {
                throw e.AsExtractException("ELI54200");
            }
        }

        public void RefreshScreen(AlertsObject newObject)
        {
            try
            {
                ThisAlert = newObject ?? throw new ExtractException("ELI53867", "Issue with refreshing screen, object to refresh to is null or invalid");

                //Need better error handling here. Problem with database retrieving files prevents taking actions on alerts.
                AssociatedFilesVM = new(ThisAlert, _dbService);
            }
            catch (Exception e)
            {
                throw new ExtractException("ELI53868", "Issue refreshing screen", e);
            }
        }

        #endregion Methods
    }
}