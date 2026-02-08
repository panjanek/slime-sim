using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;
using SlimeSim.Models;
using SlimeSim.Utils;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using AppContext = SlimeSim.Models.AppContext;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace SlimeSim.Gui
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        private AppContext app;

        private bool updating;

        public int NavigationMode { get; private set; } = 0;

        public bool ShowPointers { get; private set; } = true;

        public string RecordDir { get; private set; }

        public ConfigWindow(AppContext app)
        {
            this.app = app;
            InitializeComponent();
            customTitleBar.MouseLeftButtonDown += (s, e) => { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); };
            minimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;
            Closing += (s, e) => { e.Cancel = true; WindowState = WindowState.Minimized; };


            Loaded += ConfigWindow_Loaded;

            saveButton.Click += (s, e) =>
            {
                app.renderer.Stopped = true;
                var dialog = new CommonSaveFileDialog { Title = "Save simulation to gz or json file", DefaultExtension = "gz",  AlwaysAppendDefaultExtension = false };
                dialog.Filters.Add(new CommonFileDialogFilter("GZIP files", "*.gz"));
                dialog.Filters.Add(new CommonFileDialogFilter("JSON files", "*.json"));
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    try
                    {
                        app.Save(dialog.FileName);
                        PopupMessage.Show(app.mainWindow, $"Simulation saved to {dialog.FileName}");
                    }
                    catch (Exception ex)
                    {
                        PopupMessage.Show(app.mainWindow, $"Something went wrong: {ex.Message}");
                    }
                }
                app.renderer.Stopped = false;
            };
            loadButton.Click += (s, e) =>
            {
                app.renderer.Paused = true;
                var dialog = new CommonOpenFileDialog { Title = "Open simulation gz or json file" };
                dialog.Filters.Add(new CommonFileDialogFilter("Simulation files", "*.gz;*.json"));
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    try
                    {
                        app.Load(dialog.FileName);
                        PopupMessage.Show(app.mainWindow, $"Simulation loaded from {dialog.FileName}");
                    }
                    catch (Exception ex)
                    {
                        PopupMessage.Show(app.mainWindow, $"Something went wrong: {ex.Message}");
                    }
                }
                app.renderer.Paused = false;
            };
            newButton.Click += (s, e) =>
            {
                app.renderer.Stopped = true;
                var parameters = DialogUtil.ShowStartNewSimulationDialog();
                if (parameters != null)
                {
                    try
                    {
                        if (parameters.useExistingAgents)
                        {
                            if (parameters.loadAgentsFromFiles)
                            {
                                foreach(var fn in parameters.fileNames)
                                {
                                    string json = fn.EndsWith(".gz") ? GzipUtil.Decompress(File.ReadAllBytes(fn)) : File.ReadAllText(fn);
                                    var sim = SerializationUtil.DeserializeFromJson(json);
                                    parameters.sources.Add(sim);
                                }
                            }
                            else
                            {
                                parameters.sources.Add(app.simulation);
                            }
                        }
                        app.Start(parameters);
                        PopupMessage.Show(app.mainWindow, $"New simulation started.");
                    }
                    catch (Exception ex)
                    {
                        PopupMessage.Show(app.mainWindow, $"Something went wrong: {ex.Message}");
                    }
                }
                app.renderer.Stopped = false;
            };

            KeyDown += (s, e) => app.mainWindow.MainWindow_KeyDown(s, e);
            navigationCombo.SelectionChanged += (s, e) => NavigationMode = WpfUtil.GetTagAsInt(navigationCombo.SelectedItem);
            pointersCheckbox.Click += (s, e) => ShowPointers = pointersCheckbox.IsChecked == true;

            recordButton.Click += (s, e) =>
            {

                if (recordButton.IsChecked == true)
                {
                    app.renderer.Paused = true;
                    var dialog = new CommonOpenFileDialog { IsFolderPicker = true, Title = "Select folder to save frames as PNG files" };
                    if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                        RecordDir = dialog.FileName;
                    else
                        recordButton.IsChecked = false;
                    app.renderer.Paused = false;
                }
                else
                {
                    RecordDir = null;
                }

                e.Handled = true;
            };
        }

        public void SetControls()
        {
            updating = true;
            WpfUtil.SelectByIntTag(evolveCombo, app.simulation.shaderConfig.generationDuration);
            WpfUtil.SelectByDoubleTag(mutationMagnitudeCombo, app.simulation.mutationMagnitude);
            WpfUtil.SelectByDoubleTag(mutationFrequencyCombo, app.simulation.mutationFrequency);
            WpfUtil.SelectByDoubleTag(crossingoverFrequencyCombo, app.simulation.crossingOverFrequency);
            updating = false;
        }

        public void SetTitle(string title)
        {
            Title = title;
            titleText.Text = title;
        }

        private void ConfigWindow_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
