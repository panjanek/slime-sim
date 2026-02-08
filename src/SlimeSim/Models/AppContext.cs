using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SlimeSim.Gpu;
using SlimeSim.Gui;
using SlimeSim.Utils;

namespace SlimeSim.Models
{
    public class AppContext
    {
        public Simulation simulation;

        public MainWindow mainWindow;

        public OpenGlRenderer renderer;

        public ConfigWindow configWindow;

        public AppContext(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            this.simulation = new Simulation();
            simulation.SetFlags();
            renderer = new OpenGlRenderer(mainWindow.placeholder, this);
            renderer.UploadAgents();
            renderer.ResetOrigin();
            configWindow = new ConfigWindow(this);
            configWindow.Show();
            configWindow.Activate();
        }

        public void Load(string fn)
        {
            string json = fn.EndsWith(".gz") ? GzipUtil.Decompress(File.ReadAllBytes(fn)) : File.ReadAllText(fn);
            renderer.Stopped = true;
            simulation = SerializationUtil.DeserializeFromJson(json);
            configWindow.SetControls();
            renderer.UploadAgents();
            renderer.ClearTextures();
            renderer.Stopped = false;
        }

        public void Save(string fn)
        {
            renderer.Stopped = true;
            renderer.DownloadAgents();
            renderer.Stopped = false;
            var json = SerializationUtil.SerializeToJson(simulation);
            if (fn.EndsWith(".gz"))
                File.WriteAllBytes(fn, GzipUtil.Compress(json));
            else
                File.WriteAllText(fn, json);
        }

        public void Start(StartNewSimulationParameters parameters)
        {
            renderer.Stopped = true;
            simulation = new Simulation(parameters);
            renderer.UploadAgents();
            renderer.ClearTextures();
            renderer.Stopped = false;
        }
    }
}
