using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using SlimeSim.Models;
using static System.Net.Mime.MediaTypeNames;
using AppContext = SlimeSim.Models.AppContext;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using MessageBox = System.Windows.MessageBox;
using Orientation = System.Windows.Controls.Orientation;
using Panel = System.Windows.Controls.Panel;
using TextBox = System.Windows.Controls.TextBox;

namespace SlimeSim.Gui
{
    public static class DialogUtil
    {
        public static StartNewSimulationParameters ShowStartNewSimulationDialog()
        {
            // Window
            Window dialog = new Window()
            {
                Width = 400,
                Height = 500,
                Title = "Start new simulation",
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.ToolWindow,
                Owner = Application.Current.MainWindow
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true
            };

            // Layout
            StackPanel panel = new StackPanel() { Margin = new Thickness(10) };

            TextBlock txt = new TextBlock() { Text = "Choose simulation start parameters:", Margin = new Thickness(0, 0, 0, 10) };

            ComboBox sizeCombo = new ComboBox() { Margin = new Thickness(0, 5, 0, 0) };
            sizeCombo.Items.Add(new ComboBoxItem() { Content = "960x540", Tag = new StartNewSimulationParameters() { width = 960, height = 540 } });
            sizeCombo.Items.Add(new ComboBoxItem() { Content = "1280x1080", Tag = new StartNewSimulationParameters() { width = 1280, height = 720 } });
            sizeCombo.Items.Add(new ComboBoxItem() { Content = "1920x1080", Tag = new StartNewSimulationParameters() { width = 1920, height = 1080 }, IsSelected = true });
            sizeCombo.Items.Add(new ComboBoxItem() { Content = "2880x1620", Tag = new StartNewSimulationParameters() { width = 2880, height = 1620 } });
            sizeCombo.Items.Add(new ComboBoxItem() { Content = "3840x2160", Tag = new StartNewSimulationParameters() { width = 3840, height = 2160 } });
            sizeCombo.Items.Add(new ComboBoxItem() { Content = "7680x4320", Tag = new StartNewSimulationParameters() { width = 7680, height = 4320 } });

            var jsonBox = new TextBox
            {
                AcceptsReturn = true,        // Enables multiline
                AcceptsTab = true,           // Optional: allow tabs
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                MinHeight = 80,
                Margin = new Thickness(0, 5, 0, 0), Height = 200
            };

            jsonBox.Text = JsonSerializer.Serialize(new StartNewSimulationParameters(), options);


            ComboBox seedCombo = new ComboBox() { Margin = new Thickness(0, 5, 0, 0) };
            seedCombo.Items.Add(new ComboBoxItem() { Content = "Initialize agents randomly with fixed seed", Tag = new StartNewSimulationParameters() { fixedSeed = true }, IsSelected = true });
            seedCombo.Items.Add(new ComboBoxItem() { Content = "Initialize agents randomly with random seed", Tag = new StartNewSimulationParameters() { fixedSeed = false } });
            seedCombo.Items.Add(new ComboBoxItem() { Content = "Use agents from current simulation", Tag = new StartNewSimulationParameters() { useExistingAgents = true } });
            seedCombo.Items.Add(new ComboBoxItem() { Content = "Load agents from (multiple) files", Tag = new StartNewSimulationParameters() { useExistingAgents = true, loadAgentsFromFiles = true } });

            // Buttons
            StackPanel buttonPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };

            Button ok = new Button() { Content = "Start!", Width = 70, Margin = new Thickness(5, 0, 0, 0) };
            Button cancel = new Button() { Content = "Cancel", Width = 70 };
            Button addFiles = new Button() { Content = "Add...", Width = 70, Margin = new Thickness(5, 0, 0, 0), HorizontalAlignment = HorizontalAlignment.Right, IsEnabled = false };
            TextBlock txtFiles = new TextBlock() { Text = "", Margin = new Thickness(0, 0, 0, 10), FontSize = 10, TextWrapping = TextWrapping.Wrap, Height = 50 };
            List<string> fileNames = new List<string>();
            seedCombo.SelectionChanged += (s, e) =>
            {
                var selectedSeed = WpfUtil.GetTagAsObject<StartNewSimulationParameters>(seedCombo.SelectedItem);
                addFiles.IsEnabled = selectedSeed.loadAgentsFromFiles;
                ok.IsEnabled = !selectedSeed.loadAgentsFromFiles || fileNames.Count > 0;
            };

           
            addFiles.Click += (s, e) =>
            {
                var openDialog = new CommonOpenFileDialog { Title = "Open simulation gz or json file", Multiselect = true };
                openDialog.Filters.Add(new CommonFileDialogFilter("Simulation files", "*.gz;*.json"));
                if (openDialog.ShowDialog() == CommonFileDialogResult.Ok)
                    fileNames.AddRange(openDialog.FileNames);
                txtFiles.Text = "Load agents from files: " + string.Join(",  ", fileNames.Select(fn=>Path.GetFileName(fn)));
                var selectedSeed = WpfUtil.GetTagAsObject<StartNewSimulationParameters>(seedCombo.SelectedItem);
                ok.IsEnabled = !selectedSeed.loadAgentsFromFiles || fileNames.Count > 0;
            };

            ok.Click += (s, e) => { dialog.DialogResult = true; dialog.Close(); };
            cancel.Click += (s, e) => { dialog.DialogResult = false; dialog.Close(); };

            buttonPanel.Children.Add(cancel);
            buttonPanel.Children.Add(ok);

            // Compose UI
            panel.Children.Add(txt);
            panel.Children.Add(sizeCombo);
            panel.Children.Add(jsonBox);
            panel.Children.Add(seedCombo);
            panel.Children.Add(addFiles);
            panel.Children.Add(txtFiles);
            panel.Children.Add(buttonPanel);

            dialog.Content = panel;

            // Show input dialog
            if (dialog.ShowDialog() == true)
            {
                var selectedSize = WpfUtil.GetTagAsObject<StartNewSimulationParameters>(sizeCombo.SelectedItem);
                var selectedSeed = WpfUtil.GetTagAsObject<StartNewSimulationParameters>(seedCombo.SelectedItem);
                var parameters = JsonSerializer.Deserialize<StartNewSimulationParameters>(jsonBox.Text, options);
                parameters.width = selectedSize.width;
                parameters.height = selectedSize.height;
                parameters.fixedSeed = selectedSeed.fixedSeed;
                parameters.useExistingAgents = selectedSeed.useExistingAgents;
                parameters.loadAgentsFromFiles = selectedSeed.loadAgentsFromFiles;
                parameters.fileNames = fileNames;
                return parameters;

            }
            else
            {
                return null;
            }
        }
    }
}
