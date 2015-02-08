﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AppsTracker.Controllers;
using AppsTracker.DAL.Service;
using AppsTracker.Models.EntityModels;

namespace AppsTracker.MVVM
{
    internal abstract class SettingsBaseViewModel : ViewModelBase
    {
        private ISettingsService _settingsService;

        private Setting _settings;
        public Setting Settings
        {
            get
            {
                return _settings;
            }
            set
            {
                _settings = value;
                PropertyChanging("Settings");
            }
        }

        private ICommand _saveChangesCommand;
        public ICommand SaveChangesCommand
        {
            get
            {
                return _saveChangesCommand ?? (_saveChangesCommand = new DelegateCommand(SaveChanges));
            }
        }
        public SettingsBaseViewModel()
        {
            _settingsService = ServiceFactory.Get<ISettingsService>();
            Settings = _settingsService.Settings;
        }

        private void SaveChanges()
        {
            _settingsService.SaveChanges(_settings);
        }

        protected void SettingsChanging()
        {
            PropertyChanging("Settings");
        }
    }
}
