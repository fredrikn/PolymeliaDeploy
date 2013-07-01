using System.Windows.Controls;

namespace PolymeliaDeployClient.Controls
{
    using System;
    using System.Activities.Core.Presentation;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Toolbox;
    using System.Activities.XamlIntegration;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows;

    using PolymeliaDeploy.Activities;
    using System.Activities.Presentation.Validation;
    using PolymeliaDeployClient.Workflow;
    using System.Activities;

    /// <summary>
    /// Interaction logic for WorkflowDesignerControl.xaml
    /// </summary>
    public partial class WorkflowDesignerControl : UserControl
    {
        WorkflowDesigner _wd;

        public WorkflowDesigner WorkflowDesigner
        {
            get { return _wd; }
        }


        public static readonly DependencyProperty SelectedEnvironmentProperty =
             DependencyProperty.Register("SelectedEnvironment", typeof(PolymeliaDeploy.Data.Environment),
             typeof(WorkflowDesignerControl), 
             new FrameworkPropertyMetadata());


        public PolymeliaDeploy.Data.Environment SelectedEnvironment
        {
            get { return (PolymeliaDeploy.Data.Environment)GetValue(SelectedEnvironmentProperty); }
            set { SetValue(SelectedEnvironmentProperty, value); }
        }


        public WorkflowDesignerControl()
        {
            InitializeComponent();

            var assemblies = Assembly.GetExecutingAssembly().GetReferencedAssemblies();

            CreateToolBox(assemblies);

            Loaded += (sender, args) =>
                {
                    try
                    {
                        LoadWorkflowDesigner();
                    }
                    catch(Exception e)
                    {
                        MessageBox.Show(
                            string.Format("Error while loading Workflow designer, error: {0}", e.Message),
                            "Error while loading desinger");
                    }
                };
        }


        public bool HasChanges { get; set; } 


        private void CreateToolBox(IEnumerable<AssemblyName> assemblies)
        {
            IDictionary<string, ToolboxCategory> toolboxCategories = new Dictionary<string,ToolboxCategory>();
            toolboxCategories.Add("", new ToolboxCategory { CategoryName = "General" });

            foreach (var assemblyName in assemblies)
            {
                var assembly = Assembly.Load(assemblyName);

                foreach (
                    var type in
                        assembly.GetTypes()
                                .Where(t => t != typeof(Start) && t.IsSubclassOf(typeof(PolymeliaNativiveActivity)))
                                .ToList())
                {
                    var displayName = GetToolBoxItemDisplayName(type);
                    var categoryName = GetToolBoxItemCategoryName(type);

                    if (!toolboxCategories.ContainsKey(categoryName))
                        toolboxCategories.Add(categoryName, new ToolboxCategory { CategoryName = categoryName });

                    if (!string.IsNullOrWhiteSpace(displayName))
                        toolboxCategories[categoryName].Tools.Add(new ToolboxItemWrapper(type, displayName));
                    else
                        toolboxCategories[categoryName].Tools.Add(new ToolboxItemWrapper(type));
                }
            }

            foreach (var toolboxCategory in toolboxCategories.Keys.OrderBy(k => k))
                toolbox.Categories.Add(toolboxCategories[toolboxCategory]);
        }


        private static string GetToolBoxItemDisplayName(Type type)
        {
            var displayName = "";

            var displayNames = type.GetCustomAttributes<DisplayNameAttribute>();
            
            if (displayNames.Any())
            {
                displayName = displayNames.First().DisplayName;
            }

            return displayName;
        }


        private static string GetToolBoxItemCategoryName(Type type)
        {
            var categoryName = "";

            var categoryNames = type.GetCustomAttributes<CategoryAttribute>();
            if (categoryNames.Any())
                categoryName = categoryNames.First().Category;

            return categoryName;
        }


        private void LoadWorkflowDesigner()
        {        
            _wd = new WorkflowDesigner();

            _wd.Context.Services.Publish<IValidationErrorService>(new DebugValidationErrorService());

            (new DesignerMetadata()).Register();

            if (string.IsNullOrWhiteSpace(SelectedEnvironment.WorkflowContent))
                _wd.Load(new Start { DisplayName = SelectedEnvironment.Name });
            else
            {
                var wf = ActivityXamlServices.Load(new StringReader(SelectedEnvironment.WorkflowContent));
                _wd.Load(wf);
            }

            _wd.ModelChanged += (sender, args) =>
                {
                    _wd.Flush();
                    SelectedEnvironment.WorkflowContent = _wd.Text;
                    SelectedEnvironment.HasChanges = true;
                };

            _wd.Flush();

            SelectedEnvironment.WorkflowContent = _wd.Text;

            designer.Children.Clear();
            designer.Children.Add(_wd.View);

            propertyView.Children.Clear();
            propertyView.Children.Add(_wd.PropertyInspectorView);
        }
    }
}