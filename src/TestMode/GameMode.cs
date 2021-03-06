﻿// SampSharp
// Copyright 2015 Tim Potze
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using SampSharp.GameMode;
using SampSharp.GameMode.Controllers;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.Natives;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using TestMode.Tests;

namespace TestMode
{
    public class GameMode : BaseMode
    {
        #region Tests

        private readonly List<ITest> _tests = new List<ITest>
        {
            new CommandsTest(),
            new ASyncTest(),
            new DelayTest(),
            new MenuTest(),
            new DisposureTest(),
            new DialogTest(),
            new CharsetTest(),
            new VehicleInfoTest(),
            new NativesTest(),
            new MapAndreasTest(),
            new KeyHandlerTest(),
            new ExtensionTest(),
            new ParamsTest(),
            new ActorTest(),
            new ServicesTest()
        };

        #endregion

        // Test code below....
        private delegate int SetGameModeTextImpl(string text);
        [Native("SetGameModeText")]
        private readonly static SetGameModeTextImpl SetGameModeText = null;

        private delegate int GetNetworkStatsImpl(out string text, int len);
        [Native("GetNetworkStats")]
        private readonly static GetNetworkStatsImpl GetNetworkStats = null;

        private delegate int GetVehiclePosImpl(int vehicleid, out float x, out float y, out float z);
        [Native("GetVehiclePos")]
        private readonly static GetVehiclePosImpl GetVehiclePos = null;

        
        // End test code.

        #region Overrides of BaseMode

        protected override void OnInitialized(EventArgs args)
        {
            float x, y, z;
            SetGameModeText("ABCDE");
            int id = Native.CreateVehicle(400, 10f, 20f, 30f, 40f, -1, -1, 30);

            var func = new NativeFunction("GetVehiclePos", typeof (int), typeof (float).MakeByRefType(),
                typeof (float).MakeByRefType(), typeof (float).MakeByRefType());

            func.Invoke(__arglist(id, out x, out y, out z));
            Console.WriteLine("Invoke NativeFunction: {0}, {1}, {2}", x, y, z);

            GetVehiclePos(id, out x, out y, out z);
            Console.WriteLine("Delegate: {0}, {1}, {2}", x, y, z);
            string outp;
            GetNetworkStats(out outp, 500);
            //Console.WriteLine(outp);
            return;
            Console.WriteLine("TestMode for SampSharp");
            Console.WriteLine("----------------------");

            Server.ToggleDebugOutput(true);
            SetGameModeText("sa-mp# testmode");
            UsePlayerPedAnimations();

            AddPlayerClass(65, new Vector3(5), 0);

            foreach (ITest test in _tests)
            {
                Console.WriteLine("=========");
                Console.WriteLine("Starting test: {0}", test);
                test.Start(this);
                Console.WriteLine();
            }

            base.OnInitialized(args);
        }

        protected override void OnRconCommand(RconEventArgs e)
        {
            Console.WriteLine("[RCON] {0}", e.Command);
            GtaPlayer.SendClientMessageToAll("Rcon message: {0}", e.Command);

            Console.WriteLine("Throwing exception after a stack filter...");
            StackFiller(1);

            e.Success = false;
            base.OnRconCommand(e);
        }

        private void StackFiller(int c)
        {
            if (c <= 0)
                throw new Exception();
            StackFiller(c - 1);

        }

        protected override void LoadControllers(ControllerCollection controllers)
        {
            base.LoadControllers(controllers);

            foreach (IControllerTest test in _tests.OfType<IControllerTest>())
                test.LoadControllers(controllers);
        }

        #region Overrides of BaseMode

        /// <summary>
        ///     Raises the <see cref="E:CallbackException" /> event.
        /// </summary>
        /// <param name="e">The <see cref="ExceptionEventArgs"/> instance containing the event data.</param>
        protected override void OnCallbackException(ExceptionEventArgs e)
        {
            Console.WriteLine("[SampSharp] Exception thrown during execution of {0}:", e.Exception.TargetSite.Name);

            Console.WriteLine("{0}: {1}",e.Exception.GetType().FullName, e.Exception.Message);
            Console.WriteLine(e.Exception.StackTrace);
            e.Handled = true;
            base.OnCallbackException(e);
        }

        #endregion

        #endregion

    }
}
