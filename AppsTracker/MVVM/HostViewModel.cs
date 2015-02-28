﻿#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.Windows.Input;
using AppsTracker.Common.Utils;

namespace AppsTracker.MVVM
{
    internal abstract class HostViewModel : ViewModelBase
    {
        private Dictionary<Type, ViewModelResolver> _childrenMap = new Dictionary<Type, ViewModelResolver>();

        protected ViewModelBase _selectedChild;

        protected ICommand _changePageCommand;

        public ViewModelBase SelectedChild
        {
            get
            {
                return _selectedChild;
            }
            set
            {
                if (_selectedChild != null && _selectedChild.Title == value.Title)
                    return;

                _selectedChild = value;
                PropertyChanging("SelectedChild");
            }
        }

        public virtual ICommand ChangePageCommand
        {
            get
            {
                return _changePageCommand ?? (_changePageCommand = new DelegateCommand(ChangePage));
            }
        }

        protected virtual void ChangePage(object parameter)
        {
            SelectedChild = GetChild((Type)parameter);
        }

        protected void RegisterChild<T>(Func<T> getter) where T : ViewModelBase
        {
            if (_childrenMap.ContainsKey(typeof(T)))
                return;
            Ensure.NotNull(getter);

            var resolver = new ViewModelResolver(getter);
            _childrenMap.Add(typeof(T), resolver);
        }

        protected ViewModelBase GetChild(Type type)
        {
            Ensure.NotNull(type);
            Ensure.Condition<InvalidOperationException>(_childrenMap.ContainsKey(type) == true, string.Format("Can't resolve {0} type!", type));

            var resolver = _childrenMap[type];
            ViewModelBase viewModel = null;
            resolver.Reference.TryGetTarget(out viewModel);
            if (viewModel == null)
            {
                viewModel = resolver.Getter();
                resolver.Reference.SetTarget(viewModel);
            }
            return viewModel;
        }

        private class ViewModelResolver
        {
            public WeakReference<ViewModelBase> Reference { get; private set; }
            public Func<ViewModelBase> Getter { get; private set; }

            public ViewModelResolver(Func<ViewModelBase> getter)
            {
                Reference = new WeakReference<ViewModelBase>(null);
                Getter = getter;
            }
        }
    }
}
