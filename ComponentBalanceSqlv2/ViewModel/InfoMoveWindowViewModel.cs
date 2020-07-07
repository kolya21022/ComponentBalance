using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ComponentBalanceSqlv2.Commands;
using ComponentBalanceSqlv2.Data;
using ComponentBalanceSqlv2.Data.Moves;
using ComponentBalanceSqlv2.Utils;
using System.Reflection;
using ComponentBalanceSqlv2.Model;

namespace ComponentBalanceSqlv2.ViewModel
{
    public class InfoMoveWindowViewModel : BaseNotificationClass
    {
        #region Private Fields
        private readonly DomainContext _context;
        private readonly WorkGuild _login;
        private readonly bool _isClose;
        private ObservableCollection<Move> _moves;
        private Move _selectedMove;
        #endregion

        #region Public fields
        public Move SelectedMove
        {
            get { return _selectedMove; }
            set
            {
                _selectedMove = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Move> Moves
        {
            get { return _moves; }
            set
            {
                _moves = value;
                OnPropertyChanged();
            }
        }
        public Balance Balance { get;  }
        #endregion

        #region Commands
        private ICommand _deleteCommand;
        public ICommand DeleteCommand => _deleteCommand ?? (_deleteCommand = new RelayCommand(obj => Delete(), obj => !_isClose));
        public void Delete()
        {
            var result = BuildMessageBox.GetConfirmMessageBox("Удалить запись?");
            if (result != MessageBoxResult.Yes) { return; }

            Balance.DeleteMove(_context, SelectedMove);
            _context.SaveChanges();
            Reload(Balance);
        }

        #endregion

        public InfoMoveWindowViewModel(DomainContext context, WorkGuild login, bool isClose, Balance balance)
        {
            _context = context;
            _login = login;
            _isClose = isClose;
            Balance = balance;
            Reload(Balance);
        }

        private void Reload(Balance balance)
        {
            _context.Entry(balance).Reload();
            Moves = new ObservableCollection<Move>(balance.Moves);
            if (_login.Lvl == 1)
            {
                foreach (var move in Moves)
                {
                    var baseType = move.GetType().GetTypeInfo().BaseType;
                    if (baseType == null) { return; }
                    if (baseType.Name != "ImportWorkGuildMove")
                    {
                        move.IsUserCanDelete = true;
                    }
                }
            }
            SelectedMove = null;
        }
    }
}
