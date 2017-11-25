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

        private int _amount;
        public int Amount
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
        public int Amount { get; set; }
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
        Incoming,
        Expense,
        ToCash,
        FromCash,
    }


    public class TransactionDb
    {
        SQLiteConnection database;

        public TransactionDb()
        {
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MyData.db");
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
    }
}
