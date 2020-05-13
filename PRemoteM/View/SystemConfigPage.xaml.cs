﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using PRM.Core.DB;
using PRM.Core.Model;
using PRM.ViewModel;
using Shawn.Ulits;
using SQLite;
using Binding = System.Windows.Data.Binding;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using UserControl = System.Windows.Controls.UserControl;

namespace PRM.View
{
    /// <summary>
    /// SystemConfigPage.xaml 的交互逻辑
    /// </summary>
    public partial class SystemConfigPage : UserControl
    {
        public VmMain Host;
        public VmSystemConfigPage VmSystemConfigPage;
        public SystemConfigPage(VmMain host)
        {
            Host = host;
            VmSystemConfigPage = new VmSystemConfigPage(host);
            InitializeComponent();
            DataContext = VmSystemConfigPage;
        }

        private void ButtonSelectDb_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "Sqlite Database|*.db";
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    using (var db = new SQLiteConnection(dlg.FileName))
                    {
                    }
                    VmSystemConfigPage.SystemConfig.DataSecurity.DbPath = dlg.FileName;
                }
                catch (Exception ee)
                {
                    MessageBox.Show(SystemConfig.GetInstance().Language
                        .GetText("system_options_general_item_database_can_not_open"));
                }
            }
        }



        private void ButtonSelectRsaKey_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = $"private key|*{SystemConfigDataSecurity.PrivateKeyFileExt}",
                InitialDirectory = VmSystemConfigPage.SystemConfig.DataSecurity.RsaPrivateKeyPath,
            };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    // TODO 验证私钥正确性
                }
                catch (Exception ee)
                {
                    //MessageBox.Show(SystemConfig.GetInstance().Language
                    //    .GetText("system_options_general_item_database_can_not_open"));
                }
            }
        }


        private void TextBoxKey_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            var key = e.Key;
            // In case of an Alt modifier, e.Key returns Key.System and the real key is in e.SystemKey. 
            switch (key)
            {
                case Key.System:
                    key = e.SystemKey;
                    break;
                case Key.ImeProcessed:
                    key = e.ImeProcessedKey;
                    break;
                case Key.DeadCharProcessed:
                    key = e.DeadCharProcessedKey;
                    break;
            }
            if (
                key == Key.Tab ||
                key == Key.CapsLock ||
                key == Key.PrintScreen ||
                key == Key.Scroll ||
                key == Key.Sleep ||
                key == Key.Pause ||
                key == Key.LeftCtrl ||
                key == Key.RightCtrl ||
                key == Key.LeftAlt ||
                key == Key.RightAlt ||
                key == Key.LeftShift ||
                key == Key.RightShift ||
                key == Key.LWin ||
                key == Key.RWin ||
                key == Key.Clear ||
                key == Key.OemClear ||
                key == Key.Escape ||
                key == Key.Apps)
            {
            }
            else
            {
                if (this.IsLoaded)
                {
                    SetHotkeyIsRegistered(VmSystemConfigPage.SystemConfig.QuickConnect.HotKeyModifiers, key);
                }
            }
        }

        private void Modifiers_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded)
            {
                SetHotkeyIsRegistered(VmSystemConfigPage.SystemConfig.QuickConnect.HotKeyModifiers, VmSystemConfigPage.SystemConfig.QuickConnect.HotKeyKey);
            }
        }

        private bool SetHotkeyIsRegistered(ModifierKeys modifier, Key key)
        {
            if (modifier == SystemConfig.GetInstance().QuickConnect.HotKeyModifiers
                && key == SystemConfig.GetInstance().QuickConnect.HotKeyKey)
            {
                VmSystemConfigPage.SystemConfig.QuickConnect.HotKeyModifiers = modifier;
                VmSystemConfigPage.SystemConfig.QuickConnect.HotKeyKey = key;
                return false;
            }


            // check if HOTKEY_ALREADY_REGISTERED
            var r = GlobalHotkeyHooker.GetInstance().Regist(null, modifier, key, () => { });
            switch (r.Item1)
            {
                case GlobalHotkeyHooker.RetCode.Success:
                    GlobalHotkeyHooker.GetInstance().Unregist(r.Item3);
                    VmSystemConfigPage.SystemConfig.QuickConnect.HotKeyModifiers = modifier;
                    VmSystemConfigPage.SystemConfig.QuickConnect.HotKeyKey = key;
                    return true;
                case GlobalHotkeyHooker.RetCode.ERROR_HOTKEY_NOT_REGISTERED:
                    MessageBox.Show(SystemConfig.GetInstance().Language.GetText("info_hotkey_registered_fail") + ": " + r.Item2);
                    break;
                case GlobalHotkeyHooker.RetCode.ERROR_HOTKEY_ALREADY_REGISTERED:
                    MessageBox.Show(SystemConfig.GetInstance().Language.GetText("info_hotkey_already_registered") + ": " + r.Item2);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            VmSystemConfigPage.SystemConfig.QuickConnect.HotKeyModifiers = SystemConfig.GetInstance().QuickConnect.HotKeyModifiers;
            VmSystemConfigPage.SystemConfig.QuickConnect.HotKeyKey = SystemConfig.GetInstance().QuickConnect.HotKeyKey;

            return false;
        }
    }


    public class EnumServerOrderBy2IsCheckedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value) ? parameter : Binding.DoNothing;
        }
    }



    public class Key2KeyStringConverter : IValueConverter
    {
        // 实现接口的两个方法  
        #region IValueConverter 成员  
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Key k = (Key)value;
            return k.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}