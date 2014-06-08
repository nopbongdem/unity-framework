﻿using System.IO;
using UnityEngine;

namespace CleaveFramework.Application
{
    /// <summary>
    /// App is the Model/Controller for the Application data
    /// </summary>
    public class App
    {
        public EngineOptions Options { get; private set; }

        public App()
        {
            // initialize UnityEngine from Options
            Options = new EngineOptions();
            Command.Register(typeof(ApplyOptionsCmd), OnApplyOptions);
        }

        void OnApplyOptions(Command cmd)
        {
//             Debug.Log("Application::OnApplyOptions()");
            Screen.SetResolution(Options.Width, Options.Height, Options.FullScreen);
        }

    }
}
