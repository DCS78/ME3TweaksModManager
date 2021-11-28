﻿using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LegendaryExplorerCore.Gammtek.Extensions.Collections.Generic;
using LegendaryExplorerCore.Misc;
using ME3TweaksModManager.modmanager.localizations;
using ME3TweaksModManager.modmanager.nexusmodsintegration;
using ME3TweaksModManager.modmanager.objects;
using ME3TweaksModManager.ui;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace ME3TweaksModManager.modmanager.usercontrols
{
    /// <summary>
    /// Interaction logic for LogUploader.xaml
    /// </summary>
    public partial class NXMHandlerConfigPanel : MMBusyPanelBase
    {
        public NXMHandlerConfigPanel()
        {
            DataContext = this;
            LoadCommands();
        }

        public ICommand AddOtherAppCommand { get; set; }
        public ICommand CloseCommand { get; set; }
        public ICommand RegisterCommand { get; set; }
        public ICommand RemoveAppCommand { get; set; }

        private void LoadCommands()
        {
            RemoveAppCommand = new RelayCommand(RemoveApp);
            RegisterCommand = new GenericCommand(RegisterM3);
            AddOtherAppCommand = new GenericCommand(AddNXMApp, CanAddNXMApp);
            CloseCommand = new GenericCommand(() => OnClosing(DataEventArgs.Empty), CanClose);
        }

        private void RegisterM3()
        {
            NexusModsUtilities.SetupNXMHandling();
            M3L.ShowDialog(window, "Set ME3Tweaks Mod Manager to handle nxm:// links. You can forward these links to other applications by adding other applications.", "Configured nxm:// handling", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RemoveApp(object obj)
        {
            if (obj is NexusDomainHandler ndh)
            {
                OtherGameHandlers.Remove(ndh);
            }
        }

        private bool CanClose()
        {
            if (ValidateAll())
            {
                return true;
            }

            return false;
        }

        private void AddNXMApp()
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = $@"{M3L.GetString(M3L.string_executables)}|*.exe",
                CheckFileExists = true,
            };
            var result = ofd.ShowDialog();
            if (result.HasValue && result.Value)
            {
                OtherGameHandlers.Add(new NexusDomainHandler() { ProgramPath = ofd.FileName, Arguments = GetDefaultArgumentsForApp(Path.GetFileNameWithoutExtension(ofd.FileName))});
            }
        }

        private string GetDefaultArgumentsForApp(string appExeName)
        {
            switch (appExeName.ToLower())
            {
                case @"vortex":
                    return @"-d %1";
                case @"kortex x64":
                case @"kortex":
                    return "-DownloadLink \"%1\""; // do not localize
                case @"nexusclient":
                default:
                    return @"%1";
            }
        }

        private bool CanAddNXMApp()
        {
            return ValidateAll();
        }

        private bool ValidateAll()
        {
            foreach (var d in OtherGameHandlers)
            {
                if (string.IsNullOrWhiteSpace(d.ProgramPath))
                    return false; // Can't be empty
                if (!File.Exists(d.ProgramPath))
                    return false;
                if (!d.Arguments.Contains(@"%1"))
                    return false;
                if (string.IsNullOrWhiteSpace(d.DomainsEditable))
                    return false;
            }

            return true;
        }


        public ObservableCollectionExtended<NexusDomainHandler> OtherGameHandlers { get; } = new();

        public override void HandleKeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && ValidateAll())
            {
                e.Handled = true;
                OnClosing(DataEventArgs.Empty);
            }
        }

        protected override void OnClosing(DataEventArgs args)
        {
            if (ValidateAll())
            {
                // Save
                SaveAndCommit();
            }
            base.OnClosing(args);
        }

        private void SaveAndCommit()
        {
            foreach (var v in OtherGameHandlers)
            {
                v.Domains = v.DomainsEditable.Split(',').ToList();
            }
            App.NexusDomainHandlers.ReplaceAll(OtherGameHandlers);
            File.WriteAllText(M3Utilities.GetExternalNexusHandlersFile(), JsonConvert.SerializeObject(OtherGameHandlers));
        }

        public override void OnPanelVisible()
        {
            InitializeComponent();
            OtherGameHandlers.AddRange(App.NexusDomainHandlers);
            foreach (var handler in OtherGameHandlers)
            {
                handler.LoadEditable();
            }
        }
    }
}
