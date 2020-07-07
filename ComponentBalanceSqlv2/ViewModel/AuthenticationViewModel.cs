using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ComponentBalanceSqlv2.Commands;
using ComponentBalanceSqlv2.Data;
using ComponentBalanceSqlv2.Model;
using ComponentBalanceSqlv2.Utils;

namespace ComponentBalanceSqlv2.ViewModel
{
    public class AuthenticationViewModel : BaseNotificationClass
    {
        public event EventHandler<AuthenticationEventArgs> OnAuthorize;

        #region Private Fields  
        private readonly DomainContext _context = new DomainContext();

        private WorkGuild _selectedLogin;
        private string _password;
        private string _message;
        private bool _showMessage;
        #endregion

        #region Public Properties      
        public List<WorkGuild> Logins { get; }
        public WorkGuild SelectedLogin
        {
            get { return _selectedLogin; }
            set
            {
                _selectedLogin = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                ShowMessage = value != string.Empty;
                OnPropertyChanged();
            }
        }

        public bool ShowMessage
        {
            get { return _showMessage; }
            set
            {
                _showMessage = value;
                OnPropertyChanged();
            }
        }


        #endregion

        #region Commands
        private RelayCommand _loginCommand;
        public ICommand LoginCommand => _loginCommand ?? (_loginCommand = new RelayCommand(arg => Authorize()));
        private void Authorize()
        {
            if (SelectedLogin.Password == _password)
            {
                Message = string.Empty;
                OnAuthorize?.Invoke(this, new AuthenticationEventArgs(_selectedLogin, true));
            }
            else
            {
                Message = $"Неверный пароль для цеха {_selectedLogin.WorkGuildNumber}!";
                OnAuthorize?.Invoke(this, new AuthenticationEventArgs(_selectedLogin, false));
            }
        }

        #endregion

        public AuthenticationViewModel()
        {
            Logins = _context.WorkGuilds.AsNoTracking().ToList();
            SelectedLogin = Logins.FirstOrDefault();
        }
    }
}
