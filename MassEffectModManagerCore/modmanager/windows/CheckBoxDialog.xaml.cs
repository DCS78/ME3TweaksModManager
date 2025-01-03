﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LegendaryExplorerCore.Misc;
using PropertyChanged;
using CheckBoxSelectionPair = MassEffectModManagerCore.ui.CheckBoxSelectionPair;

namespace MassEffectModManagerCore.modmanager.windows
{
    /// <summary>
    /// Interaction logic for CheckBoxDialog.xaml
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public partial class CheckBoxDialog : Window
    {

        public ObservableCollectionExtended<CheckBoxSelectionPair> Items { get; } = new();
        public CheckBoxDialog(Window owningWindow, string message, string caption, object[] options, object[] preselectedOptions = null, object[] disabledOptions = null, int requestedWidth = 400, int requestedHeight = 250)
        {
            Owner = owningWindow;
            Message = message;
            Caption = caption;

            RequestedWidth = requestedWidth;
            RequestedHeight = requestedHeight;

            foreach (var o in options)
            {
                CheckBoxSelectionPair c =
                    new CheckBoxSelectionPair(o, preselectedOptions != null && preselectedOptions.Contains(o), null)
                    {
                        IsEnabled = disabledOptions == null || !disabledOptions.Contains(o)
                    };
                Items.Add(c);
            }


            InitializeComponent();
        }

        public int RequestedHeight { get; set; }

        public int RequestedWidth { get; set; }

        public string Message { get; }
        public string Caption { get; }

        public object[] GetSelectedItems()
        {
            return Items.Where(x => x.IsChecked).Select(x => x.Item).ToArray();
        }


        private void CloseDialog(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
