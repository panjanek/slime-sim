using System.Text;
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
using SlimeSim.Gui;
using SlimeSim.Utils;
using AppContext = SlimeSim.Models.AppContext;
using Application = System.Windows.Application;

namespace SlimeSim
{
    public partial class MainWindow : Window
    {
        private bool uiPending;

        private DateTime lastCheckTime;

        private long lastCheckFrameCount;

        private long lastChechStepCount;

        private FullscreenWindow fullscreen;

        private AppContext app;

        private System.Timers.Timer systemTimer;
        public MainWindow()
        {
            InitializeComponent();
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        private void parent_Loaded(object sender, RoutedEventArgs e)
        {
            app = new AppContext(this);
            KeyDown += MainWindow_KeyDown;
            systemTimer = new System.Timers.Timer() { Interval = 5 };
            systemTimer.Elapsed += SystemTimer_Elapsed;
            systemTimer.Start();
            DispatcherTimer infoTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1.0) };
            infoTimer.Tick += InfoTimer_Tick;
            infoTimer.Start();
            DebugUtil.TestBeta();
        }

        public void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    app.renderer.Paused = !app.renderer.Paused;
                    e.Handled = true;
                    break;
                case Key.F:
                    ToggleFullscreen();
                    e.Handled = true;
                    break;
                case Key.Escape:
                    if (fullscreen!=null)
                        ToggleFullscreen();
                    e.Handled = true;
                    break;
            }
        }

        public void ToggleFullscreen()
        {
            if (fullscreen == null)
            {
                parent.Children.Remove(placeholder);
                fullscreen = new FullscreenWindow() { Owner = Window.GetWindow(this) };
                fullscreen.KeyDown += MainWindow_KeyDown;
                fullscreen.ContentHost.Content = placeholder;
                fullscreen.Show();
            }
            else
            {
                fullscreen.ContentHost.Content = null;
                parent.Children.Add(placeholder);
                fullscreen.Close();
                fullscreen = null;
            }
        }

        private void SystemTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (!uiPending)
            {
                uiPending = true;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        app.renderer.Step();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    finally
                    {
                        uiPending = false;
                    }

                    uiPending = false;
                }), DispatcherPriority.Render);
            }
        }

        private void InfoTimer_Tick(object? sender, EventArgs e)
        {
            var now = DateTime.Now;
            var timespan = now - lastCheckTime;
            double frames = app.renderer.FrameCounter - lastCheckFrameCount;
            double steps = app.simulation.step - lastChechStepCount;
            if (timespan.TotalSeconds >= 0.0001)
            {

                double fps = frames / timespan.TotalSeconds;
                double sps = steps / timespan.TotalSeconds;
                Title = $"Slime Sim. " +
                        $"fps:{fps.ToString("0.0")} " +
                        $"config:{app.simulation.shaderConfig.width}x{app.simulation.shaderConfig.height}/{app.simulation.shaderConfig.agentsCount} " +
                        $"step:{app.simulation.step} " +
                        $"gen:{app.simulation.generation} ";

                if (!string.IsNullOrWhiteSpace(app.configWindow.RecordDir))
                {
                    Title += $"[recording to {app.configWindow.RecordDir}] ";
                }

                app.configWindow.SetTitle(Title);

                lastCheckFrameCount = app.renderer.FrameCounter;
                lastChechStepCount = app.simulation.step;
                lastCheckTime = now;
            }
        }
    }
}