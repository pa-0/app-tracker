﻿#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    internal class AppDetailsViewModel : ViewModelBase, ICommunicator
    {
        private readonly IDataService dataService;
        private readonly IStatsService statsService;


        public override string Title
        {
            get { return "APPS"; }
        }


        private bool isChartVisible;

        public bool IsChartVisible
        {
            get { return isChartVisible; }
            set { SetPropertyValue(ref isChartVisible, value); }
        }


        private string selectedAppsDuration;

        public string SelectedAppsDuration
        {
            get { return selectedAppsDuration; }
            set { SetPropertyValue(ref selectedAppsDuration, value); }
        }


        private string selectedWindowsDuration;

        public string SelectedWindowsDuration
        {
            get { return selectedWindowsDuration; }
            set { SetPropertyValue(ref selectedWindowsDuration, value); }
        }


        private readonly AsyncProperty<IEnumerable<Aplication>> appList;

        public AsyncProperty<IEnumerable<Aplication>> AppList
        {
            get { return appList; }
        }


        private readonly AsyncProperty<IEnumerable<AppSummary>> appSummaryList;

        public AsyncProperty<IEnumerable<AppSummary>> AppSummaryList
        {
            get { return appSummaryList; }
        }


        private readonly AsyncProperty<IEnumerable<WindowSummary>> windowSummaryList;

        public AsyncProperty<IEnumerable<WindowSummary>> WindowSummaryList
        {
            get { return windowSummaryList; }
        }


        private readonly AsyncProperty<IEnumerable<WindowDurationOverview>> windowDurationList;

        public AsyncProperty<IEnumerable<WindowDurationOverview>> WindowDurationList
        {
            get { return windowDurationList; }
        }


        private AppSummary selectedAppSummary;

        public AppSummary SelectedAppSummary
        {
            get
            {
                return selectedAppSummary;
            }
            set
            {
                SetPropertyValue(ref selectedAppSummary, value);
                SelectedWindowsDuration = string.Empty;
            }
        }


        private Aplication selectedApp;

        public Aplication SelectedApp
        {
            get { return selectedApp; }
            set
            {
                SetPropertyValue(ref selectedApp, value);
                IsChartVisible = false;
                if (value != null)
                {
                    appSummaryList.Reload();
                }
            }
        }


        private ICommand selectedAppsChangingCommand;

        public ICommand SelectedAppsChangingCommand
        {
            get { return selectedAppsChangingCommand ?? (selectedAppsChangingCommand = new DelegateCommand(OverallAppSelectionChanged)); }
        }


        private ICommand selectedWindowsChangingCommand;

        public ICommand SelectedWindowsChangingCommand
        {
            get { return selectedWindowsChangingCommand ?? (selectedWindowsChangingCommand = new DelegateCommand(OverallWindowSelectionChanged)); }
        }


        public IMediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }


        public AppDetailsViewModel()
        {
            dataService = serviceResolver.Resolve<IDataService>();
            statsService = serviceResolver.Resolve<IStatsService>();

            appList = new AsyncProperty<IEnumerable<Aplication>>(GetApps, this);
            appSummaryList = new AsyncProperty<IEnumerable<AppSummary>>(GetAppSummary, this);
            windowSummaryList = new AsyncProperty<IEnumerable<WindowSummary>>(GetWindowSummary, this);
            windowDurationList = new AsyncProperty<IEnumerable<WindowDurationOverview>>(GetWindowDuration, this);

            Mediator.Register(MediatorMessages.ApplicationAdded, new Action<Aplication>(ApplicationAdded));
            Mediator.Register(MediatorMessages.RefreshLogs, new Action(appList.Reload));
        }


        private IEnumerable<Aplication> GetApps()
        {
            return dataService.GetFiltered<Aplication>(a => a.User.UserID == Globals.SelectedUserID
                                                                && a.Windows.SelectMany(w => w.Logs).Where(l => l.DateCreated >= Globals.DateFrom).Any()
                                                                && a.Windows.SelectMany(w => w.Logs).Where(l => l.DateCreated <= Globals.DateTo).Any())
                                                           .ToList()
                                                           .Distinct();
        }


        private IEnumerable<AppSummary> GetAppSummary()
        {
            var app = SelectedApp;
            if (app == null)
                return null;

            return statsService.GetAppSummary(Globals.SelectedUserID, app.ApplicationID, app.Name, Globals.DateFrom, Globals.DateTo);
        }


        private IEnumerable<WindowSummary> GetWindowSummary()
        {
            var selectedApp = SelectedAppSummary;
            if (AppSummaryList.Result == null || selectedApp == null)
                return null;

            var selectedDates = AppSummaryList.Result.Where(t => t.IsSelected).Select(t => t.DateTime);
            return statsService.GetWindowsSummary(Globals.SelectedUserID, selectedApp.AppName, selectedDates);
        }


        private IEnumerable<WindowDurationOverview> GetWindowDuration()
        {
            var selectedApp = SelectedAppSummary;

            if (AppSummaryList.Result == null || selectedApp == null || WindowSummaryList.Result == null)
                return null;

            var selectedWindows = WindowSummaryList.Result.Where(w => w.IsSelected).Select(w => w.Title).ToList();
            var selectedDates = AppSummaryList.Result.Where(t => t.IsSelected).Select(t => t.DateTime);

            return statsService.GetWindowDurationOverview(Globals.SelectedUserID, selectedApp.AppName, selectedWindows, selectedDates);
        }


        private void ApplicationAdded(Aplication app)
        {
            appList.Reload();

            windowDurationList.Reset();
            appSummaryList.Reset();
            windowSummaryList.Reset();
        }


        private void OverallAppSelectionChanged()
        {
            IsChartVisible = false;

            if (AppSummaryList.Result == null)
                return;

            long ticks = 0;
            var selectedApps = AppSummaryList.Result.Where(a => a.IsSelected);

            if (selectedApps.Count() == 0)
            {
                windowSummaryList.Reset();
                return;
            }

            foreach (var app in selectedApps)
                ticks += app.Duration;

            if (ticks == 0)
                return;

            TimeSpan timeSpan = new TimeSpan(ticks);
            SelectedAppsDuration = string.Format("Selected: {0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            windowSummaryList.Reload();
        }


        private void OverallWindowSelectionChanged()
        {
            if (windowSummaryList.Result == null)
            {
                WindowDurationList.Reset();
                IsChartVisible = false;
                return;
            }

            if (windowSummaryList.Result.Where(w => w.IsSelected).Count() == 0)
            {
                IsChartVisible = false;
                return;
            }

            IsChartVisible = true;
            long ticks = 0;

            foreach (var window in windowSummaryList.Result.Where(w => w.IsSelected))
                ticks += window.Duration;

            if (ticks == 0)
                return;

            TimeSpan timeSpan = new TimeSpan(ticks);
            SelectedWindowsDuration = string.Format("Selected: {0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);

            windowDurationList.Reload();
        }
    }
}
