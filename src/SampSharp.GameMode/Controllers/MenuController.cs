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

using System.Linq;
using SampSharp.GameMode.Display;
using SampSharp.GameMode.World;

namespace SampSharp.GameMode.Controllers
{
    /// <summary>
    ///     A controller processing all menu actions.
    /// </summary>
    public class MenuController : IEventListener
    {
        /// <summary>
        ///     Registers the events this PlayerController wants to listen to.
        /// </summary>
        /// <param name="gameMode">The running GameMode.</param>
        public void RegisterEvents(BaseMode gameMode)
        {
            gameMode.PlayerExitedMenu += (sender, args) =>
            {
                var player = sender as GtaPlayer;
                Menu menu = Menu.All.FirstOrDefault(m => m.Viewers.Contains(player));

                if (menu != null)
                    menu.OnExit(player, args);
            };

            gameMode.PlayerSelectedMenuRow += (sender, args) =>
            {
                var player = sender as GtaPlayer;
                Menu menu = Menu.All.FirstOrDefault(m => m.Viewers.Contains(player));

                if (menu != null)
                    menu.OnResponse(player, args);
            };
        }
    }
}