using Autodesk.Revit.DB;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using VolumeOfMaterials.Models;
using Button = System.Windows.Controls.Button;
using Grid = System.Windows.Controls.Grid;
using Settings = VolumeOfMaterials.Properties.Settings;

namespace VolumeOfMaterials.FrontEnd
{
    /// <summary>
    /// Логика взаимодействия для ExportExcelWindow.xaml
    /// </summary>
    /// 

    public class ItemSelect
    {
        public string Text { get; set; }
        public RevitLinkType RevitLinkType { get; set; }
    }

    public class ItemDelete
    {
        public string Text { get; set; }
        public string ListRules { get; set; } = "";

    }



    public partial class ExportExcelWindow
    {
        ObservableCollection<ItemDelete> Items = new ObservableCollection<ItemDelete>();
        ObservableCollection<ItemDelete> Books = new ObservableCollection<ItemDelete>();
        ObservableCollection<ItemSelect> Links = new ObservableCollection<ItemSelect>();
        public List<RevitLinkType> SelectedLinks = new List<RevitLinkType>();
        public List<RuleNameObject> RulesNames = new List<RuleNameObject>();
        private Document ExDocument { get; set; }
        public ExportExcelWindow(IEnumerable<RevitLinkType> links, Document document)
        {
            InitializeComponent();

            ExDocument = document;



            lbBooks.DataContext = Books;
            Books.Add(new ItemDelete { Text = "Стены (WAL)" });
            Books.Add(new ItemDelete { Text = "Перекрытия (SLB)" });
            Books.Add(new ItemDelete { Text = "Колонны (CLM)" });
            Books.Add(new ItemDelete { Text = "Балки (FRM)" });
            Books.Add(new ItemDelete { Text = "Кровля (ROF)" });
            Books.Add(new ItemDelete { Text = "Окна и Двери (OPN)" });
            Books.Add(new ItemDelete { Text = "Витражи (CRP)" });
            Books.Add(new ItemDelete { Text = "Потолки (CLG)" });
            Books.Add(new ItemDelete { Text = "Фундамент (FUN)" });

            lvDescriptions.DataContext = Items;
            txtExportTable.Text = Settings.Default.PathExport;

            if (links.Count() == 0)
            {
                var columnIndex = 2;
                foreach (UIElement element in grdMain.Children)
                {
                    var column = Grid.GetColumn(element);
                    if (column == columnIndex)
                    {
                        element.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }

                grdMain.ColumnDefinitions[2].Width = new GridLength(0);
                grdMain.ColumnDefinitions[1].Width = new GridLength(0);
            }

            lbLinks.DataContext = Links;
            foreach (var link in links)
            {
                Links.Add(new ItemSelect { Text = link.Name, RevitLinkType = link });
            }

            Loaded += ExportExcelWindow_Loaded;
            Closing += ExportExcelWindow_Closing;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)e.Source;
            var grid = (Grid)button.Parent;
            var textBlock = (TextBlock)grid.Children[0];
            var text = textBlock.Text;
            var tag = button.Tag.ToString();
            ItemDelete toRemove = null;
            ObservableCollection<ItemDelete> items = null;
            if (tag == "Books") { items = Books; } else { items = Items; }
            foreach (ItemDelete item in items)
            {
                if (item.Text == text)
                {
                    toRemove = item;
                    break;
                }
            }
            if (toRemove != null)
                items.Remove(toRemove);
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
            if (openFileDialog.ShowDialog() == true)
            {
                txtExportTable.Text = openFileDialog.FileName;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (txtExportTable.Text.Length != 0)
            {

                SelectedLinks = lbLinks.SelectedItems.Cast<ItemSelect>().Select(x => x.RevitLinkType).ToList();
                Settings.Default.PathExport = txtExportTable.Text;
                DialogResult = true;

                Close();
            }
            else
            {
                MessageBox.Show("Необходимо заполнить все параметры");
            }

        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (txtAddDescription.Text.Length != 0)
            {
                var newItem = new ItemDelete { Text = txtAddDescription.Text };
                Items.Add(newItem);
                txtAddDescription.Text = "";
            }
        }

        private void btnAddBooks_Click(object sender, RoutedEventArgs e)
        {
            if (txtAddBooks.Text.Length != 0)
            {
                var newItem = new ItemDelete { Text = txtAddBooks.Text };
                Books.Add(newItem);
                txtAddBooks.Text = "";
            }
        }

        private const string FileNameDes = "VOM.SETTINGS.DES.xml";
        private const string FileNameParams = "VOM.SETTINGS.PARAMS.xml";
        private const string FileNameBooks = "VOM.SETTINGS.BOOKS.xml";

        private void SaveListBoxData()
        {
            string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePathDes = Path.Combine(documentsFolder, FileNameDes);
            string filePathParams = Path.Combine(documentsFolder, FileNameParams);
            string filePathBooks = Path.Combine(documentsFolder, FileNameBooks);

            using (var stream = new FileStream(filePathDes, FileMode.Create))
            {
                var serializer = new XmlSerializer(typeof(ObservableCollection<ItemDelete>));
                serializer.Serialize(stream, Items);

                stream.Flush();
                stream.Close();
            }

            using (var stream = new FileStream(filePathParams, FileMode.Create))
            {
                var rulesNamesSerializer = new XmlSerializer(typeof(List<RuleNameObject>));
                rulesNamesSerializer.Serialize(stream, RulesNames);

                stream.Flush();
                stream.Close();
            }

            using (var stream = new FileStream(filePathBooks, FileMode.Create))
            {
                var rulesNamesSerializer = new XmlSerializer(typeof(ObservableCollection<ItemDelete>));
                rulesNamesSerializer.Serialize(stream, Books);

                stream.Flush();
                stream.Close();
            }
        }

        private void LoadListBoxDataParams()
        {
            string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePathParams = Path.Combine(documentsFolder, FileNameParams);

            if (!File.Exists(filePathParams))
            {
                return;
            }

            using (var stream = new FileStream(filePathParams, FileMode.Open))
            {

                var rulesNamesSerializer = new XmlSerializer(typeof(List<RuleNameObject>));
                var lst = (List<RuleNameObject>)rulesNamesSerializer.Deserialize(stream);
                lst.ForEach(x =>
                {
                    var itemDelete = Books.FirstOrDefault(item => item.Text == x.Tag);
                    if (itemDelete != null)
                    {
                        itemDelete.ListRules = String.Join(", ", CreateRulesForListBox(x));
                        lbBooks.Items.Refresh();
                    }

                });

                RulesNames = lst;
            }
        }

        private List<string> CreateRulesForListBox(RuleNameObject obj)
        {
            var res = new List<string>();
            for(int i = 0; i < obj.Parameters.Count; i++)
            {
                res.Add($"<{obj.Prefixes[i]}> {obj.Parameters[i]} <{obj.Suffixes[i]}>");
            }
            return res;
        }

        private void LoadListBoxDataBooks()
        {
            string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePathBooks = Path.Combine(documentsFolder, FileNameBooks);

            if (!File.Exists(filePathBooks))
            {
                return;
            }

            using (var stream = new FileStream(filePathBooks, FileMode.Open))
            {
                var serializer = new XmlSerializer(typeof(ObservableCollection<ItemDelete>));
                Books = (ObservableCollection<ItemDelete>)serializer.Deserialize(stream);
                lbBooks.DataContext = Books;
            }
        }

        private void LoadListBoxDataDes()
        {
            string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePathDes = Path.Combine(documentsFolder, FileNameDes);

            if (!File.Exists(filePathDes))
            {
                return;
            }

            using (var stream = new FileStream(filePathDes, FileMode.Open))
            {
                var serializer = new XmlSerializer(typeof(ObservableCollection<ItemDelete>));
                Items = (ObservableCollection<ItemDelete>)serializer.Deserialize(stream);
                lvDescriptions.DataContext = Items;
            }
        }

        private void ExportExcelWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveListBoxData();
        }

        private void ExportExcelWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadListBoxDataDes();
            LoadListBoxDataParams();
            LoadListBoxDataBooks();
        }

        private void BtnAddParameters(object sender, RoutedEventArgs e)
        {
            var button = (Button)e.Source;

            var grid = (Grid)button.Parent;
            var textBlock = (TextBlock)grid.Children[0];
            var tag = textBlock.Text;

            var w = new AddRuleNameWindow(tag, ExDocument, RulesNames);
            w.ShowDialog();

            if (!w.DialogResult.Value) return;

            if (RulesNames.Find(x => x.Tag == w.RuleNameObject.Tag) != null) 
            {
                var searchItem = RulesNames.Find(x => x.Tag == w.RuleNameObject.Tag);
                searchItem.Parameters = w.RuleNameObject.Parameters;
                searchItem.Suffixes = w.RuleNameObject.Suffixes;
                searchItem.Prefixes = w.RuleNameObject.Prefixes;
                searchItem.Divides = w.RuleNameObject.Divides;
            }
            else RulesNames.Add(
                new RuleNameObject
                {
                    Tag = w.RuleNameObject.Tag,
                    Parameters = w.RuleNameObject.Parameters,
                    Prefixes = w.RuleNameObject.Prefixes,
                    Suffixes = w.RuleNameObject.Suffixes,
                    Divides = w.RuleNameObject.Divides,
                });

            var item = ((Button)sender).DataContext as ItemDelete;
            item.ListRules = string.Join(", ", CreateRulesForListBox(w.RuleNameObject));
            lbBooks.Items.Refresh();



        }


    }
}
