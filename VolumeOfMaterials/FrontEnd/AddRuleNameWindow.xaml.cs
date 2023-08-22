using Autodesk.Revit.DB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VolumeOfMaterials.Models;

namespace VolumeOfMaterials.FrontEnd
{
    /// <summary>
    /// Interaction logic for AddRuleNameWindow.xaml
    /// </summary>
    public partial class AddRuleNameWindow
    {
        private List<string> Parameters = new List<string> { Helpers.NAME_OF_STRUCTURE };
        ObservableCollection<ParameterItem> ParameterItems = new ObservableCollection<ParameterItem>();
        public RuleNameObject RuleNameObject { get; set; } = new RuleNameObject();
        private new string Tag { get; set; }
        public AddRuleNameWindow(string tag, Document document, Dictionary<string, List<string>> rulesNames)
        {
            InitializeComponent();

            Tag = tag;
            txbNameCategory.Text = $"{txbNameCategory.Text} {tag}";

            LsbParameters.DataContext = ParameterItems;

            if (tag == "Окна и Двери (OPN)") Parameters.Clear();

            var parameters = (new FilteredElementCollector(document)
                .OfCategory(GetCategoryFormTag(tag))
                .WhereElementIsElementType()?
                .ToElements().First())
                .Parameters;

            foreach (Parameter parameter in parameters)
            {
                Parameters.Add(parameter.Definition.Name);
            }


            if (rulesNames.ContainsKey(tag))
            {
                var listParameters = rulesNames[tag];

                var number = new Random().Next(100000, 999999);

                var sourceParameters = new ObservableCollection<string>();

                foreach (var item in Parameters)
                {
                    sourceParameters.Add(item);
                }

                foreach (var parameterName in listParameters)
                {
                    if (sourceParameters.Count > 0)
                    {
                        var comboBox = new ComboBox();
                        comboBox.ItemsSource = sourceParameters;
                        comboBox.SelectedItem = parameterName;

                        var parameterItem = new ParameterItem
                        {
                            Number = number,
                            ComboBox = comboBox,
                            SourceParameters = sourceParameters,
                            SelectedParameter = parameterName,
                        };

                        ParameterItems.Add(parameterItem);
                    }
                }
            }

        }


        private BuiltInCategory GetCategoryFormTag(string tag)
        {
            switch (tag)
            {
                case "Стены (WAL)":
                    return BuiltInCategory.OST_Walls;
                case "Перекрытия (SLB)":
                    return BuiltInCategory.OST_Floors;
                case "Потолки (CLG)":
                    return BuiltInCategory.OST_Ceilings;
                case "Кровля (ROF)":
                    return BuiltInCategory.OST_Roofs;
                case "Окна и Двери (OPN)":
                    return BuiltInCategory.OST_Windows;
                default:
                    return BuiltInCategory.OST_Walls;
            }
        }

        public class ParameterItem
        {
            public int Number { get; set; }
            public ObservableCollection<string> SourceParameters { get; set; }
            public ComboBox ComboBox { get; set; }
            public string SelectedParameter { get; set; }
        }

        private void BtnAddParameter_Click(object sender, RoutedEventArgs e)
        {
            var number = new Random().Next(100000, 999999);

            var sourceParameters = new ObservableCollection<string>();

            foreach (var item in Parameters)
            {
                sourceParameters.Add(item);
            }

            if (sourceParameters.Count > 0)
            {
                var comboBox = new ComboBox();
                comboBox.ItemsSource = sourceParameters;
                comboBox.SelectedIndex = 0;

                var parameterItem = new ParameterItem
                {
                    Number = number,
                    ComboBox = comboBox,
                    SourceParameters = sourceParameters,
                    SelectedParameter = sourceParameters[0],
                };

                ParameterItems.Add(parameterItem);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;

            if (comboBox.SelectedItem != null)
            {
                var item = comboBox.DataContext as ParameterItem;
                item.SelectedParameter = comboBox.SelectedItem.ToString();
            }
        }

        private void Button_Delete(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var dataContext = button.DataContext;
            var item = (ParameterItem)dataContext;
            var number = item.Number;
            ParameterItem toRemove = null;

            foreach (var selectItem in ParameterItems)
            {
                if (selectItem.Number == number)
                {
                    toRemove = selectItem;
                    break;
                }
            }
            if (toRemove != null)
                ParameterItems.Remove(toRemove);
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            RuleNameObject = new RuleNameObject
            {
                Tag = Tag,
                Parameters = GetSelectedItemsFromComboBoxes(),
            };

            DialogResult = true;
            Close();
        }

        private List<string> GetSelectedItemsFromComboBoxes()
        {
            List<string> selectedItems = new List<string>();

            foreach (var item in LsbParameters.Items)
            {
                var parameterItem = item as ParameterItem;
                var selectedParameter = parameterItem.SelectedParameter;
                selectedItems.Add(selectedParameter);
            }

            return selectedItems;
        }
    }
}
