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
        const string DOCTYPE_RESOURCE_NAME = "Resources.LuceneSuggestionProvider.doctypes.txt";
        const string PROCEDURE_RESOURCE_NAME = "Resources.LuceneSuggestionProvider.procedures.txt";
        const string DEPT_RESOURCE_NAME = "Resources.LuceneSuggestionProvider.departments.txt";
        const string VENDORS_RESOURCE_NAME = "Resources.LuceneSuggestionProvider.vendors.txt";

        List<Control> controls;
        FlowLayoutPanel mainPanel;

        int? LimitNumberOfSuggestions { get; set; } = null;
        bool ShowLowScoringSuggestions { get; set; } = false;
        AutoDropDownMode AutoDropDownMode { get; set; } = AutoDropDownMode.Never;
        bool AutomaticallySelectBestMatchingItem { get; set; } = false;

        string ValidationList { get; set; } = PROCEDURE_RESOURCE_NAME;

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

            BuildPropertySetterGroup(topPanel, nameof(LimitNumberOfSuggestions));
            BuildPropertySetterGroup(topPanel, nameof(ShowLowScoringSuggestions));
            BuildPropertySetterGroup(topPanel, nameof(AutoDropDownMode));
            BuildPropertySetterGroup(topPanel, nameof(AutomaticallySelectBestMatchingItem));
            BuildPropertySetterGroup(topPanel, nameof(ValidationList));
        }

        void BuildPropertySetterGroup(Control container, string propertyName)
        {
            var group = new GroupBox { Text = propertyName, AutoSize = true };
            var panel = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, Dock = DockStyle.Fill, AutoSize = true };
            group.Controls.Add(panel);

            switch (propertyName)
            {
                case nameof(LimitNumberOfSuggestions):
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

                case nameof(ShowLowScoringSuggestions):
                    var showLowScoring = new CheckBox { Text = "Show low scoring suggestions", AutoSize = true };
                    panel.Controls.Add(showLowScoring);
                    showLowScoring.Checked = ShowLowScoringSuggestions;
                    showLowScoring.CheckedChanged += delegate
                    {
                        ShowLowScoringSuggestions = showLowScoring.Checked;
                        BuildDataEntryControls();
                    };
                    break;

                case nameof(AutoDropDownMode):
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

                case nameof(AutomaticallySelectBestMatchingItem):
                    var automaticallySelect = new CheckBox { Text = "Select best matching item automatically", AutoSize = true };
                    panel.Controls.Add(automaticallySelect);
                    automaticallySelect.Checked = AutomaticallySelectBestMatchingItem;
                    automaticallySelect.CheckedChanged += delegate
                    {
                        AutomaticallySelectBestMatchingItem = automaticallySelect.Checked;
                        BuildDataEntryControls();
                    };
                    break;

                case nameof(ValidationList):
                    var doctype = new RadioButton { Text = "Document types", AutoSize = true, Checked = ValidationList == DOCTYPE_RESOURCE_NAME };
                    panel.Controls.Add(doctype);
                    var procedure = new RadioButton { Text = "Procedures", AutoSize = true, Checked = ValidationList == PROCEDURE_RESOURCE_NAME };
                    panel.Controls.Add(procedure);
                    var dept = new RadioButton { Text = "Departments", AutoSize = true, Checked = ValidationList == DEPT_RESOURCE_NAME };
                    panel.Controls.Add(dept);
                    var vendors = new RadioButton { Text = "Vendors", AutoSize = true, Checked = ValidationList == VENDORS_RESOURCE_NAME };
                    panel.Controls.Add(vendors);
                    EventHandler validationListChangeDelegate = delegate
                    {
                        if (doctype.Checked)
                            ValidationList = DOCTYPE_RESOURCE_NAME;
                        else if (procedure.Checked)
                            ValidationList = PROCEDURE_RESOURCE_NAME;
                        else if (dept.Checked)
                            ValidationList = DEPT_RESOURCE_NAME;
                        else if (vendors.Checked)
                            ValidationList = VENDORS_RESOURCE_NAME;
                        BuildDataEntryControls();
                    };
                    doctype.CheckedChanged += validationListChangeDelegate;
                    procedure.CheckedChanged += validationListChangeDelegate;
                    dept.CheckedChanged += validationListChangeDelegate;
                    vendors.CheckedChanged += validationListChangeDelegate;
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
                    autoSuggest.UpdateAutoCompleteList(GetValidationListForDataEntryControl(ValidationList));
                    break;
                case DataEntryComboBox combo:
                    combo.DataEntryControlHost = this;
                    autoSuggest = GetAutoSuggest(combo);
                    var valList = GetValidationListForDataEntryControl(ValidationList).ToList();
                    autoSuggest.UpdateAutoCompleteList(valList);
                    CreateValidator(combo).SetAutoCompleteValues(valList.Select(kv => kv.Value.ToArray()).ToArray());
                    break;
                case DocumentTypeComboBox combo:
                    combo.DataEntryControlHost = this;
                    combo.SetAutoCompleteValues(GetValidationListForDocumentTypeCombo(ValidationList));
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
                    validator.SetAutoCompleteValues(
                        GetValidationListForDataEntryControl(ValidationList)
                        .Select(kv => kv.Value.ToArray())
                        .ToArray());
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

        static IEnumerable<string> GetValidationListForDocumentTypeCombo(string resourceName)
        {
            using var stream =
                Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(typeof(AutoSuggestDemo), resourceName);
            using var reader = new StreamReader(stream);
            while (!reader.EndOfStream)
                yield return reader.ReadLine();
        }

        static IEnumerable<KeyValuePair<string, List<string>>> GetValidationListForDataEntryControl(string resourceName)
        {
            using var stream =
                Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(typeof(AutoSuggestDemo), resourceName);
            using var reader = new StreamReader(stream);
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                yield return new KeyValuePair<string, List<string>>(line, new List<string> { line });
            }
        }
    }
}
