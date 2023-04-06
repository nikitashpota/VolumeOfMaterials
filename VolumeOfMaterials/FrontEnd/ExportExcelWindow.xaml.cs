using Microsoft.Win32;
using System.Windows;
using VolumeOfMaterials.Properties;

namespace VolumeOfMaterials.FrontEnd
{
    /// <summary>
    /// Логика взаимодействия для ExportExcelWindow.xaml
    /// </summary>
    public partial class ExportExcelWindow
    {

        public ExportExcelWindow()
        {

            InitializeComponent();
            txtExportTable.Text = Settings.Default.PathExport;
            txtImportTable.Text = Settings.Default.PathImport;
            txtExportBook.Text = Settings.Default.ExportBook;
            txtImportBook.Text = Settings.Default.ImportBook;

        }


        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
            if (openFileDialog.ShowDialog() == true)
            {
                txtImportTable.Text = openFileDialog.FileName;
            }

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
            if (txtImportTable.Text.Length != 0 && 
                txtImportBook.Text.Length != 0 && 
                txtExportBook.Text.Length != 0 && 
                txtExportTable.Text.Length != 0)
            {
                Settings.Default.PathImport = txtImportTable.Text;
                Settings.Default.ImportBook = txtImportBook.Text;
                Settings.Default.PathExport = txtExportTable.Text;
                Settings.Default.ExportBook = txtExportBook.Text;


                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Необходимо заполнить все параметры");
            }

        }
    }
}
