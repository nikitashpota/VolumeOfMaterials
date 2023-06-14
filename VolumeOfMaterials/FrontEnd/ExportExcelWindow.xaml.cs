using Autodesk.Revit.DB;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using VolumeOfMaterials.Properties;
using Button = System.Windows.Controls.Button;
using CheckBox = System.Windows.Controls.CheckBox;
using Grid = System.Windows.Controls.Grid;
using Settings = VolumeOfMaterials.Properties.Settings;

namespace VolumeOfMaterials.FrontEnd
{
    /// <summary>
    /// Логика взаимодействия для ExportExcelWindow.xaml
    /// </summary>
    /// 

    class ItemSelect
    {
        public string Text { get; set; }
        public RevitLinkType RevitLinkType { get; set; }
    }

    class ItemDelete
    {
        public string Text { get; set; }
        public Button DeleteButton { get; set; }
    }

    public partial class ExportExcelWindow
    {
        ObservableCollection<ItemDelete> Items = new ObservableCollection<ItemDelete>();
        ObservableCollection<ItemDelete> Books = new ObservableCollection<ItemDelete>();
        ObservableCollection<ItemSelect> Links = new ObservableCollection<ItemSelect>();
        public List<RevitLinkType> SelectedLinks = new List<RevitLinkType>();
        public ExportExcelWindow(IEnumerable<RevitLinkType> links)
        {

            //Assembly.LoadFrom("GongSolutions.WPF.DragDrop.dll");
            //Assembly.LoadFrom("MahApps.Metro.IconPacks.dll");
            //Assembly.LoadFrom("MahApps.Metro.IconPacks.MaterialDesign.dll");
            InitializeComponent();
            lbBooks.DataContext = Books;
            Books.Add(new ItemDelete { Text = "Стены (WAL)" });
            Books.Add(new ItemDelete { Text = "Перекрытия (SLB)" });
            Books.Add(new ItemDelete { Text = "Кровля (ROF)" });

            lvDescriptions.DataContext = Items;
            Items.Add(new ItemDelete { Text = "подземная  часть" });
            Items.Add(new ItemDelete { Text = "1-й этаж" });
            Items.Add(new ItemDelete { Text = "надземные этажи" });
            txtExportTable.Text = Settings.Default.PathExport;

            lbLinks.DataContext = Links;
            foreach (var link in links)
            {
                Links.Add(new ItemSelect { Text = link.Name, RevitLinkType = link });
            }
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
            if(txtAddDescription.Text.Length != 0)
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
    }
}
