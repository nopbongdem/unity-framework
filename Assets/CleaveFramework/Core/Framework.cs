﻿using System;
using System.Collections.Generic;
using System.Linq;
using CleaveFramework.Interfaces;
using UnityEngine;
using System.Collections;

namespace CleaveFramework.Core
{
    /// <summary>
    /// AppManager is the main script that will register with a Unity GameObject
    /// the AppManager view object MUST be in every scene that will execute the framework
    /// </summary>
    public class Framework : MonoBehaviour
    {
        /// <summary>
        /// name of the scene to use while transitioning into the next scene
        /// </summary>
        [SerializeField] private string _transitionScene = "Loading";
        public static string TransitionScene
        {
            get { return Instance._transitionScene; }
        }

        /// <summary>
        /// singleton instance
        /// </summary>
        public static Framework Instance { get; private set; }

        private static CommandQueue _commands;

        /// <summary>
        /// singleton library
        /// </summary>
        private static Dictionary<Type, object> _singletons;

        /// <summary>
        /// IUpdateable objects library
        /// </summary>
        private static Dictionary<Type, List<IUpdateable>> _updateables; 

        void Awake()
        {
            if (Instance != this && Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                _singletons = new Dictionary<Type, object>();
                _updateables = new Dictionary<Type, List<IUpdateable>>();

                _commands = new CommandQueue();
                
                InjectAsSingleton(new SceneManager());
                InjectAsSingleton(new App());

                StartCoroutine(ProcessCommands());
                DontDestroyOnLoad(gameObject);
            }
            Debug.Log("Creating Level: " + UnityEngine.Application.loadedLevelName);
        }

        void Update()
        {
            // process IUpdateables
            foreach (var obj in _updateables.SelectMany(updateList => updateList.Value))
            {
                obj.Update();
            }
        }

        static IEnumerator ProcessCommands()
        {
            while (true)
            {
                _commands.Process(1);
                yield return null;
            }
        }

        /// <summary>
        /// push a command with no delay into the command queue
        /// </summary>
        /// <param name="cmd">the command</param>
        public static void PushCommand(Command cmd)
        {
            _commands.Push(cmd);
        }

        /// <summary>
        /// push a command with a frame count execution delay
        /// </summary>
        /// <param name="cmd">the command</param>
        /// <param name="frameDelay"># of frames to wait until command will be executed</param>
        public static void PushCommand(Command cmd, int frameDelay)
        {
            _commands.Push(cmd, frameDelay);
        }

        /// <summary>
        /// push a command with a timed execution delay
        /// </summary>
        /// <param name="cmd">the command</param>
        /// <param name="timeDelay"># of seconds to wait until command will be executed</param>
        public static void PushCommand(Command cmd, float timeDelay)
        {
            _commands.Push(cmd, timeDelay);
        }

        /// <summary>
        /// Injects a new object into the framework singleton library
        /// </summary>
        /// <typeparam name="T">object type</typeparam>
        /// <param name="dep">object instance</param>
        public static void InjectAsSingleton<T>(T dep)
        {
            if (typeof(IUpdateable).IsAssignableFrom(typeof(T)))
            {
                if (_updateables.ContainsKey(typeof (T)))
                {
                    // we clear the list because we're singleton
                    _updateables[typeof(T)].Clear();
                }
                else
                {
                    // make a new list
                    _updateables.Add(typeof(T), new List<IUpdateable>());
                }
                // store it
                _updateables[typeof(T)].Add((IUpdateable)dep);
            }

            if(!_singletons.ContainsKey(typeof(T)))
            {
                // insert object into the library
                _singletons.Add(typeof(T), dep);
            }
            else
            {
                // overwrite existing object with new instance
                _singletons[typeof (T)] = dep;
            }
        }

        /// <summary>
        /// resolve a type into an instance
        /// </summary>
        /// <typeparam name="T">object type to retrieve</typeparam>
        /// <returns>object instance if injected, otherwise null</returns>
        public static object ResolveSingleton<T>()
        {
            return _singletons.ContainsKey(typeof (T)) ? _singletons[typeof (T)] : null;
        }
    }
}