using Autodesk.Revit.DB;
using System;
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
        public AddRuleNameWindow(string tag, Document document, List<RuleNameObject> rulesNames)
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

            Parameters.Sort();


            if (rulesNames.Find(x => x.Tag == tag) != null)
            {
                var listParameters = rulesNames.Find(x => x.Tag == tag).Parameters;
                var listSuffixes = rulesNames.Find(x => x.Tag == tag).Suffixes;
                var listPrefixes = rulesNames.Find(x => x.Tag == tag).Prefixes;
                var listDivides = rulesNames.Find(x => x.Tag == tag).Divides;

                

                var sourceParameters = new ObservableCollection<string>();

                foreach (var item in Parameters)
                {
                    sourceParameters.Add(item);
                }

                for (int i = 0; i < listParameters.Count; i++)
                {
                    if (sourceParameters.Count > 0)
                    {
                        var number = new Random().Next(100000, 999999) + i;
                        var parameterName = listParameters[i];
                        var comboBox = new ComboBox();
                        comboBox.ItemsSource = sourceParameters;
                        comboBox.SelectedItem = parameterName;

                        var parameterItem = new ParameterItem
                        {
                            Number = number,
                            ComboBox = comboBox,
                            SourceParameters = sourceParameters,
                            SelectedParameter = parameterName,
                            Prefix = listPrefixes[i],
                            Suffix = listSuffixes[i],
                            Divide = listDivides[i],
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
            public string Suffix { get; set; }
            public string Prefix { get; set; }
            public bool Divide { get; set; }
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
                    Prefix = "",
                    Suffix = "",
                    Divide = true,
                    SourceParameters = sourceParameters,
                    SelectedParameter = sourceParameters[0],
                };

                ParameterItems.Add(parameterItem);
            }

            LsbParameters.Items.Refresh();
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
                Suffixes = GetSuffixesFromTextBox(),
                Prefixes = GetPrefixesFromTextBox(),
                Divides = GetDividesFromCheckBox(),
            };

            DialogResult = true;
            Close();
        }

        private List<bool> GetDividesFromCheckBox()
        {
            var divides = new List<bool>();

            foreach (var item in LsbParameters.Items)
            {
                var parameterItem = item as ParameterItem;
                var changePrefixes = parameterItem.Divide;
                divides.Add(changePrefixes);
            }

            return divides;
        }

        private List<string> GetPrefixesFromTextBox()
        {
            var prefixes = new List<string>();

            foreach (var item in LsbParameters.Items)
            {
                var parameterItem = item as ParameterItem;
                var changePrefixes = parameterItem.Prefix;
                prefixes.Add(changePrefixes);
            }

            return prefixes;
        }
        private List<string> GetSuffixesFromTextBox()
        {
            var suffixes = new List<string>();

            foreach (var item in LsbParameters.Items)
            {
                var parameterItem = item as ParameterItem;
                var changeSuffixes = parameterItem.Suffix;
                suffixes.Add(changeSuffixes);
            }

            return suffixes;
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
