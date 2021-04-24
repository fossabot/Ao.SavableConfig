﻿using Ao.SavableConfig;
using Ao.SavableConfig.Binder;
using Ao.SavableConfig.Saver;
using System;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Microsoft.Extensions.Configuration
{
    public static class BinderExtensions
    {
        public static IDisposable BindTwoWay(this IConfigurationChangeNotifyable notifyable, object value,params IChangeTransferCondition[] changeTransferConditions)
        {
            var setting = new BindSettings(value, BindSettings.DefaultDelayTime, changeTransferConditions);
            return BindTwoWay(notifyable,setting);
        }
        public static IDisposable BindTwoWay(this IConfigurationChangeNotifyable notifyable, BindSettings bindSettings)
        {
            var updater = bindSettings.Updater;
            if (updater is null)
            {
                updater = a => a();
            }
            var once = new ConcurrentOnce();
            var watcher = notifyable.CreateWatcher();
            notifyable.GetReloadToken()
                .RegisterChangeCallback(Reload, null);
            notifyable.Bind(bindSettings.Value);
            void Reload(object state)
            {
                updater(() => notifyable.Bind(bindSettings.Value));
                notifyable.GetReloadToken()
                    .RegisterChangeCallback(Reload, null);
            }
            async void handler(object o, IConfigurationChangeInfo e)
            {
                if (await once.WaitAsync(bindSettings.DelayTime))
                {
                    var infos = watcher.ChangeInfos;
                    watcher.Clear();
                    var repo = ChangeReport.FromChanges(notifyable, infos);
                    var saver = new ChangeSaver(repo, bindSettings.Conditions);
                    var res = saver.EmitAndSave();
                    updater(() => notifyable.Bind(bindSettings.Value));
                }
            }
            watcher.ChangePushed += handler;
            return new BindToken
            {
                Disposed = () =>
                {
                    watcher.ChangePushed -= handler;
                    watcher.Dispose();
                }
            };
        }
    }
}
