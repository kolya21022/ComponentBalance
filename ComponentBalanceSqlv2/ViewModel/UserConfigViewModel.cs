using System;
using System.Windows;
using System.Windows.Input;
using ComponentBalanceSqlv2.Commands;
using ComponentBalanceSqlv2.Model;
using ComponentBalanceSqlv2.Utils;
using ComponentBalanceSqlv2.View.Windows;

namespace ComponentBalanceSqlv2.ViewModel
{
    public class UserConfigViewModel : BaseNotificationClass
    {
        #region Private Fields 

        private string _foxProDbFolderComposition;
        private string _foxProDbFolderFox60ArmLimit;
        private string _foxProDbFolderTemp;
        private string _foxProDbFolderTempWork;
        private string _foxProDbFolderFox60ArmBase;
        private string _foxProDbFolderFoxProSkl;
        private string _foxProDbFolderFoxProSkl58;
        private bool _isRunningFullscreen;
        private double _fontSize;

        #endregion

        #region Public Properties 

        public string FoxProDbFolderComposition
        {
            get { return _foxProDbFolderComposition; }
            set
            {
                Properties.Settings.Default.FoxProDbFolder_Composition = value;
                _foxProDbFolderComposition = value;
                OnPropertyChanged();
            }
        }

        public string FoxProDbFolderFox60ArmLimit
        {
            get { return _foxProDbFolderFox60ArmLimit; }
            set
            {
                Properties.Settings.Default.FoxProDbFolder_Fox60_Arm_Limit = value;
                _foxProDbFolderFox60ArmLimit = value;
                OnPropertyChanged();
            }
        }

        public string FoxProDbFolderTemp
        {
            get
            { return _foxProDbFolderTemp; }
            set
            {
                Properties.Settings.Default.FoxProDbFolder_Temp = value;
                _foxProDbFolderTemp = value;
                OnPropertyChanged();
            }
        }

      

        public string FoxProDbFolderTempWork
        {
            get { return _foxProDbFolderTempWork; }
            set
            {
                Properties.Settings.Default.FoxProDbFolder_Temp_Work = value;
                _foxProDbFolderTempWork = value;
                OnPropertyChanged();
            }
        }

        public string FoxProDbFolderFox60ArmBase
        {
            get { return _foxProDbFolderFox60ArmBase; }
            set
            {
                Properties.Settings.Default.FoxProDbFolder_Fox60_Arm_Base = value;
                _foxProDbFolderFox60ArmBase = value;
                OnPropertyChanged();
            }
        }

        public string FoxProDbFolderFoxProSkl
        {
            get { return _foxProDbFolderFoxProSkl; }
            set
            {
                Properties.Settings.Default.FoxProDbFolder_FoxPro_Skl = value;
                _foxProDbFolderFoxProSkl = value;
                OnPropertyChanged();
            }
        }

        public string FoxProDbFolderFoxProSkl58
        {
            get { return _foxProDbFolderFoxProSkl58; }
            set
            {
                Properties.Settings.Default.FoxProDbFolder_FoxPro_Skl58 = value;
                _foxProDbFolderFoxProSkl58 = value;
                OnPropertyChanged();
            }
        }

        public bool IsRunningFullscreen
        {
            get { return _isRunningFullscreen; }
            set
            {
                Properties.Settings.Default.IsRunInFullscreen = value;
                _isRunningFullscreen = value;
                OnPropertyChanged();
            }
        }

        public Double FontSize
        {
            get { return _fontSize; }
            set
            {
                Properties.Settings.Default.FontSize = value;
                _fontSize = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands
        #region Сохранить
        private RelayCommand _saveCommand;
        public ICommand SaveCommand => _saveCommand ?? (_saveCommand = new RelayCommand(Save));

        private string[] _parametrSave;
        public void Save(object parameter)
        {
            _parametrSave = ((string)parameter).Split('|');
            DoSaveCorrect(parameter);
        }

        private void DoSaveCorrect(object obj)
        {
            var foxProDbFolderComposition = _parametrSave[0];
            if (FoxProDbFolderComposition != foxProDbFolderComposition)
            {
                FoxProDbFolderComposition = foxProDbFolderComposition;
                Properties.Settings.Default.FoxProDbFolder_Composition = foxProDbFolderComposition;
            }
            var foxProDbFolderFox60ArmLimit = _parametrSave[1];
            if (FoxProDbFolderFox60ArmLimit != foxProDbFolderFox60ArmLimit)
            {
                FoxProDbFolderFox60ArmLimit = foxProDbFolderFox60ArmLimit;
                Properties.Settings.Default.FoxProDbFolder_Fox60_Arm_Limit = foxProDbFolderFox60ArmLimit;
            }

            var foxProDbFolderTemp = _parametrSave[2];
            if (FoxProDbFolderTemp != foxProDbFolderTemp)
            {
                FoxProDbFolderTemp = foxProDbFolderTemp;
                Properties.Settings.Default.FoxProDbFolder_Temp = foxProDbFolderTemp;
            }

            var foxProDbFolderTempWork = _parametrSave[3];
            if (FoxProDbFolderTempWork != foxProDbFolderTempWork)
            {
                FoxProDbFolderTempWork = foxProDbFolderTempWork;
                Properties.Settings.Default.FoxProDbFolder_Temp_Work = foxProDbFolderTempWork;
            }

            var foxProDbFolderFox60ArmBase = _parametrSave[4];
            if (FoxProDbFolderFox60ArmBase != foxProDbFolderFox60ArmBase)
            {
                FoxProDbFolderFox60ArmBase = foxProDbFolderFox60ArmBase;
                Properties.Settings.Default.FoxProDbFolder_Fox60_Arm_Base = foxProDbFolderFox60ArmBase;
            }

            var foxProDbFolderFoxProSkl = _parametrSave[5];
            if (FoxProDbFolderFoxProSkl != foxProDbFolderFoxProSkl)
            {
                FoxProDbFolderFoxProSkl = foxProDbFolderFoxProSkl;
                Properties.Settings.Default.FoxProDbFolder_FoxPro_Skl = foxProDbFolderFoxProSkl;
            }

            var foxProDbFolderFoxProSkl58 = _parametrSave[6];
            if (FoxProDbFolderFoxProSkl58 != foxProDbFolderFoxProSkl58)
            {
                FoxProDbFolderFoxProSkl58 = foxProDbFolderFoxProSkl58;
                Properties.Settings.Default.FoxProDbFolder_FoxPro_Skl58 = foxProDbFolderFoxProSkl58;
            }

            var isRunningFullscreen = Convert.ToBoolean(_parametrSave[7]);
            if (IsRunningFullscreen != isRunningFullscreen)
            {
                IsRunningFullscreen = isRunningFullscreen;
                Properties.Settings.Default.IsRunInFullscreen = isRunningFullscreen;
            }
            var fontSize = Convert.ToInt32(_parametrSave[8]);
            if (FontSize != fontSize)
            {
                FontSize = fontSize;
                Properties.Settings.Default.FontSize = fontSize;
            }

            Properties.Settings.Default.Save();
            CloseCommand.Execute(obj);
        }
        #endregion
        #region Отмена

        private RelayCommand _closeCommand;

        public ICommand CloseCommand
        {
            get { return _closeCommand ?? (_closeCommand = new RelayCommand(o => Close())); }
        }

        private void Close()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is UserConfigWindow)
                {
                    window.Close();
                    break;
                }
            }
        }
        #endregion
        #endregion

        public UserConfigViewModel()
        {
            FoxProDbFolderComposition = Properties.Settings.Default.FoxProDbFolder_Composition;
            FoxProDbFolderFox60ArmLimit = Properties.Settings.Default.FoxProDbFolder_Fox60_Arm_Limit;      
            FoxProDbFolderTempWork = Properties.Settings.Default.FoxProDbFolder_Temp_Work;
            FoxProDbFolderTemp = Properties.Settings.Default.FoxProDbFolder_Temp;
            FoxProDbFolderFox60ArmBase = Properties.Settings.Default.FoxProDbFolder_Fox60_Arm_Base;
            FoxProDbFolderFoxProSkl = Properties.Settings.Default.FoxProDbFolder_FoxPro_Skl;
            FoxProDbFolderFoxProSkl58 = Properties.Settings.Default.FoxProDbFolder_FoxPro_Skl58;            
            FontSize = Properties.Settings.Default.FontSize;
            IsRunningFullscreen = Properties.Settings.Default.IsRunInFullscreen;
        }
    }
}
