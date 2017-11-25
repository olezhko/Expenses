using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace Expenses
{
    // сделать итого на всю статистику 
    // фильтр по датам от и до 
    public class MainViewModel:ViewModelBase
    {
        public RelayCommand DropFilterCommand { get; set; }
        public DateTime FilterStartDateTime { get; set; } = DateTime.Today;
        public DateTime FilterEndDateTime { get; set; } = DateTime.Today;
        public RelayCommand ApplyDateTimeFilterCommand { get; set; }
        public RelayCommand<SelectionChangedEventArgs> TabControlSelectChangedCommand { get; set; }
        public RelayCommand ApplicationClosingCommand { get; set; }
        private TransactionDb db = new TransactionDb();
        public MainViewModel()
        {
            ApplyDateTimeFilterCommand = new RelayCommand(ApplyDateTimeFilter);
            CellEditEndingCommand = new RelayCommand<DataGridCellEditEndingEventArgs>(CellEditEnding);
            TabControlSelectChangedCommand = new RelayCommand<SelectionChangedEventArgs>(TabControlSelectChanged);
            ApplicationClosingCommand = new RelayCommand(ApplicationClosing);
            DropFilterCommand = new RelayCommand(DropFilter);


            SourceItems = new ObservableCollection<string>();
            SourceItems.Add(CashSource);
            ExpensesItems = new ObservableCollection<Transaction>();
            ExpensesItems.CollectionChanged += ExpensesItems_CollectionChanged;
            LoadBase();
        }

        private void DropFilter()
        {
            LoadBase();
        }

        private void LoadBase()
        {
            var baseItems = db.GetItems();

            foreach (var transaction in baseItems)
            {
                ExpensesItems.Add(transaction);
                if (!SourceItems.Contains(transaction.MoneySource))
                {
                    SourceItems.Add(transaction.MoneySource);
                }
            }
            RaisePropertyChanged(nameof(ExpensesItems));
        }

        private void ApplicationClosing()
        {
            
        }

        private void ApplyDateTimeFilter()
        {
            ExpensesItems.Clear();
            var datePeriodItems = db.GetItems(FilterStartDateTime, FilterEndDateTime);
            foreach (var datePeriodItem in datePeriodItems)
            {
                ExpensesItems.Add(datePeriodItem);
            }
            RaisePropertyChanged(nameof(ExpensesItems));
        }

        private void TabControlSelectChanged(SelectionChangedEventArgs obj)
        {
            var tc = obj.Source as TabControl;
            if (obj.AddedItems!=null && tc!=null)
            {
                var header = (obj.AddedItems[0] as TabItem)?.Header;
                if (header != null && (string) header == "Статистика")
                {
                    CalculateIncoming();
                    CalculateBalance();
                    CalculateExpenses();
                }
            }
        }

        #region Транзакции
        public RelayCommand<DataGridCellEditEndingEventArgs> CellEditEndingCommand { get; set; }
        public object DataGridSelectedItem { get; set; }
        public ObservableCollection<Transaction> ExpensesItems { get; set; }
        public ObservableCollection<String> SourceItems { get; set; }



        enum DataGridColumns
        {
            DateTimeField,
            AmountField,
            TypeField,
            MoneySourceField,
            CommentField
        }

        private void CellEditEnding(DataGridCellEditEndingEventArgs e)
        {
            var columnDisplayIndex = (DataGridColumns)e.Column.DisplayIndex;

            if (DataGridSelectedItem is Transaction selectedTransaction)
            {
                if (columnDisplayIndex == DataGridColumns.MoneySourceField)
                {
                    if (!SourceItems.Contains(selectedTransaction.MoneySource))
                    {
                        SourceItems.Add(selectedTransaction.MoneySource);
                    }
                }               
            }
        }

        private void ExpensesItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (Transaction eNewItem in e.NewItems)
                {
                    db.SaveTrainingExerciseItem(eNewItem);
                    eNewItem.PropertyChanged += ENewItem_PropertyChanged;
                }

            if (e.OldItems != null)
                foreach (Transaction eNewItem in e.OldItems)
                {
                    eNewItem.PropertyChanged -= ENewItem_PropertyChanged;
                }
        }

        private void ENewItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Transaction send = sender as Transaction;
            db.SaveTrainingExerciseItem(send);
        }
        #endregion

        #region Поступления

        public double TotalIncoming { get; set; }
        public ObservableCollection<IncomingMoneySource> IncomingMoneyItems { get; set; }
        private void CalculateIncoming()
        {
            var dic = new Dictionary<string,int>();
            foreach (var expensesItem in ExpensesItems)
            {
                if (expensesItem.Amount > 0 && expensesItem.Type == TransactionType.Incoming)
                {
                    if (!dic.ContainsKey(expensesItem.MoneySource))
                    {
                        dic.Add(expensesItem.MoneySource,0);
                    }
                    dic[expensesItem.MoneySource] += expensesItem.Amount;
                }
            }

            IncomingMoneyItems = new ObservableCollection<IncomingMoneySource>(dic.ToList().Select(a => new IncomingMoneySource() { Amount = a.Value, MoneySource = a.Key }));
            TotalIncoming = IncomingMoneyItems.Sum(a => a.Amount);
            RaisePropertyChanged(nameof(TotalIncoming));
            RaisePropertyChanged(nameof(IncomingMoneyItems));
        }
        #endregion

        #region Остатки

        private string CashSource = "Наличные";
        public double TotalBalance { get; set; }
        public ObservableCollection<IncomingMoneySource> BalanceMoneyItems { get; set; }
        private void CalculateBalance()
        {
            var dic = new Dictionary<string, int>();
            dic.Add(CashSource,0);
            foreach (var expensesItem in ExpensesItems)
            {
                if (expensesItem.MoneySource!=null)
                {
                    if (!dic.ContainsKey(expensesItem.MoneySource))
                    {
                        dic.Add(expensesItem.MoneySource, 0);
                    }
                    switch (expensesItem.Type)
                    {
                        case TransactionType.Incoming:
                            dic[expensesItem.MoneySource] += expensesItem.Amount;
                            break;
                        case TransactionType.Expense:
                            dic[expensesItem.MoneySource] -= expensesItem.Amount;
                            break;
                        case TransactionType.ToCash:
                            dic[expensesItem.MoneySource] -= expensesItem.Amount;
                            dic[CashSource] += expensesItem.Amount;
                            break;
                        case TransactionType.FromCash:
                            dic[expensesItem.MoneySource] += expensesItem.Amount;
                            dic[CashSource] -= expensesItem.Amount;
                            break;
                    }
                }
            }

            BalanceMoneyItems = new ObservableCollection<IncomingMoneySource>(dic.ToList().Select(a => new IncomingMoneySource() { Amount = a.Value, MoneySource = a.Key }));

            TotalBalance = BalanceMoneyItems.Sum(a => a.Amount);
            RaisePropertyChanged(nameof(TotalBalance));
            RaisePropertyChanged(nameof(BalanceMoneyItems));
        }
        public double TotalExpenses { get; set; }
        public ObservableCollection<Transaction> ExpensesMoneyItems { get; set; }
        private void CalculateExpenses()
        {
            ExpensesMoneyItems = new ObservableCollection<Transaction>(ExpensesItems.Where(a => a.Type == TransactionType.Expense));
            TotalExpenses = ExpensesMoneyItems.Sum(a => a.Amount);
            RaisePropertyChanged(nameof(TotalExpenses));
            RaisePropertyChanged(nameof(ExpensesMoneyItems));
        }
        #endregion
    }
}
