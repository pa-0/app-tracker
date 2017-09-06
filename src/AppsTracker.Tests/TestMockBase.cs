﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using AppsTracker.Common.Communication;
using AppsTracker.Communication;
using AppsTracker.Controllers;
using AppsTracker.Data.Repository;
using AppsTracker.Domain;
using AppsTracker.Domain.Apps;
using AppsTracker.Domain.Logs;
using AppsTracker.Domain.Screenshots;
using AppsTracker.Domain.Settings;
using AppsTracker.Domain.Tracking;
using AppsTracker.Domain.Usages;
using AppsTracker.Domain.Windows;
using AppsTracker.Service;
using AppsTracker.Service.Web;
using AppsTracker.Tests.Fakes;
using AppsTracker.Tracking;
using AppsTracker.Tracking.Helpers;
using AppsTracker.Tracking.Limits;
using AppsTracker.ViewModels;
using Moq;

namespace AppsTracker.Tests
{
    public abstract class TestMockBase
    {
        protected readonly Mock<IRepository> repository = new Mock<IRepository>();
        protected readonly Mock<ITrackingService> trackingService = new Mock<ITrackingService>();
        protected readonly Mock<IAppSettingsService> settingsService = new Mock<IAppSettingsService>();
        protected readonly Mock<IUserSettingsService> userSettingsService = new Mock<IUserSettingsService>();
        protected readonly Mock<IWindowService> windowService = new Mock<IWindowService>();
        protected readonly Mock<IScreenshotService> screenshotService = new Mock<IScreenshotService>();
        protected readonly Mock<IReleaseNotesService> releaseNotesService = new Mock<IReleaseNotesService>();
        protected readonly Mock<IAppChangedNotifier> windowChangedNotifier = new Mock<IAppChangedNotifier>();
        protected readonly Mock<ILimitHandler> limitHandler = new Mock<ILimitHandler>();
        protected readonly Mock<IMidnightNotifier> midnightNotifier = new Mock<IMidnightNotifier>();
        protected readonly Mock<IScreenshotFactory> screenshotFactory = new Mock<IScreenshotFactory>();
        protected readonly Mock<IAppChangedNotifier> appChangedNotifier = new Mock<IAppChangedNotifier>();
        protected readonly Mock<IScreenshotTracker> screenshotTracker = new Mock<IScreenshotTracker>();
        protected readonly Mock<IAppearanceController> appearanceController = new Mock<IAppearanceController>();
        protected readonly Mock<ITrackingController> trackingController = new Mock<ITrackingController>();
        protected readonly Mock<IAppDurationCalc> appDurationCalc = new Mock<IAppDurationCalc>();

        protected readonly Mock<IUseCaseAsync<AppModel>> appStatsUseCaseAsync = new Mock<IUseCaseAsync<AppModel>>();
        protected readonly Mock<IUseCase<String, Int32, AppSummary>> appSummaryStatsUseCase = new Mock<IUseCase<string, int, AppSummary>>();
        protected readonly Mock<IUseCase<String, IEnumerable<DateTime>, WindowSummary>> windowsStatsSummaryUseCase = new Mock<IUseCase<string, IEnumerable<DateTime>, WindowSummary>>();
        protected readonly Mock<IUseCase<String, IEnumerable<String>, IEnumerable<DateTime>, WindowDurationOverview>> windowDurationUseCase = new Mock<IUseCase<string, IEnumerable<string>, IEnumerable<DateTime>, WindowDurationOverview>>();
        protected readonly Mock<IUseCase<DateTime, LogSummary>> logSummaryUseCase = new Mock<IUseCase<DateTime, LogSummary>>();
        protected readonly Mock<IUseCase<UserLoggedTime>> userLoggedTimeUseCase = new Mock<IUseCase<UserLoggedTime>>();
        protected readonly Mock<IUseCase<String, UsageOverview>> usageOverViewUseCase = new Mock<IUseCase<string, UsageOverview>>();
        protected readonly Mock<IUseCase<AppDuration>> appDurationUseCase = new Mock<IUseCase<AppDuration>>();
        protected readonly Mock<IUseCase<String, DailyAppDuration>> dailyAppDurationUseCase = new Mock<IUseCase<string, DailyAppDuration>>();
        protected readonly Mock<IUseCase<CategoryDuration>> categoryDurationUseCase = new Mock<IUseCase<CategoryDuration>>();
        protected readonly Mock<IUseCase<String, DailyCategoryDuration>> dailyCategoryDurationUseCase = new Mock<IUseCase<string, DailyCategoryDuration>>();
        protected readonly Mock<IUseCase<ScreenshotOverview>> screenshotModelUseCase = new Mock<IUseCase<ScreenshotOverview>>();
        protected readonly Mock<IUseCase<String, DailyScreenshotModel>> dailyScreenshotModelUseCase = new Mock<IUseCase<string, DailyScreenshotModel>>();

        protected readonly Mock<ScreenshotStore> screenshotStore = new Mock<ScreenshotStore>();
        protected readonly Mediator mediator = new Mediator();
        protected readonly ISyncContext syncContext = new SyncContextMock();

        protected ExportFactory<AppDetailsViewModel> GetAppDetailsVMFactory()
        {
            var tupleFactory =
                new Func<Tuple<AppDetailsViewModel, Action>>(
                    () => new Tuple<AppDetailsViewModel, Action>(
                        new AppDetailsViewModel(appStatsUseCaseAsync.Object,
                            appSummaryStatsUseCase.Object,
                            windowsStatsSummaryUseCase.Object,
                            windowDurationUseCase.Object,
                            mediator),
                            ExportFactoryContextRelease));
            return new ExportFactory<AppDetailsViewModel>(tupleFactory);
        }

        protected ExportFactory<ScreenshotsViewModel> GetScreenshotsVMFactory()
        {
            var tupleFactory =
                new Func<Tuple<ScreenshotsViewModel, Action>>(
                    () => new Tuple<ScreenshotsViewModel, Action>(
                        new ScreenshotsViewModel(
                            settingsService.Object,
                            screenshotService.Object,
                            windowService.Object,
                            mediator,
                            screenshotStore.Object),
                            ExportFactoryContextRelease));

            return new ExportFactory<ScreenshotsViewModel>(tupleFactory);
        }

        protected ExportFactory<DaySummaryViewModel> GetDaySummaryVMFactory()
        {
            var tupleFactory =
                new Func<Tuple<DaySummaryViewModel, Action>>(
                    () => new Tuple<DaySummaryViewModel, Action>(
                        new DaySummaryViewModel(new Mock<IUseCase<DateTime, LogSummary>>().Object,
                        new Mock<IUseCase<DateTime, AppSummary>>().Object,
                        new Mock<IUseCase<String, DateTime, WindowSummary>>().Object,
                        null,
                        new Mock<IUseCase<DateTime, CategoryDuration>>().Object,
                            mediator),
                            ExportFactoryContextRelease));

            return new ExportFactory<DaySummaryViewModel>(tupleFactory);
        }

        protected ExportFactory<DataHostViewModel> GetDataHostVMFactory()
        {
            var appDetailsVMFactory = GetAppDetailsVMFactory();
            var screenshotsVMFactory = GetScreenshotsVMFactory();
            var daySummaryVMFactory = GetDaySummaryVMFactory();
            var tupleFactory =
                new Func<Tuple<DataHostViewModel, Action>>(
                () => new Tuple<DataHostViewModel, Action>(
                        new DataHostViewModel(appDetailsVMFactory,
                            screenshotsVMFactory,
                            daySummaryVMFactory),
                            ExportFactoryContextRelease));

            return new ExportFactory<DataHostViewModel>(tupleFactory);
        }

        protected ExportFactory<UserStatsViewModel> GetUserStatsVMFactory()
        {
            var tupleFactory =
                new Func<Tuple<UserStatsViewModel, Action>>(
                    () => new Tuple<UserStatsViewModel, Action>(
                        new UserStatsViewModel(
                            userLoggedTimeUseCase.Object,
                            usageOverViewUseCase.Object,
                            mediator),
                            ExportFactoryContextRelease));

            return new ExportFactory<UserStatsViewModel>(tupleFactory);
        }

        protected ExportFactory<AppStatsViewModel> GetAppStatsVMFactory()
        {
            var tupleFactory =
                new Func<Tuple<AppStatsViewModel, Action>>(
                    () => new Tuple<AppStatsViewModel, Action>(
                        new AppStatsViewModel(
                            appDurationUseCase.Object,
                            dailyAppDurationUseCase.Object,
                            mediator),
                            ExportFactoryContextRelease));

            return new ExportFactory<AppStatsViewModel>(tupleFactory);
        }

        protected ExportFactory<DailyAppUsageViewModel> GetDailyAppUsageVMFactory()
        {
            var tupleFactory =
                new Func<Tuple<DailyAppUsageViewModel, Action>>(
                    () => new Tuple<DailyAppUsageViewModel, Action>(
                        new DailyAppUsageViewModel(
                            new Mock<IUseCase<AppDurationOverview>>().Object,
                            mediator),
                            ExportFactoryContextRelease));

            return new ExportFactory<DailyAppUsageViewModel>(tupleFactory);
        }


        protected ExportFactory<ScreenshotsStatsViewModel> GetScreenshotStatsVMFactory()
        {
            var tupleFactory =
                new Func<Tuple<ScreenshotsStatsViewModel, Action>>(
                    () => new Tuple<ScreenshotsStatsViewModel, Action>(
                        new ScreenshotsStatsViewModel(
                            screenshotModelUseCase.Object,
                            dailyScreenshotModelUseCase.Object,
                            mediator),
                            ExportFactoryContextRelease));

            return new ExportFactory<ScreenshotsStatsViewModel>(tupleFactory);
        }

        protected ExportFactory<CategoryStatsViewModel> GetCategoryStatsVMFactory()
        {
            var tupleFactory =
                new Func<Tuple<CategoryStatsViewModel, Action>>(
                    () => new Tuple<CategoryStatsViewModel, Action>(
                        new CategoryStatsViewModel(
                            categoryDurationUseCase.Object,
                            dailyCategoryDurationUseCase.Object,
                            mediator),
                            ExportFactoryContextRelease));

            return new ExportFactory<CategoryStatsViewModel>(tupleFactory);
        }


        protected ExportFactory<StatisticsHostViewModel> GetStatisticsHostVMFactory()
        {
            var userStatsVMFactory = GetUserStatsVMFactory();
            var appStatsVMFactory = GetAppStatsVMFactory();
            var dailyAppUsageVMFactory = GetDailyAppUsageVMFactory();
            var screenshotStatsVMFactory = GetScreenshotStatsVMFactory();
            var categoryStatsVMFactory = GetCategoryStatsVMFactory();
            var tupleFactory =
                new Func<Tuple<StatisticsHostViewModel, Action>>(
                    () => new Tuple<StatisticsHostViewModel, Action>(
                        new StatisticsHostViewModel(userStatsVMFactory,
                            appStatsVMFactory,
                            dailyAppUsageVMFactory,
                            screenshotStatsVMFactory,
                            categoryStatsVMFactory),
                            ExportFactoryContextRelease));

            return new ExportFactory<StatisticsHostViewModel>(tupleFactory);
        }

        protected ExportFactory<SettingsGeneralViewModel> GetSettingsGeneralVMFactory()
        {
            var tupleFactory =
                new Func<Tuple<SettingsGeneralViewModel, Action>>(
                    () => new Tuple<SettingsGeneralViewModel, Action>(
                        new SettingsGeneralViewModel(windowService.Object,
                            settingsService.Object,
                            mediator),
                            ExportFactoryContextRelease));

            return new ExportFactory<SettingsGeneralViewModel>(tupleFactory);
        }


        protected ExportFactory<SettingsTrackingViewModel> GetSettingsLoggingVMFactory()
        {
            var tupleFactory =
                new Func<Tuple<SettingsTrackingViewModel, Action>>(
                    () => new Tuple<SettingsTrackingViewModel, Action>(
                        new SettingsTrackingViewModel(settingsService.Object, mediator),
                            ExportFactoryContextRelease));

            return new ExportFactory<SettingsTrackingViewModel>(tupleFactory);
        }

        protected ExportFactory<SettingsScreenshotsViewModel> GetSettingsScreenshotsVMFactory()
        {
            var tupleFactory =
                new Func<Tuple<SettingsScreenshotsViewModel, Action>>(
                    () => new Tuple<SettingsScreenshotsViewModel, Action>(
                        new SettingsScreenshotsViewModel(settingsService.Object,
                            userSettingsService.Object,
                            trackingService.Object,
                            repository.Object,
                            windowService.Object,
                            mediator),
                            ExportFactoryContextRelease));

            return new ExportFactory<SettingsScreenshotsViewModel>(tupleFactory);
        }


        protected ExportFactory<SettingsPasswordViewModel> GetSettingsPasswordVMFactory()
        {
            var tupleFactory =
                new Func<Tuple<SettingsPasswordViewModel, Action>>(
                    () => new Tuple<SettingsPasswordViewModel, Action>(
                        new SettingsPasswordViewModel(windowService.Object,
                            settingsService.Object,
                            mediator),
                            ExportFactoryContextRelease));

            return new ExportFactory<SettingsPasswordViewModel>(tupleFactory);
        }


        protected ExportFactory<SettingsAppCategoriesViewModel> GetSettingsAppCategoriesVMFactory()
        {
            var tupleFactory =
                new Func<Tuple<SettingsAppCategoriesViewModel, Action>>(
                    () => new Tuple<SettingsAppCategoriesViewModel, Action>(
                        new SettingsAppCategoriesViewModel(null,
                            mediator),
                            ExportFactoryContextRelease));

            return new ExportFactory<SettingsAppCategoriesViewModel>(tupleFactory);
        }


        protected ExportFactory<SettingsLimitsViewModel> GetSettingsLimitsVMFactory()
        {
            var tupleFactory =
                new Func<Tuple<SettingsLimitsViewModel, Action>>(
                    () => new Tuple<SettingsLimitsViewModel, Action>(
                        new SettingsLimitsViewModel(new Mock<AppLimitsCoordinator>().Object,
                            mediator),
                            ExportFactoryContextRelease));

            return new ExportFactory<SettingsLimitsViewModel>(tupleFactory);
        }

        protected ExportFactory<SettingsHostViewModel> GetSettingsHostVMFactory()
        {
            var settingsGeneralVMFactory = GetSettingsGeneralVMFactory();
            var settingsLoggingVMFactory = GetSettingsLoggingVMFactory();
            var settingsScreenshotsVMFactory = GetSettingsScreenshotsVMFactory();
            var settingsPasswordVMFactory = GetSettingsPasswordVMFactory();
            var settingsCategoriesVMFactory = GetSettingsAppCategoriesVMFactory();
            var settingsLimitsVMFactory = GetSettingsLimitsVMFactory();

            var tupleFactory = new Func<Tuple<SettingsHostViewModel, Action>>(
                () => new Tuple<SettingsHostViewModel, Action>(
                    new SettingsHostViewModel(settingsGeneralVMFactory,
                        settingsLoggingVMFactory,
                        settingsScreenshotsVMFactory,
                        settingsPasswordVMFactory,
                        settingsCategoriesVMFactory,
                        settingsLimitsVMFactory),
                        ExportFactoryContextRelease));
            return new ExportFactory<SettingsHostViewModel>(tupleFactory);
        }

        protected void ExportFactoryContextRelease()
        {

        }
    }
}
