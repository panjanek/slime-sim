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

        private List<StatsSeries> series;

        private bool updating;

        public bool GraphCommonScale => commonScaleCheckbox.IsChecked == true;

        public int NavigationMode { get; private set; } = 0;

        public bool Evolve { get; private set; } = true;

        public bool ShowPointers { get; private set; } = true;

        public string RecordDir { get; private set; }

        public bool MaxSpeed { get; private set; }

        public bool ShowMemory { get; private set; }

        public int GraphHistory
        {
            get
            {
                if (graphHistoryCombo.SelectedItem == null)
                    return 100;

                return int.Parse(WpfUtil.GetTagAsString(graphHistoryCombo.SelectedItem));
            }
        }

        private ObservableCollection<StatsSeries> seriesCollection;
        public ConfigWindow(AppContext app)
        {
            this.app = app;
            InitializeComponent();
            customTitleBar.MouseLeftButtonDown += (s, e) => { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); };
            minimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;
            Closing += (s, e) => { e.Cancel = true; WindowState = WindowState.Minimized; };
            seriesCollection = new ObservableCollection<StatsSeries>()
            {
                // -------------------------------------------- AVG -------------------------------------------
                new StatsSeries() {
                    Name = "blue avg fitness",
                    Selector = s=>s.topBlueAvgFitness,
                    Style = new SeriesStyle() { StrokeThickness = 3, Stroke = Brushes.Blue },
                    IsSelected = false
                },
                new StatsSeries() {
                    Name = "red avg fitness",
                    Selector = s=>s.topRedAvgFitness,
                    Style = new SeriesStyle() { StrokeThickness = 3, Stroke = Brushes.Red },
                    IsSelected = false
                },

                // -------------------------------------------------- MEDIAN ------------------------------------
                new StatsSeries() {
                    Name = "blue med fitness",
                    Selector = s=>s.topBlueMedFitness,
                    Style = new SeriesStyle() { StrokeThickness = 3, Stroke = Brushes.Blue },
                    IsSelected = true
                },
                new StatsSeries() {
                    Name = "red med fitness",
                    Selector = s=>s.topRedMedFitness,
                    Style = new SeriesStyle() { StrokeThickness = 3, Stroke = Brushes.Red },
                    IsSelected = true
                },

                // -------------------------------------------------- OTHER ------------------------------------
                new StatsSeries() {
                    Name = "blue avg age",
                    Selector = s=>s.topBlueAvgAge,
                    Style = new SeriesStyle() { StrokeThickness = 1, Stroke = Brushes.Cyan },
                    IsSelected = false
                },
                new StatsSeries() {
                    Name = "red avg age",
                    Selector = s=>s.topRedAvgAge,
                    Style = new SeriesStyle() { StrokeThickness = 1, Stroke = Brushes.Magenta },
                    IsSelected = false
                },
                new StatsSeries() {
                    Name = "blue meals/age",
                    Selector = s=>s.topBlueMealsPerAge,
                    Style = new SeriesStyle() { StrokeThickness = 3, Stroke = Brushes.Cyan },
                    IsSelected = true
                },
                new StatsSeries() {
                    Name = "red meals/age",
                    Selector = s=>s.topRedMealsPerAge,
                    Style = new SeriesStyle() { StrokeThickness = 3, Stroke = Brushes.Magenta },
                    IsSelected = true
                },
                new StatsSeries() {
                    Name = "active plants",
                    Selector = s=>s.plantsCount,
                    Style = new SeriesStyle() { StrokeThickness = 2, Stroke = Brushes.Green },
                    IsSelected = false
                },
                new StatsSeries() {
                    Name = "blue deaths",
                    Selector = s=>s.blueDeaths,
                    Style = new SeriesStyle() { StrokeThickness = 2, Stroke = Brushes.Yellow },
                    IsSelected = false
                },

                new StatsSeries() {
                    Name = "top near prey",
                    Selector = s=>s.topNearPrey,
                    Style = new SeriesStyle() { StrokeThickness = 2, Stroke = Brushes.Gray },
                    IsSelected = true
                },
                new StatsSeries() {
                    Name = "all near prey",
                    Selector = s=>s.allNearPrey,
                    Style = new SeriesStyle() { StrokeThickness = 2, Stroke = Brushes.Gray, StrokeDashArray = new DoubleCollection() { 0, 3 }, LineCap = PenLineCap.Round },
                    IsSelected = false
                },

                new StatsSeries() {
                    Name = "blue energy spent",
                    Selector = s=>s.topBlueEnergySpent,
                    Style = new SeriesStyle() { StrokeThickness = 1, Stroke = Brushes.Blue },
                    IsSelected = false
                },
                new StatsSeries() {
                    Name = "red energy spent",
                    Selector = s=>s.topRedEnergySpent,
                    Style = new SeriesStyle() { StrokeThickness = 1, Stroke = Brushes.Red },
                    IsSelected = false
                },

                new StatsSeries() {
                    Name = "top survival dur",
                    Selector = s=>s.topSurvival,
                    Style = new SeriesStyle() { StrokeThickness = 1, Stroke = Brushes.Pink },
                    IsSelected = true
                },

                new StatsSeries() {
                    Name = "blue diversity",
                    Selector = s=>s.blueDiversity,
                    Style = new SeriesStyle() { StrokeThickness = 3, Stroke = Brushes.DarkBlue, StrokeDashArray = new DoubleCollection() { 2, 4 } },
                    IsSelected = true
                },
                new StatsSeries() {
                    Name = "red diversity",
                    Selector = s=>s.redDiversity,
                    Style = new SeriesStyle() { StrokeThickness = 3, Stroke = Brushes.DarkRed, StrokeDashArray = new DoubleCollection() { 2, 4 } },
                    IsSelected = true
                },
            };

            Loaded += ConfigWindow_Loaded;
            graphHistoryCombo.SelectionChanged += (s, e) => statsGraph.Redraw();
            commonScaleCheckbox.Click += (s, e) => statsGraph.Redraw();
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
                                    sim.InitAfterLoad();
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
            maxSpeedCheckbox.Click += (s, e) => MaxSpeed = maxSpeedCheckbox.IsChecked == true;
            memoryCheckbox.Click += (s, e) => ShowMemory = memoryCheckbox.IsChecked == true;

            // evolution rate combos
            evolveCombo.SelectionChanged += (s, e) =>
            {
                if (!updating)
                {
                    var value = WpfUtil.GetTagAsInt(evolveCombo.SelectedItem);
                    Evolve = (value > 0);
                    if (Evolve)
                        app.simulation.shaderConfig.generationDuration = value;
                }
            };
            mutationMagnitudeCombo.SelectionChanged += (s, e) =>
            {
                if (!updating)
                    app.simulation.mutationMagnitude = WpfUtil.GetTagAsDouble(mutationMagnitudeCombo.SelectedItem);
            };
            mutationFrequencyCombo.SelectionChanged += (s, e) =>
            {
                if (!updating)
                    app.simulation.mutationFrequency = WpfUtil.GetTagAsDouble(mutationFrequencyCombo.SelectedItem);
            };
            crossingoverFrequencyCombo.SelectionChanged += (s, e) =>
            {
                if (!updating)
                    app.simulation.crossingOverFrequency = WpfUtil.GetTagAsDouble(crossingoverFrequencyCombo.SelectedItem);
            };

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

        public void SetMemoryBars(Agent agent)
        {
            if (ShowMemory)
            {
                var brush = agent.type == 1 ? Brushes.Blue : Brushes.Red;
                memory0Bar.Foreground = brush;
                memory1Bar.Foreground = brush;
                memory0Bar.Value = agent.memory0;
                memory1Bar.Value = agent.memory1;
            }
        }

        private void ConfigWindow_Loaded(object sender, RoutedEventArgs e)
        {
            seriesList.ItemsSource = seriesCollection;
        }

        public void DrawStats(List<Stats> stats)
        {
            statsGraph.UpdateSeries(GetVisibleSeries());
            statsGraph.Draw(stats);
        }

        public void ClearStats()
        {
            statsGraph.Children.Clear();
        }

        public List<StatsSeries> GetVisibleSeries()
        {
            if (seriesCollection == null || seriesCollection.Count(c => c.IsSelected) == 0)
                return series;
            else
                return seriesCollection.Where(c => c.IsSelected).Select(c => c).ToList();

        }

        private void SeriesCheckBox_Click(object sender, RoutedEventArgs e) => statsGraph.UpdateSeries(GetVisibleSeries());
    }
}
