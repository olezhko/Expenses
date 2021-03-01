using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Expenses.Annotations;
using SQLite;

namespace Expenses
{
    public class Transaction:INotifyPropertyChanged
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        private string _moneySource;
        public string MoneySource
        {
            get { return _moneySource; }
            set
            {
                _moneySource = value;
                OnPropertyChanged(nameof(MoneySource));
            }
        }

        private double _amount;
        public double Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                OnPropertyChanged(nameof(Amount));
            }
        }

        private DateTime _dateTime;
        public DateTime DateTime
        {
            get { return _dateTime; }
            set
            {
                _dateTime = value;
                OnPropertyChanged(nameof(DateTime));
            }
        }

        private string _comment;
        public string Comment
        {
            get { return _comment; }
            set
            {
                _comment = value;
                OnPropertyChanged(nameof(Comment));
            }
        }

        private TransactionType _type;
        public TransactionType Type
        {
            get { return _type; }
            set
            {
                _type = value;
                OnPropertyChanged(nameof(Type));
            }
        }


        public Transaction()
        {
            DateTime = DateTime.Now;;
            Amount = 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class IncomingMoneySource : INotifyPropertyChanged
    {
        public double Amount { get; set; }
        public string MoneySource { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }
    }

    public enum TransactionType
    {
        [Description("Пополнение")]
        Incoming,
        [Description("Расход")]
        Expense,
        [Description("В наличные")]
        ToCash,
        [Description("С Наличных")]
        FromCash,
    }


    public class TransactionDb
    {
        SQLiteConnection database;
        private string databasePath;
        public TransactionDb()
        {
            databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MyData.db");
            database = new SQLiteConnection(databasePath);
            database.CreateTable<Transaction>();
        }

        public IEnumerable<Transaction> GetItems()
        {
            return (from i in database.Table<Transaction>() select i).ToList();
        }

        public IEnumerable<Transaction> GetItems(DateTime start,DateTime end)
        {
            var endWithEndOfDay = new DateTime(end.Year, end.Month, end.Day, 23, 59, 59);
            return (from i in database.Table<Transaction>() where i.DateTime < endWithEndOfDay && i.DateTime>start select i).ToList();
        }

        public int SaveTrainingExerciseItem(Transaction item)
        {
            if (item.Id != 0)
            {
                database.Update(item);
                return item.Id;
            }
            return database.Insert(item);
        }

        public IEnumerable<Transaction> FindByKey(string searchArgsKeyword, List<string> searchSections)
        {
            if (String.IsNullOrEmpty(searchArgsKeyword))
            {
                return GetItems();
            }

            if (searchSections.Count == 0)
            {
                return (from i in database.Table<Transaction>() where (i.Comment.Contains(searchArgsKeyword) || i.MoneySource.Contains(searchArgsKeyword)) select i).ToList();
            }

            var searchSection = searchSections[0];
            switch (searchSection)
            {
                case nameof(Transaction.Comment):
                    return (from i in database.Table<Transaction>() where i.Comment.Contains(searchArgsKeyword) select i).ToList();
                case nameof(Transaction.MoneySource):
                    return (from i in database.Table<Transaction>() where i.MoneySource.Contains(searchArgsKeyword) select i).ToList();
            }
            return GetItems();
        }

        public void MakeCopy(string filename)
        {
            File.Copy(databasePath,filename);
        }

        public void RemoveItem(Transaction item)
        {
            database.Delete(item);
        }
    }
}
