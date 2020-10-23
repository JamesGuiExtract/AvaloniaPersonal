using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.DataEntry.Test
{
    public partial class AutoSuggestDemo : DataEntryControlHost
    {
        private List<string> docTypes;
        List<KeyValuePair<string, List<string>>> procedures;
        string[][] proceduresForValidator;
        List<Control> controls;
        FlowLayoutPanel mainPanel;

        int? LimitNumberOfSuggestions { get; set; } = null;
        bool ShowLowScoringSuggestions { get; set; } = false;
        AutoDropDownMode AutoDropDownMode { get; set; } = AutoDropDownMode.Never;
        bool AutomaticallySelectBestMatchingItem { get; set; } = false;

        public AutoSuggestDemo()
        {
            InitializeComponent();
        }

        internal void Run()
        {
            var form = new Form
            {
                Text = "LuceneAutoSuggestDemo",
                MinimumSize = new Size(550, 800),
            };
            form.Controls.Add(this);
            form.ShowDialog();
        }

        protected override void OnLoad(EventArgs e)
        {
            // Avoid exception for null data entry app
            ExtractException.BlockExceptionDisplays();

            base.OnLoad(e);

            ExtractException.EndBlockExceptionDisplays();

            docTypes = GetDocumentTypes().ToList();
            procedures = GetProcedures().ToList();
            proceduresForValidator = procedures.Select(kv => kv.Value.ToArray()).ToArray();

            BuildMainPanels();
            BuildDataEntryControls();
        }

        void BuildMainPanels()
        {
            Dock = DockStyle.Fill;
            mainPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, Dock = DockStyle.Fill, AutoSize = true };
            Controls.Add(mainPanel);
            var topPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, Dock = DockStyle.Top, AutoSize = true };
            mainPanel.Controls.Add(topPanel);

            BuildPropertySetterGroup(topPanel, nameof(IDataEntryAutoCompleteControl.LimitNumberOfSuggestions));
            BuildPropertySetterGroup(topPanel, nameof(IDataEntryAutoCompleteControl.ShowLowScoringSuggestions));
            BuildPropertySetterGroup(topPanel, nameof(IDataEntryAutoCompleteControl.AutoDropDownMode));
            BuildPropertySetterGroup(topPanel, nameof(IDataEntryAutoCompleteControl.AutomaticallySelectBestMatchingItem));
        }

        void BuildPropertySetterGroup(Control container, string propertyName)
        {
            var group = new GroupBox { Text = propertyName, AutoSize = true };
            var panel = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, Dock = DockStyle.Fill, AutoSize = true };
            group.Controls.Add(panel);

            switch (propertyName)
            {
                case nameof(IDataEntryAutoCompleteControl.LimitNumberOfSuggestions):
                    var limitNumberOfSuggestions = new CheckBox { Text = "Limit suggestions for typed text", AutoSize = true };
                    panel.Controls.Add(limitNumberOfSuggestions);
                    var numericBox = new NumericUpDown { Minimum = 1, Maximum = int.MaxValue, Enabled = LimitNumberOfSuggestions.HasValue };
                    numericBox.ValueChanged += delegate
                    {
                        LimitNumberOfSuggestions = numericBox.Enabled ? (int)numericBox.Value : int.MaxValue;
                        BuildDataEntryControls();
                    };
                    panel.Controls.Add(numericBox);
                    limitNumberOfSuggestions.Checked = LimitNumberOfSuggestions.HasValue;
                    limitNumberOfSuggestions.CheckedChanged += delegate
                    {
                        numericBox.Enabled = limitNumberOfSuggestions.Checked;
                        LimitNumberOfSuggestions = numericBox.Enabled ? (int)numericBox.Value : int.MaxValue;
                        BuildDataEntryControls();
                    };
                    break;

                case nameof(IDataEntryAutoCompleteControl.ShowLowScoringSuggestions):
                    var showLowScoring = new CheckBox { Text = "Show low scoring suggestions", AutoSize = true };
                    panel.Controls.Add(showLowScoring);
                    showLowScoring.Checked = ShowLowScoringSuggestions;
                    showLowScoring.CheckedChanged += delegate
                    {
                        ShowLowScoringSuggestions = showLowScoring.Checked;
                        BuildDataEntryControls();
                    };
                    break;

                case nameof(IDataEntryAutoCompleteControl.AutoDropDownMode):
                    var never = new RadioButton { Text = "Never", AutoSize = true, Checked = AutoDropDownMode == AutoDropDownMode.Never };
                    panel.Controls.Add(never);
                    var always = new RadioButton { Text = "Always", AutoSize = true, Checked = AutoDropDownMode == AutoDropDownMode.Always };
                    panel.Controls.Add(always);
                    var whenEmpty = new RadioButton { Text = "WhenEmpty", AutoSize = true, Checked = AutoDropDownMode == AutoDropDownMode.WhenEmpty };
                    panel.Controls.Add(whenEmpty);
                    EventHandler changeDelegate = delegate
                    {
                        if (never.Checked)
                            AutoDropDownMode = AutoDropDownMode.Never;
                        else if (always.Checked)
                            AutoDropDownMode = AutoDropDownMode.Always;
                        else if (whenEmpty.Checked)
                            AutoDropDownMode = AutoDropDownMode.WhenEmpty;
                        BuildDataEntryControls();
                    };
                    never.CheckedChanged += changeDelegate;
                    always.CheckedChanged += changeDelegate;
                    whenEmpty.CheckedChanged += changeDelegate;
                    break;

                case nameof(IDataEntryAutoCompleteControl.AutomaticallySelectBestMatchingItem):
                    var automaticallySelect = new CheckBox { Text = "Select best matching item automatically", AutoSize = true };
                    panel.Controls.Add(automaticallySelect);
                    automaticallySelect.Checked = AutomaticallySelectBestMatchingItem;
                    automaticallySelect.CheckedChanged += delegate
                    {
                        AutomaticallySelectBestMatchingItem = automaticallySelect.Checked;
                        BuildDataEntryControls();
                    };
                    break;
            }
            container.Controls.Add(group);
        }

        void BuildDataEntryControls()
        {
            SuspendLayout();

            try
            {
                if (controls != null)
                {
                    foreach (var control in controls)
                    {
                        mainPanel.Controls.Remove(control);
                        (control as IDisposable)?.Dispose();
                    }
                }
                controls = new List<IEnumerable<Control>>
                {
                    AddControl(new DataEntryTextBox()),
                    AddControl(new DataEntryComboBox()),
                    AddControl(new DocumentTypeComboBox()),
                    AddTable()
                }
                .SelectMany(x => x)
                .ToList();
            }
            finally
            {
                ResumeLayout();
            }
        }
        IEnumerable<Control> AddControl(IDataEntryAutoCompleteControl dataEntryAutoCompleteControl)
        {
            var label = new Label
            {
                Text = dataEntryAutoCompleteControl.GetType().Name,
                AutoSize = true
            };
            mainPanel.Controls.Add(label);

            var control = dataEntryAutoCompleteControl as Control;
            mainPanel.Controls.Add(control);
            control.Width = 520;
            SetupDataEntryControl(dataEntryAutoCompleteControl);
            return new[] { label, control };
        }

        void SetupDataEntryControl(IDataEntryAutoCompleteControl dataEntryAutoCompleteControl)
        {
            dataEntryAutoCompleteControl.LimitNumberOfSuggestions = LimitNumberOfSuggestions;
            dataEntryAutoCompleteControl.ShowLowScoringSuggestions = ShowLowScoringSuggestions;
            dataEntryAutoCompleteControl.AutoDropDownMode = AutoDropDownMode;
            dataEntryAutoCompleteControl.AutomaticallySelectBestMatchingItem = AutomaticallySelectBestMatchingItem; 

            LuceneAutoSuggest autoSuggest;
            switch (dataEntryAutoCompleteControl)
            {
                case DataEntryTextBox textBox:
                    textBox.DataEntryControlHost = this;
                    autoSuggest = GetAutoSuggest(textBox);
                    autoSuggest.UpdateAutoCompleteList(procedures);
                    break;
                case DataEntryComboBox combo:
                    combo.DataEntryControlHost = this;
                    autoSuggest = GetAutoSuggest(combo);
                    autoSuggest.UpdateAutoCompleteList(procedures);
                    CreateValidator(combo).SetAutoCompleteValues(proceduresForValidator);
                    break;
                case DocumentTypeComboBox combo:
                    combo.DataEntryControlHost = this;
                    combo.SetAutoCompleteValues(docTypes);
                    break;
                case DataEntryTable table:
                    table.DataEntryControlHost = this;
                    var attribute = new AttributeClass { Name = "A" };
                    var tableAttribute = new AttributeClass { Name = "_" };
                    tableAttribute.SubAttributes.PushBack(attribute);
                    var tableAttributes = new IUnknownVectorClass();
                    tableAttributes.PushBack(tableAttribute);
                    table.SetAttributes(tableAttributes);
                    var validator = (DataEntryValidator)AttributeStatusInfo.GetStatusInfo(attribute).Validator;
                    validator.SetAutoCompleteValues(proceduresForValidator);
                    break;
            }
        }

        IEnumerable<Control> AddTable()
        {
            var label = new Label
            {
                Text = nameof(DataEntryTable),
                AutoSize = true
            };
            mainPanel.Controls.Add(label);

            var table = new DataEntryTable { AttributeName = "_", AllowUserToAddRows = false };
            table.Columns.Add(new DataEntryTableColumn { AttributeName = "A", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });

            var control = (Control)table;
            control.Width = 520;
            mainPanel.Controls.Add(control);

            SetupDataEntryControl(table);

            return new[] { label, control };
        }


        static LuceneAutoSuggest GetAutoSuggest(IDataEntryAutoCompleteControl dataEntryControl)
        {
            static IEnumerable<FieldInfo> GetAutoSuggestFields(Type t)
            {
                if (t == null)
                {
                    return Enumerable.Empty<FieldInfo>();
                }

                return t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(field => field.FieldType == typeof(LuceneAutoSuggest))
                    .Concat(GetAutoSuggestFields(t.BaseType));
            }

            return (LuceneAutoSuggest)
                GetAutoSuggestFields(dataEntryControl.GetType())
                .First()
                .GetValue(dataEntryControl);
        }

        static DataEntryValidator CreateValidator(DataEntryComboBox dataEntryControl)
        {
            static IEnumerable<PropertyInfo> GetValidatorFields(Type t)
            {
                if (t == null)
                {
                    return Enumerable.Empty<PropertyInfo>();
                }

                return new[] { t.GetProperty("ActiveValidator", BindingFlags.NonPublic | BindingFlags.Instance) }
                    .Concat(GetValidatorFields(t.BaseType));
            }

            var validator = new DataEntryValidator();
            GetValidatorFields(dataEntryControl.GetType())
                .First()
                .SetValue(dataEntryControl, validator);
            return validator;
        }

        static IEnumerable<string> GetDocumentTypes()
        {
            using var stream =
                Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(typeof(AutoSuggestDemo),"Resources.LuceneSuggestionProvider.doctypes.txt");
            using var reader = new StreamReader(stream);
            while (!reader.EndOfStream)
                yield return reader.ReadLine();
        }

        static IEnumerable<KeyValuePair<string, List<string>>> GetProcedures()
        {
            using var stream =
                Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(typeof(AutoSuggestDemo),"Resources.LuceneSuggestionProvider.procedures.txt");
            using var reader = new StreamReader(stream);
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                yield return new KeyValuePair<string, List<string>>(line, new List<string> { line });
            }
        }
    }
}
