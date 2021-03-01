using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Expenses
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // сделать дробные
        // придумать как сделать разные валюты - переключать программу или еещ че
        // сделать валидацию даты чтобы читать без времени или наоборот
        public MainWindow()
        {
            InitializeComponent();
            SearchTextBox.SectionsList = MainViewModel.SearchSections;

            ExpensesDataGrid.Items.SortDescriptions.Clear();
            // Add the new sort description
            var column = ExpensesDataGrid.Columns[0];
            ExpensesDataGrid.Items.SortDescriptions.Add(new SortDescription(column.SortMemberPath, ListSortDirection.Ascending));
            foreach (var col in ExpensesDataGrid.Columns)
            {
                col.SortDirection = null;
            }
            column.SortDirection = ListSortDirection.Ascending;
            ExpensesDataGrid.Items.Refresh();// Refresh items to display sort

            MainDataGrid.Items.SortDescriptions.Clear();
            // Add the new sort description
            column = MainDataGrid.Columns[0];
            MainDataGrid.Items.SortDescriptions.Add(new SortDescription(column.SortMemberPath, ListSortDirection.Ascending));
            foreach (var col in MainDataGrid.Columns)
            {
                col.SortDirection = null;
            }
            column.SortDirection = ListSortDirection.Ascending;
            MainDataGrid.Items.Refresh();// Refresh items to display sort
        }
    }
}
