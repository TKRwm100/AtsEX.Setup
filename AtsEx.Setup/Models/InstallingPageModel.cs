﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Reactive.Bindings;
using Reactive.Bindings.Disposables;
using Reactive.Bindings.Extensions;

using AtsEx.Setup.Installing;

namespace AtsEx.Setup.Models
{
    internal class InstallingPageModel : INotifyPropertyChanged, IDisposable
    {
        private readonly CompositeDisposable Disposables = new CompositeDisposable();

        public event PropertyChangedEventHandler PropertyChanged;

        public InstallingPageModel()
        {
        }

        public async Task InstallAsync(IProgress<InstallationState> stateReporter)
        {
            try
            {
                try
                {
                    using (Installer installer = new Installer(stateReporter))
                    {
                        await Task.Run(() =>
                        {
                            if (TargetPath.CopyBve.Value)
                            {
                                if (TargetPath.Bve6Path.Value is null)
                                {
                                    string newBve5Path = installer.CopyBve(TargetPath.Bve5Path.Value, 5);
                                    TargetPath.Bve5Path.Value = newBve5Path;
                                }
                                else
                                {
                                    string newBve6Path = installer.CopyBve(TargetPath.Bve6Path.Value, 6);
                                    TargetPath.Bve6Path.Value = newBve6Path;
                                }

                                TargetPath.CopyBve.Value = false;
                            }

                            installer.Install();
                        });
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    Navigator.Instance.Page.Value = Page.RequiresElevation;
                    return;
                }
                catch (IOException ex)
                {
                    switch (ex.HResult)
                    {
                        case -2147024864: // 0x80070020
                            Navigator.Instance.Abort($"{ex.Message}\nPC を再起動の上もう一度お試しください。");
                            return;
                        default:
                            throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Navigator.Instance.Abort(ex.ToString());
                return;
            }

            Navigator.Instance.Page.Value = Page.Completed;
        }

        public void Abort()
        {
            if (MessageBox.Show("本当にインストールを中止してもよろしいですか?", "AtsEX セットアップウィザード", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Navigator.Instance.Page.Value = Page.Welcome;
            }
        }

        public void Dispose()
        {
            Disposables.Dispose();
        }
    }
}
