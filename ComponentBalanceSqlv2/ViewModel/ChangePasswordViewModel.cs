using System.Data.Entity;
using System.Text;
using System.Windows;
using System.Windows.Input;
using ComponentBalanceSqlv2.Commands;
using ComponentBalanceSqlv2.Data;
using ComponentBalanceSqlv2.Model;
using ComponentBalanceSqlv2.Utils;

namespace ComponentBalanceSqlv2.ViewModel
{
    public class ChangePasswordViewModel : BaseNotificationClass
    {
        #region Private Fields

        private WorkGuild _login;
        private string _oldPassword;
        private string _newPassword;
        private string _compressionPassword;
        private string _message;
        private bool _showMessage;

        #endregion

        #region Public Fields

        public WorkGuild Login
        {
            get { return _login; }
            set
            {
                _login = value;
                OnPropertyChanged();
            }
        }
        public string OldPassword
        {
            get { return _oldPassword; }
            set
            {
                _oldPassword = value;
                OnPropertyChanged();
            }
        }
        public string NewPassword
        {
            get { return _newPassword; }
            set
            {
                _newPassword = value;
                OnPropertyChanged();
            }
        }
        public string CompressionPassword
        {
            get { return _compressionPassword; }
            set
            {
                _compressionPassword = value;
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
        private RelayCommand _saveCommand;
        public ICommand SaveCommand => _saveCommand ?? (_saveCommand = new RelayCommand(Save));

        private bool CanSave()
        {
            var isValid = true;
            var errorMessages = new StringBuilder();
            isValid &= Validator.IsNotNullSelectedObject(OldPassword, "Старый пароль", errorMessages);
            isValid &= Validator.IsNotNullSelectedObject(NewPassword, "Новый пароль", errorMessages);
            isValid &= Validator.IsNotNullSelectedObject(CompressionPassword, "Повторный пароль", errorMessages);
            if (isValid)
            {
                return true;
            }
            BuildMessageBox.GetLogicErrorMessageBox(errorMessages.ToString());
            return false;
        }

        private void Save(object obj)
        {   
            var messageBoxResult = BuildMessageBox.GetConfirmMessageBox("Вы действительно желаете изменить пароль? ");
            if (messageBoxResult != MessageBoxResult.Yes)
            {
                return;
            }

            if (!CanSave())
            {
                return;
            }

            try
            {
                if (Login.Password == OldPassword)
                {
                    if (NewPassword != CompressionPassword)
                    {
                        Message = "Новый пароль не совпадает с проверочным!";
                    }
                    else
                    {
                        Message = string.Empty;

                        using (var context = new DomainContext())
                        {
                            Login.Password = NewPassword;
                            context.Entry(Login).State = EntityState.Modified;
                            context.SaveChanges();
                        }

                        BuildMessageBox.GetInformationMessageBox("Новый пароль успешно сохранен!");
                        CloseCommand.Execute(obj);
                    }
                }
                else
                {
                    Message = $"Неверный старый пароль для цеха {_login.WorkGuildNumber}!";
                }
            }
            catch (StorageException ex)
            {
                BuildMessageBox.GetInformationMessageBox(Common.ShowDetailExceptionMessage(ex));
            }
        }

        private RelayCommand _closeCommand;
        public ICommand CloseCommand
        {
            get { return _closeCommand ?? (_closeCommand = new RelayCommand(o => ((Window)o).Close())); }
        }
        #endregion

        public ChangePasswordViewModel(WorkGuild login)
        {
            Login = login;
        }
    }
}
