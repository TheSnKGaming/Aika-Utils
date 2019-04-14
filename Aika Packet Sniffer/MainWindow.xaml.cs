﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Aika_Packet_Sniffer.Logger;
using Aika_Packet_Sniffer.Logger.Xml;
using Aika_Packet_Sniffer.Model;
using Aika_Packet_Sniffer.Network;

namespace Aika_Packet_Sniffer
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<ListViewModel> _packets = new List<ListViewModel>();
        private Proxy _snifferProxy;
        private bool _isProxyRunning;

        public MainWindow()
        {
            InitializeComponent();
            System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en");
            RulesParser.Init();
            _isProxyRunning = false;
        }

        private void PacketsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0) return;

            var packet = e.AddedItems[0];
            if (!(packet is PacketListView data)) return;

            HexView.Stream = new MemoryStream(data.Data);
            HexView.ReadOnlyMode = true;

            foreach (var pack in _packets)
            {
                if (pack.PacketListView.Index != data.Index) continue;

                PacketParseListView.Items.Clear();
                foreach (var listView in pack.PacketParseListView)
                {
                    PacketParseListView.Items.Add(listView);
                }
            }
        }

        private void PacketParseListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0) return;

            var packet = e.AddedItems[0];
            if (!(packet is PacketParseListView data)) return;

            HexView.SelectionStart = data.Start;
            HexView.SelectionStop = data.End - 1;
        }

        private void UpdateWithParsedPacket(List<ListViewModel> data)
        {
            foreach (var model in data)
            {
                _packets.Add(model);
                _packets[_packets.Count - 1].PacketListView.Index = (uint) _packets.Count - 1;
                PacketsListView.Dispatcher.Invoke(DispatcherPriority.Normal, (Action) delegate { PacketsListView.Items.Add(model.PacketListView); });
            }
        }

        private void LogReadFinished(List<ListViewModel> packets)
        {
            _packets.Clear();
            _packets = packets;
            PacketsListView.Dispatcher.Invoke(DispatcherPriority.Normal, (Action) delegate
            {
                PacketsListView.Items.Clear();
                PacketsListView.SelectedIndex = -1;
            });
            foreach (var packet in packets)
            {
                PacketsListView.Dispatcher.Invoke(DispatcherPriority.Normal, (Action) delegate { PacketsListView.Items.Add(packet.PacketListView); });
            }
        }

        private void PlayBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (!_isProxyRunning)
            {
                PlayBtn.IsEnabled = false;
                PacketsListView.Items.Clear();
                PacketsListView.SelectedIndex = -1;
                PacketParseListView.Items.Clear();
                PacketParseListView.SelectedIndex = -1;
                _packets.Clear();
                StatusRunning.IsIndeterminate = true;
                var action = new Action<List<ListViewModel>>(UpdateWithParsedPacket);
                _snifferProxy = new Proxy(action);
                _snifferProxy.Start();
            }
            else
            {
                PlayBtn.IsEnabled = true;
                StatusRunning.IsIndeterminate = false;
                _snifferProxy.Stop();
                _isProxyRunning = false;
            }
        }

        private void StopBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (_isProxyRunning) return;

            PlayBtn.IsEnabled = true;
            StatusRunning.IsIndeterminate = false;
            _snifferProxy?.Stop();
            _isProxyRunning = false;
        }

        private void SaveBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                DefaultExt = ".packet",
                Filter = "Packet log (*.packet)|*.packet"
            };
            saveDialog.ShowDialog();
            if (saveDialog.FileName == "") return;

            var logSave = new LogSave(_packets);
            logSave.Save(saveDialog.FileName);
        }

        private void OpenBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (_isProxyRunning) return;

            var fileDialog = new OpenFileDialog
            {
                DefaultExt = ".packet",
                Filter = "Packet log (*.packet)|*.packet"
            };
            if (fileDialog.ShowDialog() != true) return;

            var action = new Action<List<ListViewModel>>(LogReadFinished);
            var logReader = new LogReader(fileDialog.FileName, action);
            logReader.Load();
        }

        private void Clear_OnClick(object sender, RoutedEventArgs e)
        {
            if (_isProxyRunning)
            {
                MessageBox.Show("Proxy is still running.");
                return;
            }

            _packets.Clear();
            PacketsListView.Items.Clear();
            PacketParseListView.Items.Clear();
        }

        private void Exit_OnClick(object sender, RoutedEventArgs e)
        {
            _isProxyRunning = false;
            _snifferProxy?.Stop();
            Environment.Exit(0);
        }

        private void HexView_OnSelectionLengthChanged(object sender, EventArgs e)
        {
            SelectionHexLenght.Text = $"Selected: {HexView.SelectionLength}";
            if (HexView.SelectionLength <= 0 && HexView.SelectionLength > 20) return;
            try
            {
                var hex = HexView.SelectionByteArray;
                var value = "";
                switch (hex.Length)
                {
                    case 1:
                        value = $"int8: {hex[0]} / {unchecked((sbyte) hex[0])} ";
                        break;
                    case 2:
                        value = $"int16: {BitConverter.ToInt16(hex, 0)} / {BitConverter.ToUInt16(hex, 0)} ";
                        break;
                    case 4:
                        value = $"int32: {BitConverter.ToInt32(hex, 0)} / {BitConverter.ToUInt32(hex, 0)} ";
                        value += $"float: {BitConverter.ToSingle(hex, 0)}";
                        break;
                    case 8:
                        value = $"int64: {BitConverter.ToInt64(hex, 0)} / {BitConverter.ToUInt64(hex, 0)} ";
                        value += $"double: {BitConverter.ToDouble(hex, 0)}";
                        break;
                    default:
                        var tmpByte = hex;
                        for (var i = 0; i < hex.Length; i++)
                            if (tmpByte[i].Equals(0xCC))
                                tmpByte[i] = 0x00;
                        value = $"string: {Encoding.UTF8.GetString(tmpByte)}";
                        break;
                }

                SelectionHex.Text = value.Replace(Environment.NewLine, " ");
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}