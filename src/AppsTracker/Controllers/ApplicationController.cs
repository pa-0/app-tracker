﻿#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using AppsTracker.Data.Service;
using AppsTracker.Widgets;
using Microsoft.Win32;
using AppsTracker.Service;

namespace AppsTracker.Controllers
{
    [Export(typeof(IApplicationController))]
    internal sealed class ApplicationController : IApplicationController
    {
        private readonly IAppearanceController appearanceController;
        private readonly ITrackingController trackingController;
        private readonly IXmlSettingsService xmlSettingsService;
        private readonly ISqlSettingsService sqlSettingsService;
        private readonly IDataService dataService;
        private readonly IWindowService windowService;

        [ImportingConstructor]
        public ApplicationController(IAppearanceController appearanceController, 
                                     ITrackingController trackingController,
                                     ISqlSettingsService sqlSettingsService, 
                                     IXmlSettingsService xmlSettingsService,
                                     IDataService dataService, 
                                     IWindowService windowService)
        {
            this.appearanceController = appearanceController;
            this.trackingController = trackingController;
            this.xmlSettingsService = xmlSettingsService;
            this.sqlSettingsService = sqlSettingsService;
            this.dataService = dataService;
            this.windowService = windowService;
        }

        public void Initialize(bool autoStart)
        {
            xmlSettingsService.Initialize();
            PropertyChangedEventManager.AddHandler(sqlSettingsService, OnSettingsChanged, "Settings");

            appearanceController.Initialize(sqlSettingsService.Settings);
            trackingController.Initialize(sqlSettingsService.Settings);

            if (autoStart == false)
            {
                windowService.CreateOrShowMainWindow();
                windowService.FirstRunWindowSetup();
            }

            dataService.DbSizeCritical += OnDbSizeCritical;
            dataService.GetDBSize();

            EntryPoint.SingleInstanceManager.SecondInstanceActivating += (s, e) => windowService.CreateOrShowMainWindow();
        }

        private void OnSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            trackingController.SettingsChanging(sqlSettingsService.Settings);
            appearanceController.SettingsChanging(sqlSettingsService.Settings);
        }

        private async void OnDbSizeCritical(object sender, EventArgs e)
        {
            var settings = sqlSettingsService.Settings;
            settings.TakeScreenshots = false;
            await sqlSettingsService.SaveChangesAsync(settings);

            windowService.ShowMessageDialog("Database size has reached the maximum allowed value"
                + Environment.NewLine + "Please run the screenshot cleaner from the settings menu to continue capturing screenshots.", false);

            dataService.DbSizeCritical -= OnDbSizeCritical;
        }

        public void ShutDown()
        {
            windowService.Shutdown();
            xmlSettingsService.ShutDown();
            trackingController.Dispose();
        }
    }
}
