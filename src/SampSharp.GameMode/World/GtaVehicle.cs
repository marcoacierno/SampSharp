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
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.Helpers;
using SampSharp.GameMode.Natives;
using SampSharp.GameMode.Pools;

namespace SampSharp.GameMode.World
{
    /// <summary>
    ///     Represents a SA-MP vehicle.
    /// </summary>
    public class GtaVehicle : IdentifiedPool<GtaVehicle>, IIdentifiable, IWorldObject
    {
        /// <summary>
        ///     Identifier indicating the handle is invalid.
        /// </summary>
        public const int InvalidId = 0xFFFF;

        /// <summary>
        ///     Maximum number of vehicles which can exist.
        /// </summary>
        public const int Max = 2000;

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GtaVehicle" /> class.
        /// </summary>
        /// <param name="id">The ID of the vehicle to initialize.</param>
        public GtaVehicle(int id)
        {
            Id = id;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return string.Format("Vehicle(Id:{0}, Model: {1})", Id, Model);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets an instance of <see cref="VehicleModelInfo" /> about this <see cref="GtaVehicle" />.
        /// </summary>
        public VehicleModelInfo ModelInfo
        {
            get { return VehicleModelInfo.ForVehicle(this); }
        }

        /// <summary>
        ///     Gets the driver of this <see cref="GtaVehicle" />.
        /// </summary>
        public GtaPlayer Driver
        {
            get { return GtaPlayer.All.FirstOrDefault(p => p.Vehicle == this && p.VehicleSeat == 0); }
        }

        /// <summary>
        ///     Gets the passengers of this <see cref="GtaVehicle" />. (not the driver)
        /// </summary>
        public IEnumerable<GtaPlayer> Passengers
        {
            get { return GtaPlayer.All.Where(p => p.Vehicle == this).Where(player => player.VehicleSeat > 0); }
        }

        /// <summary>
        ///     Gets the ID of this <see cref="GtaVehicle" />.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        ///     Gets the size of the vehicles pool.
        /// </summary>
        public static int PoolSize
        {
            get { return Native.GetVehiclePoolSize(); }
        }

        #endregion

        #region Vehicles native properties

        /// <summary>
        ///     Gets whether this <see cref="GtaVehicle" /> has been created and still is alive.
        /// </summary>
        public virtual bool IsValid
        {
            get { return Native.IsValidVehicle(Id); }
        }

        /// <summary>
        ///     Gets or sets the Z angle of this <see cref="GtaVehicle" />.
        /// </summary>
        public virtual float Angle
        {
            get
            {
                float angle;
                Native.GetVehicleZAngle(Id, out angle);
                return angle;
            }
            set { Native.SetVehicleZAngle(Id, value); }
        }

        /// <summary>
        ///     Gets the model ID of this <see cref="GtaVehicle" />.
        /// </summary>
        public virtual VehicleModelType Model
        {
            get { return (VehicleModelType) Native.GetVehicleModel(Id); }
        }

        /// <summary>
        ///     Gets whether this <see cref="GtaVehicle" /> has a trailer attached to it.
        /// </summary>
        public virtual bool HasTrailer
        {
            get { return Native.IsTrailerAttachedToVehicle(Id); }
        }

        /// <summary>
        ///     Gets or sets the the trailer attached to this <see cref="GtaVehicle" />.
        /// </summary>
        /// <returns>The trailer attached.</returns>
        public virtual GtaVehicle Trailer
        {
            get
            {
                int id = Native.GetVehicleTrailer(Id);
                return id == 0 ? null : Find(id);
            }
            set
            {
                if (value == null)
                    Native.DetachTrailerFromVehicle(Id);
                else
                    Native.AttachTrailerToVehicle(value.Id, Id);
            }
        }

        /// <summary>
        ///     Gets or sets the velocity at which this <see cref="GtaVehicle" /> is moving.
        /// </summary>
        public virtual Vector3 Velocity
        {
            get
            {
                float x, y, z;
                Native.GetVehicleVelocity(Id, out x, out y, out z);
                return new Vector3(x, y, z);
            }
            set { Native.SetVehicleVelocity(Id, value.X, value.Y, value.Z); }
        }

        /// <summary>
        ///     Gets or sets the virtual world of this <see cref="GtaVehicle" />.
        /// </summary>
        public virtual int VirtualWorld
        {
            get { return Native.GetVehicleVirtualWorld(Id); }
            set { Native.SetVehicleVirtualWorld(Id, value); }
        }

        /// <summary>
        ///     Gets or sets this <see cref="GtaVehicle" />'s engine status. If True, the engine is running.
        /// </summary>
        public virtual bool Engine
        {
            get
            {
                bool value, misc;
                GetParameters(out value, out misc, out misc, out misc, out misc, out misc, out misc);
                return value;
            }
            set
            {
                VehicleParameterValue a, b, c, d, e, f, g;
                GetParameters(out a, out b, out c, out d, out e, out f, out g);
                SetParameters(value ? VehicleParameterValue.On : VehicleParameterValue.Off, b, c, d, e, f, g);
            }
        }

        /// <summary>
        ///     Gets or sets this <see cref="GtaVehicle" />'s lights' state. If True the lights are on.
        /// </summary>
        public virtual bool Lights
        {
            get
            {
                bool value, misc;
                GetParameters(out misc, out value, out misc, out misc, out misc, out misc, out misc);
                return value;
            }
            set
            {
                VehicleParameterValue a, b, c, d, e, f, g;
                GetParameters(out a, out b, out c, out d, out e, out f, out g);
                SetParameters(a, value ? VehicleParameterValue.On : VehicleParameterValue.Off, c, d, e, f, g);
            }
        }

        /// <summary>
        ///     Gets or sets this <see cref="GtaVehicle" />'s alarm state. If True the alarm is (or was) sounding.
        /// </summary>
        public virtual bool Alarm
        {
            get
            {
                bool value, misc;
                GetParameters(out misc, out misc, out value, out misc, out misc, out misc, out misc);
                return value;
            }
            set
            {
                VehicleParameterValue a, b, c, d, e, f, g;
                GetParameters(out a, out b, out c, out d, out e, out f, out g);
                SetParameters(a, b, value ? VehicleParameterValue.On : VehicleParameterValue.Off, d, e, f, g);
            }
        }

        /// <summary>
        ///     Gets or sets the lock status of the doors of this <see cref="GtaVehicle" />. If True the doors are locked.
        /// </summary>
        public virtual bool Doors
        {
            get
            {
                bool value, misc;
                GetParameters(out misc, out misc, out misc, out value, out misc, out misc, out misc);
                return value;
            }
            set
            {
                VehicleParameterValue a, b, c, d, e, f, g;
                GetParameters(out a, out b, out c, out d, out e, out f, out g);
                SetParameters(a, b, c, value ? VehicleParameterValue.On : VehicleParameterValue.Off, e, f, g);
            }
        }

        /// <summary>
        ///     Gets or sets the bonnet/hood status of this <see cref="GtaVehicle" />. If True, it's open.
        /// </summary>
        public virtual bool Bonnet
        {
            get
            {
                bool value, misc;
                GetParameters(out misc, out value, out misc, out misc, out value, out misc, out misc);
                return value;
            }
            set
            {
                VehicleParameterValue a, b, c, d, e, f, g;
                GetParameters(out a, out b, out c, out d, out e, out f, out g);
                SetParameters(a, b, c, d, value ? VehicleParameterValue.On : VehicleParameterValue.Off, f, g);
            }
        }

        /// <summary>
        ///     Gets or sets the boot/trunk status of this <see cref="GtaVehicle" />. True means it is open.
        /// </summary>
        public virtual bool Boot
        {
            get
            {
                bool value, misc;
                GetParameters(out misc, out value, out misc, out misc, out misc, out value, out misc);
                return value;
            }
            set
            {
                VehicleParameterValue a, b, c, d, e, f, g;
                GetParameters(out a, out b, out c, out d, out e, out f, out g);
                SetParameters(a, b, c, d, e, value ? VehicleParameterValue.On : VehicleParameterValue.Off, g);
            }
        }

        /// <summary>
        ///     Gets or sets the objective status of this <see cref="GtaVehicle" />. True means the objective is on.
        /// </summary>
        public virtual bool Objective
        {
            get
            {
                bool value, misc;
                GetParameters(out misc, out value, out misc, out misc, out misc, out misc, out value);
                return value;
            }
            set
            {
                VehicleParameterValue a, b, c, d, e, f, g;
                GetParameters(out a, out b, out c, out d, out e, out f, out g);
                SetParameters(a, b, c, d, e, f, value ? VehicleParameterValue.On : VehicleParameterValue.Off);
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the driver door is open.
        /// </summary>
        public virtual bool IsDriverDoorOpen
        {
            get
            {
                bool value, misc;
                GetDoorsParameters(out value, out misc, out misc, out misc);
                return value;
            }
            set
            {
                VehicleParameterValue a, b, c, d;
                GetDoorsParameters(out a, out b, out c, out d);
                SetDoorsParameters(value ? VehicleParameterValue.On : VehicleParameterValue.Off, b, c, d);
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the passenger door is open.
        /// </summary>
        public virtual bool IsPassengerDoorOpen
        {
            get
            {
                bool value, misc;
                GetDoorsParameters(out misc, out value, out misc, out misc);
                return value;
            }
            set
            {
                VehicleParameterValue a, b, c, d;
                GetDoorsParameters(out a, out b, out c, out d);
                SetDoorsParameters(a, value ? VehicleParameterValue.On : VehicleParameterValue.Off, c, d);
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the driver door is open.
        /// </summary>
        public virtual bool IsBackLeftDoorOpen
        {
            get
            {
                bool value, misc;
                GetDoorsParameters(out misc, out misc, out value, out misc);
                return value;
            }
            set
            {
                VehicleParameterValue a, b, c, d;
                GetDoorsParameters(out a, out b, out c, out d);
                SetDoorsParameters(a, b, value ? VehicleParameterValue.On : VehicleParameterValue.Off, d);
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the driver door is open.
        /// </summary>
        public virtual bool IsBackRightDoorOpen
        {
            get
            {
                bool value, misc;
                GetDoorsParameters(out misc, out misc, out misc, out value);
                return value;
            }
            set
            {
                VehicleParameterValue a, b, c, d;
                GetDoorsParameters(out a, out b, out c, out d);
                SetDoorsParameters(a, b, c, value ? VehicleParameterValue.On : VehicleParameterValue.Off);
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the driver window is closed.
        /// </summary>
        public virtual bool IsDriverWindowClosed
        {
            get
            {
                bool value, misc;
                GetWindowsParameters(out value, out misc, out misc, out misc);
                return value;
            }
            set
            {
                VehicleParameterValue a, b, c, d;
                GetWindowsParameters(out a, out b, out c, out d);
                SetWindowsParameters(value ? VehicleParameterValue.On : VehicleParameterValue.Off, b, c, d);
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the passenger window is closed.
        /// </summary>
        public virtual bool IsPassengerWindowClosed
        {
            get
            {
                bool value, misc;
                GetWindowsParameters(out misc, out value, out misc, out misc);
                return value;
            }
            set
            {
                VehicleParameterValue a, b, c, d;
                GetWindowsParameters(out a, out b, out c, out d);
                SetWindowsParameters(a, value ? VehicleParameterValue.On : VehicleParameterValue.Off, c, d);
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the driver window is closed.
        /// </summary>
        public virtual bool IsBackLeftWindowClosed
        {
            get
            {
                bool value, misc;
                GetWindowsParameters(out misc, out misc, out value, out misc);
                return value;
            }
            set
            {
                VehicleParameterValue a, b, c, d;
                GetWindowsParameters(out a, out b, out c, out d);
                SetWindowsParameters(a, b, value ? VehicleParameterValue.On : VehicleParameterValue.Off, d);
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the driver window is closed.
        /// </summary>
        public virtual bool IsBackRightWindowClosed
        {
            get
            {
                bool value, misc;
                GetWindowsParameters(out misc, out misc, out misc, out value);
                return value;
            }
            set
            {
                VehicleParameterValue a, b, c, d;
                GetWindowsParameters(out a, out b, out c, out d);
                SetWindowsParameters(a, b, c, value ? VehicleParameterValue.On : VehicleParameterValue.Off);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this Vehicle's siren is on.
        /// </summary>
        public virtual bool IsSirenOn
        {
            get
            {
                AssertNotDisposed();
                return Native.GetVehicleParamsSirenState(Id) == 1;
            }
        }

        /// <summary>
        ///     Gets or sets the rotation of this <see cref="GtaVehicle" />.
        /// </summary>
        /// <remarks>
        ///     Only the Z angle can be set!
        /// </remarks>
        public virtual Vector3 Rotation
        {
            get { return new Vector3(0, 0, Angle); }
            set { Native.SetVehicleZAngle(Id, value.Z); }
        }

        /// <summary>
        ///     Gets or sets the health of this <see cref="GtaVehicle" />.
        /// </summary>
        public virtual float Health
        {
            get
            {
                float value;
                Native.GetVehicleHealth(Id, out value);
                return value;
            }
            set { Native.SetVehicleHealth(Id, value); }
        }

        /// <summary>
        ///     Gets or sets the position of this <see cref="GtaVehicle" />.
        /// </summary>
        public virtual Vector3 Position
        {
            get
            {
                float x, y, z;
                Native.GetVehiclePos(Id, out x, out y, out z);
                return new Vector3(x, y, z);
            }
            set { Native.SetVehiclePos(Id, value.X, value.Y, value.Z); }
        }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when the <see cref="OnSpawn" /> is being called.
        ///     This callback is called when <see cref="GtaVehicle" /> spawns.
        /// </summary>
        public event EventHandler<EventArgs> Spawn;

        /// <summary>
        ///     Occurs when the <see cref="OnDeath" /> is being called.
        ///     This callback is called when this <see cref="GtaVehicle" /> is destroyed - either by exploding or becoming
        ///     submerged in water.
        /// </summary>
        public event EventHandler<PlayerEventArgs> Died;

        /// <summary>
        ///     Occurs when the <see cref="OnPlayerEnter" /> is being called.
        ///     This callback is called when a <see cref="GtaPlayer" /> starts to enter this <see cref="GtaVehicle" />,
        ///     meaning the player is not in vehicle yet at the time this callback is called.
        /// </summary>
        public event EventHandler<EnterVehicleEventArgs> PlayerEnter;

        /// <summary>
        ///     Occurs when the <see cref="OnPlayerExit" /> is being called.
        ///     This callback is called when a <see cref="GtaPlayer" /> exits a <see cref="GtaVehicle" />.
        /// </summary>
        public event EventHandler<PlayerVehicleEventArgs> PlayerExit;

        /// <summary>
        ///     Occurs when the <see cref="OnMod" /> is being called.
        ///     This callback is called when this <see cref="GtaVehicle" /> is modded.
        /// </summary>
        public event EventHandler<VehicleModEventArgs> Mod;

        /// <summary>
        ///     Occurs when the <see cref="OnPaintjobApplied" /> is being called.
        ///     Called when a <see cref="GtaPlayer" /> changes the paintjob of this <see cref="GtaVehicle" /> (in a modshop).
        /// </summary>
        public event EventHandler<VehiclePaintjobEventArgs> PaintjobApplied;

        /// <summary>
        ///     Occurs when the <see cref="OnResprayed" /> is being called.
        ///     The callback name is deceptive, this callback is called when a <see cref="GtaPlayer" /> exits a mod shop with this
        ///     <see cref="GtaVehicle" />,
        ///     regardless of whether the vehicle's colors were changed, and is NEVER called for pay 'n' spray garages.
        /// </summary>
        public event EventHandler<VehicleResprayedEventArgs> Resprayed;

        /// <summary>
        ///     Occurs when the <see cref="OnDamageStatusUpdated" /> is being called.
        ///     This callback is called when a element of this <see cref="GtaVehicle" /> such as doors, tires, panels, or lights
        ///     get damaged.
        /// </summary>
        public event EventHandler<PlayerEventArgs> DamageStatusUpdated;

        /// <summary>
        ///     Occurs when the <see cref="OnUnoccupiedUpdate" /> is being called.
        ///     This callback is called everytime this <see cref="GtaVehicle" /> updates the server with their status while it is
        ///     unoccupied.
        /// </summary>
        public event EventHandler<UnoccupiedVehicleEventArgs> UnoccupiedUpdate;

        /// <summary>
        ///     Occurs when the <see cref="OnStreamIn" /> is being called.
        ///     Called when a <see cref="GtaVehicle" /> is streamed to a <see cref="GtaPlayer" />'s client.
        /// </summary>
        public event EventHandler<PlayerEventArgs> StreamIn;

        /// <summary>
        ///     Occurs when the <see cref="OnStreamOut" /> is being called.
        ///     This callback is called when a <see cref="GtaVehicle" /> is streamed out from some <see cref="GtaPlayer" />'s
        ///     client.
        /// </summary>
        public event EventHandler<PlayerEventArgs> StreamOut;

        /// <summary>
        ///     Occurs when the <see cref="OnTrailerUpdate" /> is being called.
        ///     This callback is called when a <see cref="GtaPlayer" /> sent a trailer update about this <see cref="GtaVehicle" />.
        /// </summary>
        public event EventHandler<TrailerEventArgs> TrailerUpdate;

        /// <summary>
        ///     Occurs when the <see cref="OnSirenStateChanged" /> is being called.
        ///     This callback is called when this <see cref="GtaVehicle" />'s siren is toggled.
        /// </summary>
        public event EventHandler<SirenStateEventArgs> SirenStateChanged;

        #endregion

        #region Vehicles natives

        /// <summary>
        ///     This function can be used to calculate the distance (as a float) between this <see cref="GtaVehicle" /> and another
        ///     map coordinate.
        ///     This can be useful to detect how far a vehicle away is from a location.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>A float containing the distance from the point specified in the coordinates.</returns>
        public virtual float GetDistanceFromPoint(Vector3 point)
        {
            AssertNotDisposed();

            return Native.GetVehicleDistanceFromPoint(Id, point.X, point.Y, point.Z);
        }

        /// <summary>
        ///     Creates a <see cref="GtaVehicle" /> in the world.
        /// </summary>
        /// <param name="vehicletype">The model for the vehicle.</param>
        /// <param name="position">The coordinates for the vehicle.</param>
        /// <param name="rotation">The facing angle for the vehicle.</param>
        /// <param name="color1">The primary color ID.</param>
        /// <param name="color2">The secondary color ID.</param>
        /// <param name="respawnDelay">
        ///     The delay until the car is respawned without a driver in seconds. Using -1 will prevent the
        ///     vehicle from respawning.
        /// </param>
        /// <param name="addAlarm">If true, enables the vehicle to have a siren, providing the vehicle has a horn.</param>
        /// <returns> The <see cref="GtaVehicle" /> created.</returns>
        public static GtaVehicle Create(int vehicletype, Vector3 position, float rotation, int color1, int color2,
            int respawnDelay = -1, bool addAlarm = false)
        {
            int id = new[] {449, 537, 538, 569, 570, 590}.Contains(vehicletype)
                ? Native.AddStaticVehicleEx(vehicletype, position.X, position.Y, position.Z, rotation, color1, color2,
                    respawnDelay, addAlarm)
                : Native.CreateVehicle(vehicletype, position.X, position.Y, position.Z, rotation, color1, color2,
                    respawnDelay, addAlarm);

            return id == InvalidId ? null : FindOrCreate(id);
        }

        /// <summary>
        ///     Creates a <see cref="GtaVehicle" /> in the world.
        /// </summary>
        /// <param name="vehicletype">The model for the vehicle.</param>
        /// <param name="position">The coordinates for the vehicle.</param>
        /// <param name="rotation">The facing angle for the vehicle.</param>
        /// <param name="color1">The primary color ID.</param>
        /// <param name="color2">The secondary color ID.</param>
        /// <param name="respawnDelay">
        ///     The delay until the car is respawned without a driver in seconds. Using -1 will prevent the
        ///     vehicle from respawning.
        /// </param>
        /// <param name="addAlarm">If true, enables the vehicle to have a siren, providing the vehicle has a horn.</param>
        /// <returns> The <see cref="GtaVehicle" /> created.</returns>
        public static GtaVehicle Create(VehicleModelType vehicletype, Vector3 position, float rotation, int color1,
            int color2,
            int respawnDelay = -1, bool addAlarm = false)
        {
            return Create((int) vehicletype, position, rotation, color1, color2, respawnDelay, addAlarm);
        }

        /// <summary>
        ///     Creates a static <see cref="GtaVehicle" /> in the world.
        /// </summary>
        /// <param name="vehicletype">The model for the vehicle.</param>
        /// <param name="position">The coordinates for the vehicle.</param>
        /// <param name="rotation">The facing angle for the vehicle.</param>
        /// <param name="color1">The primary color ID.</param>
        /// <param name="color2">The secondary color ID.</param>
        /// <param name="respawnDelay">
        ///     The delay until the car is respawned without a driver in seconds. Using -1 will prevent the
        ///     vehicle from respawning.
        /// </param>
        /// <param name="addAlarm">If true, enables the vehicle to have a siren, providing the vehicle has a horn.</param>
        /// <returns> The <see cref="GtaVehicle" /> created.</returns>
        public static GtaVehicle CreateStatic(int vehicletype, Vector3 position, float rotation, int color1, int color2,
            int respawnDelay, bool addAlarm = false)
        {
            int id = Native.AddStaticVehicleEx(vehicletype, position.X, position.Y, position.Z, rotation, color1, color2,
                respawnDelay, addAlarm);

            return id == InvalidId ? null : FindOrCreate(id);
        }

        /// <summary>
        ///     Creates a static <see cref="GtaVehicle" /> in the world.
        /// </summary>
        /// <param name="vehicletype">The model for the vehicle.</param>
        /// <param name="position">The coordinates for the vehicle.</param>
        /// <param name="rotation">The facing angle for the vehicle.</param>
        /// <param name="color1">The primary color ID.</param>
        /// <param name="color2">The secondary color ID.</param>
        /// <returns> The <see cref="GtaVehicle" /> created.</returns>
        public static GtaVehicle CreateStatic(int vehicletype, Vector3 position, float rotation, int color1, int color2)
        {
            int id = Native.AddStaticVehicle(vehicletype, position.X, position.Y, position.Z, rotation, color1, color2);

            return id == InvalidId ? null : FindOrCreate(id);
        }

        /// <summary>
        ///     Checks if this <see cref="GtaVehicle" /> is streamed in for a <see cref="GtaPlayer" />.
        /// </summary>
        /// <param name="forPlayer">The Player to check.</param>
        /// <returns>True if this vehicle is streamed in for the specified vehicle; False otherwise.</returns>
        public virtual bool IsStreamedIn(GtaPlayer forPlayer)
        {
            AssertNotDisposed();

            return Native.IsVehicleStreamedIn(Id, forPlayer.Id);
        }

        /// <summary>
        ///     Returns this <see cref="GtaVehicle" />'s rotation on all axis as a quaternion.
        /// </summary>
        /// <param name="w">A float variable in which to store the first quaternion angle, passed by reference.</param>
        /// <param name="x">A float variable in which to store the second quaternion angle, passed by reference.</param>
        /// <param name="y">A float variable in which to store the third quaternion angle, passed by reference.</param>
        /// <param name="z">A float variable in which to store the fourth quaternion angle, passed by reference.</param>
        public virtual void GetRotationQuat(out float w, out float x, out float y,
            out float z)
        {
            AssertNotDisposed();

            Native.GetVehicleRotationQuat(Id, out w, out x, out y, out z);
        }

        /// <summary>
        ///     Set the parameters of this <see cref="GtaVehicle" /> for a <see cref="GtaPlayer" />.
        /// </summary>
        /// <param name="player">The <see cref="GtaPlayer" /> to set this vehicles's parameters for.</param>
        /// <param name="objective">False to disable the objective or True to show it.</param>
        /// <param name="doorslocked">False to unlock the doors or True to lock them.</param>
        public virtual void SetParametersForPlayer(GtaPlayer player, bool objective,
            bool doorslocked)
        {
            AssertNotDisposed();

            if (player == null)
                throw new ArgumentNullException("player");

            Native.SetVehicleParamsForPlayer(Id, player.Id, objective, doorslocked);
        }

        /// <summary>
        ///     Use this function before any player connects (<see cref="BaseMode.OnInitialized" />) to tell all clients that the
        ///     script will control vehicle engines and lights. This prevents the game automatically turning the engine on/off when
        ///     players enter/exit vehicles and headlights automatically coming on when it is dark.
        /// </summary>
        public static void ManualEngineAndLights()
        {
            Native.ManualVehicleEngineAndLights();
        }

        /// <summary>
        ///     Sets this <see cref="GtaVehicle" />'s parameters for all players.
        /// </summary>
        /// <param name="engine">Toggle the engine status on or off.</param>
        /// <param name="lights">Toggle the lights on or off.</param>
        /// <param name="alarm">Toggle the vehicle alarm on or off.</param>
        /// <param name="doors">Toggle the lock status of the doors.</param>
        /// <param name="bonnet">Toggle the bonnet to be open or closed.</param>
        /// <param name="boot">Toggle the boot to be open or closed.</param>
        /// <param name="objective">Toggle the objective status for the vehicle on or off.</param>
        public virtual void SetParameters(bool engine, bool lights, bool alarm, bool doors, bool bonnet, bool boot,
            bool objective)
        {
            AssertNotDisposed();

            Native.SetVehicleParamsEx(Id, engine ? 1 : 0, lights ? 1 : 0, alarm ? 1 : 0, doors ? 1 : 0, bonnet ? 1 : 0,
                boot ? 1 : 0, objective ? 1 : 0);
        }

        /// <summary>
        ///     Sets this <see cref="GtaVehicle" />'s parameters for all players.
        /// </summary>
        /// <param name="engine">Toggle the engine status on or off.</param>
        /// <param name="lights">Toggle the lights on or off.</param>
        /// <param name="alarm">Toggle the vehicle alarm on or off.</param>
        /// <param name="doors">Toggle the lock status of the doors.</param>
        /// <param name="bonnet">Toggle the bonnet to be open or closed.</param>
        /// <param name="boot">Toggle the boot to be open or closed.</param>
        /// <param name="objective">Toggle the objective status for the vehicle on or off.</param>
        public virtual void SetParameters(VehicleParameterValue engine, VehicleParameterValue lights,
            VehicleParameterValue alarm, VehicleParameterValue doors, VehicleParameterValue bonnet,
            VehicleParameterValue boot,
            VehicleParameterValue objective)
        {
            AssertNotDisposed();

            Native.SetVehicleParamsEx(Id, (int) engine, (int) lights, (int) alarm, (int) doors, (int) bonnet, (int) boot,
                (int) objective);
        }

        /// <summary>
        ///     Gets this <see cref="GtaVehicle" />'s parameters.
        /// </summary>
        /// <param name="engine">Get the engine status. If on the engine is running.</param>
        /// <param name="lights">Get the vehicle's lights' state. If on the lights are on.</param>
        /// <param name="alarm">Get the vehicle's alarm state. If on the alarm is (or was) sounding.</param>
        /// <param name="doors">Get the lock status of the doors. If on the doors are locked.</param>
        /// <param name="bonnet">Get the bonnet/hood status. If on it is open.</param>
        /// <param name="boot">Get the boot/trunk status. If on it is open.</param>
        /// <param name="objective">Get the objective status. If on the objective is on.</param>
        public virtual void GetParameters(out VehicleParameterValue engine, out VehicleParameterValue lights,
            out VehicleParameterValue alarm, out VehicleParameterValue doors, out VehicleParameterValue bonnet,
            out VehicleParameterValue boot, out VehicleParameterValue objective)
        {
            AssertNotDisposed();

            int tmpEngine, tmpLights, tmpAlarm, tmpDoors, tmpBonnet, tmpBoot, tmpObjective;
            Native.GetVehicleParamsEx(Id, out tmpEngine, out tmpLights, out tmpAlarm, out tmpDoors, out tmpBonnet,
                out tmpBoot, out tmpObjective);

            engine = (VehicleParameterValue) tmpEngine;
            lights = (VehicleParameterValue) tmpLights;
            alarm = (VehicleParameterValue) tmpAlarm;
            doors = (VehicleParameterValue) tmpDoors;
            bonnet = (VehicleParameterValue) tmpBonnet;
            boot = (VehicleParameterValue) tmpBoot;
            objective = (VehicleParameterValue) tmpObjective;
        }

        /// <summary>
        ///     Gets this <see cref="GtaVehicle" />'s parameters.
        /// </summary>
        /// <param name="engine">Get the engine status. If true the engine is running.</param>
        /// <param name="lights">Get the vehicle's lights' state. If true the lights are on.</param>
        /// <param name="alarm">Get the vehicle's alarm state. If true the alarm is (or was) sounding.</param>
        /// <param name="doors">Get the lock status of the doors. If true the doors are locked.</param>
        /// <param name="bonnet">Get the bonnet/hood status. If true it is open.</param>
        /// <param name="boot">Get the boot/trunk status. If true it is open.</param>
        /// <param name="objective">Get the objective status. If true the objective is on.</param>
        public virtual void GetParameters(out bool engine, out bool lights, out bool alarm,
            out bool doors, out bool bonnet, out bool boot, out bool objective)
        {
            VehicleParameterValue tmpEngine, tmpLights, tmpAlarm, tmpDoors, tmpBonnet, tmpBoot, tmpObjective;
            GetParameters(out tmpEngine, out tmpLights, out tmpAlarm, out tmpDoors, out tmpBonnet, out tmpBoot,
                out tmpObjective);

            engine = tmpEngine.ToBool();
            lights = tmpLights.ToBool();
            alarm = tmpAlarm.ToBool();
            doors = tmpDoors.ToBool();
            bonnet = tmpBonnet.ToBool();
            boot = tmpBoot.ToBool();
            objective = tmpObjective.ToBool();
        }

        /// <summary>
        ///     Sets the doors parameters.
        /// </summary>
        /// <param name="driver">if set to <c>true</c> the driver side door is open.</param>
        /// <param name="passenger">if set to <c>true</c> the passenger side door is open.</param>
        /// <param name="backleft">if set to <c>true</c> the backleft door is open.</param>
        /// <param name="backright">if set to <c>true</c> the backright door is open.</param>
        public virtual void SetDoorsParameters(bool driver, bool passenger, bool backleft, bool backright)
        {
            AssertNotDisposed();

            Native.SetVehicleParamsCarDoors(Id, driver ? 1 : 0, passenger ? 1 : 0, backleft ? 1 : 0, backright ? 1 : 0);
        }

        /// <summary>
        ///     Sets the doors parameters.
        /// </summary>
        /// <param name="driver">if on the driver side door is open.</param>
        /// <param name="passenger">if on the passenger side door is open.</param>
        /// <param name="backleft">if on the backleft door is open.</param>
        /// <param name="backright">if on the backright door is open.</param>
        public virtual void SetDoorsParameters(VehicleParameterValue driver, VehicleParameterValue passenger,
            VehicleParameterValue backleft, VehicleParameterValue backright)
        {
            AssertNotDisposed();

            Native.SetVehicleParamsCarDoors(Id, (int) driver, (int) passenger, (int) backleft, (int) backright);
        }

        /// <summary>
        ///     Gets the doors parameters.
        /// </summary>
        /// <param name="driver">if on the driver side door is open.</param>
        /// <param name="passenger">if on the passenger side door is open.</param>
        /// <param name="backleft">if on the backleft door is open.</param>
        /// <param name="backright">if on the backright door is open.</param>
        public virtual void GetDoorsParameters(out VehicleParameterValue driver, out VehicleParameterValue passenger,
            out VehicleParameterValue backleft, out VehicleParameterValue backright)
        {
            AssertNotDisposed();

            int tmpDriver, tmpPassenger, tmpBackleft, tmpBackright;
            Native.GetVehicleParamsCarDoors(Id, out tmpDriver, out tmpPassenger, out tmpBackleft, out tmpBackright);

            driver = (VehicleParameterValue) tmpDriver;
            passenger = (VehicleParameterValue) tmpPassenger;
            backleft = (VehicleParameterValue) tmpBackleft;
            backright = (VehicleParameterValue) tmpBackright;
        }

        /// <summary>
        ///     Gets the doors parameters.
        /// </summary>
        /// <param name="driver">if true the driver side door is open.</param>
        /// <param name="passenger">if true the passenger side door is open.</param>
        /// <param name="backleft">if true the backleft door is open.</param>
        /// <param name="backright">if true the backright door is open.</param>
        public virtual void GetDoorsParameters(out bool driver, out bool passenger, out bool backleft,
            out bool backright)
        {
            AssertNotDisposed();

            VehicleParameterValue tmpDriver, tmpPassenger, tmpBackleft, tmpBackright;
            GetDoorsParameters(out tmpDriver, out tmpPassenger, out tmpBackleft, out tmpBackright);

            driver = tmpDriver.ToBool();
            passenger = tmpPassenger.ToBool();
            backleft = tmpBackleft.ToBool();
            backright = tmpBackright.ToBool();
        }

        /// <summary>
        ///     Sets the windows parameters.
        /// </summary>
        /// <param name="driver">if set to <c>true</c> the driver side window is closed.</param>
        /// <param name="passenger">if set to <c>true</c> the passenger side window is closed.</param>
        /// <param name="backleft">if set to <c>true</c> the backleft window is closed.</param>
        /// <param name="backright">if set to <c>true</c> the backright window is closed.</param>
        public virtual void SetWindowsParameters(bool driver, bool passenger, bool backleft, bool backright)
        {
            AssertNotDisposed();

            Native.SetVehicleParamsCarWindows(Id, driver ? 1 : 0, passenger ? 1 : 0, backleft ? 1 : 0, backright ? 1 : 0);
        }

        /// <summary>
        ///     Sets the windows parameters.
        /// </summary>
        /// <param name="driver">if on the driver side window is closed.</param>
        /// <param name="passenger">if on the passenger side window is closed.</param>
        /// <param name="backleft">if on the backleft window is closed.</param>
        /// <param name="backright">if on the backright window is closed.</param>
        public virtual void SetWindowsParameters(VehicleParameterValue driver, VehicleParameterValue passenger,
            VehicleParameterValue backleft, VehicleParameterValue backright)
        {
            AssertNotDisposed();

            Native.SetVehicleParamsCarWindows(Id, (int) driver, (int) passenger, (int) backleft, (int) backright);
        }

        /// <summary>
        ///     Gets the windows parameters.
        /// </summary>
        /// <param name="driver">if on the driver side window is closed.</param>
        /// <param name="passenger">if on the passenger side window is closed.</param>
        /// <param name="backleft">if on the backleft window is closed.</param>
        /// <param name="backright">if on the backright window is closed.</param>
        public virtual void GetWindowsParameters(out VehicleParameterValue driver, out VehicleParameterValue passenger,
            out VehicleParameterValue backleft, out VehicleParameterValue backright)
        {
            AssertNotDisposed();

            int tmpDriver, tmpPassenger, tmpBackleft, tmpBackright;
            Native.GetVehicleParamsCarWindows(Id, out tmpDriver, out tmpPassenger, out tmpBackleft, out tmpBackright);

            driver = (VehicleParameterValue) tmpDriver;
            passenger = (VehicleParameterValue) tmpPassenger;
            backleft = (VehicleParameterValue) tmpBackleft;
            backright = (VehicleParameterValue) tmpBackright;
        }

        /// <summary>
        ///     Gets the windows parameters.
        /// </summary>
        /// <param name="driver">if true the driver side window is closed.</param>
        /// <param name="passenger">if true the passenger side window is closed.</param>
        /// <param name="backleft">if true the backleft window is closed.</param>
        /// <param name="backright">if true the backright window is closed.</param>
        public virtual void GetWindowsParameters(out bool driver, out bool passenger, out bool backleft,
            out bool backright)
        {
            AssertNotDisposed();

            VehicleParameterValue tmpDriver, tmpPassenger, tmpBackleft, tmpBackright;
            GetWindowsParameters(out tmpDriver, out tmpPassenger, out tmpBackleft, out tmpBackright);

            driver = tmpDriver.ToBool(true); // unset is most commonly also closed
            passenger = tmpPassenger.ToBool(true);
            backleft = tmpBackleft.ToBool(true);
            backright = tmpBackright.ToBool(true);
        }

        /// <summary>
        ///     Sets this <see cref="GtaVehicle" /> back to the position at where it was created.
        /// </summary>
        public virtual void Respawn()
        {
            AssertNotDisposed();

            Native.SetVehicleToRespawn(Id);
        }

        /// <summary>
        ///     Links this <see cref="GtaVehicle" /> to the interior. This can be used for example for an arena/stadium.
        /// </summary>
        /// <param name="interiorid">Interior ID.</param>
        public virtual void LinkToInterior(int interiorid)
        {
            AssertNotDisposed();

            Native.LinkVehicleToInterior(Id, interiorid);
        }

        /// <summary>
        ///     Adds a 'component' (often referred to as a 'mod' (modification)) to this Vehicle.
        /// </summary>
        /// <param name="componentid">The ID of the component to add to the vehicle.</param>
        public virtual void AddComponent(int componentid)
        {
            AssertNotDisposed();

            Native.AddVehicleComponent(Id, componentid);
        }

        /// <summary>
        ///     Remove a component from the <see cref="GtaVehicle" />.
        /// </summary>
        /// <param name="componentid">ID of the component to remove.</param>
        public virtual void RemoveComponent(int componentid)
        {
            AssertNotDisposed();

            Native.RemoveVehicleComponent(Id, componentid);
        }

        /// <summary>
        ///     Change this <see cref="GtaVehicle" />'s primary and secondary colors.
        /// </summary>
        /// <param name="color1">The new vehicle's primary Color ID.</param>
        /// <param name="color2">The new vehicle's secondary Color ID.</param>
        public virtual void ChangeColor(int color1, int color2)
        {
            AssertNotDisposed();

            Native.ChangeVehicleColor(Id, color1, color2);
        }

        /// <summary>
        ///     Change this <see cref="GtaVehicle" />'s paintjob (for plain colors see <see cref="ChangeColor" />).
        /// </summary>
        /// <param name="paintjobid">The ID of the Paintjob to apply. Use 3 to remove a paintjob.</param>
        public virtual void ChangePaintjob(int paintjobid)
        {
            AssertNotDisposed();

            Native.ChangeVehiclePaintjob(Id, paintjobid);
        }

        /// <summary>
        ///     Set this <see cref="GtaVehicle" />'s numberplate, which supports olor embedding.
        /// </summary>
        /// <param name="numberplate">The text that should be displayed on the numberplate. Color Embedding> is supported.</param>
        public virtual void SetNumberPlate(string numberplate)
        {
            AssertNotDisposed();

            Native.SetVehicleNumberPlate(Id, numberplate);
        }

        /// <summary>
        ///     Retrieves the installed component ID from this <see cref="GtaVehicle" /> in a specific slot.
        /// </summary>
        /// <param name="slot">The component slot to check for components.</param>
        /// <returns>The ID of the component installed in the specified slot.</returns>
        public virtual int GetComponentInSlot(CarModType slot)
        {
            AssertNotDisposed();

            return Native.GetVehicleComponentInSlot(Id, (int) slot);
        }

        /// <summary>
        ///     Find out what type of component a certain ID is.
        /// </summary>
        /// <param name="componentid">The component ID to check.</param>
        /// <returns>The component slot ID of the specified component.</returns>
        public static int GetComponentType(int componentid)
        {
            return Native.GetVehicleComponentType(componentid);
        }

        /// <summary>
        ///     Fully repairs this <see cref="GtaVehicle" />, including visual damage (bumps, dents, scratches, popped tires etc.).
        /// </summary>
        public virtual void Repair()
        {
            AssertNotDisposed();

            Native.RepairVehicle(Id);
        }

        /// <summary>
        ///     Sets the angular velocity of this <see cref="GtaVehicle" />.
        /// </summary>
        /// <param name="velocity">The amount of velocity in the angular directions.</param>
        public virtual void SetVehicleAngularVelocity(Vector3 velocity)
        {
            AssertNotDisposed();

            Native.SetVehicleAngularVelocity(Id, velocity.X, velocity.Y, velocity.Z);
        }

        /// <summary>
        ///     Retrieve the damage statuses of this <see cref="GtaVehicle" />.
        /// </summary>
        /// <param name="panels">A variable to store the panel damage data in, passed by reference.</param>
        /// <param name="doors">A variable to store the door damage data in, passed by reference.</param>
        /// <param name="lights">A variable to store the light damage data in, passed by reference.</param>
        /// <param name="tires">A variable to store the tire damage data in, passed by reference.</param>
        public virtual void GetVehicleDamageStatus(out int panels, out int doors, out int lights, out int tires)
        {
            AssertNotDisposed();

            Native.GetVehicleDamageStatus(Id, out panels, out doors, out lights, out tires);
        }

        /// <summary>
        ///     Sets the various visual damage statuses of this <see cref="GtaVehicle" />, such as popped tires, broken lights and
        ///     damaged panels.
        /// </summary>
        /// <param name="panels">A set of bits containing the panel damage status.</param>
        /// <param name="doors">A set of bits containing the door damage status.</param>
        /// <param name="lights">A set of bits containing the light damage status.</param>
        /// <param name="tires">A set of bits containing the tire damage status.</param>
        public virtual void UpdateVehicleDamageStatus(int panels, int doors, int lights, int tires)
        {
            AssertNotDisposed();

            Native.UpdateVehicleDamageStatus(Id, panels, doors, lights, tires);
        }

        /// <summary>
        ///     Retrieve information about a specific vehicle model such as the size or position of seats.
        /// </summary>
        /// <param name="model">The vehicle model to get info of.</param>
        /// <param name="infotype">The type of information to retrieve.</param>
        /// <returns>The offset vector.</returns>
        public static Vector3 GetVehicleModelInfo(VehicleModelType model, VehicleModelInfoType infotype)
        {
            float x, y, z;
            Native.GetVehicleModelInfo((int) model, (int) infotype, out x, out y, out z);
            return new Vector3(x, y, z);
        }

        /// <summary>
        ///     Removes this instance from the pool.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (IsValid)
                Native.DestroyVehicle(Id);
        }

        #endregion

        #region Event raisers

        /// <summary>
        ///     Raises the <see cref="Spawn" /> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs" /> that contains the event data. </param>
        public virtual void OnSpawn(EventArgs e)
        {
            if (Spawn != null)
                Spawn(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="Died" /> event.
        /// </summary>
        /// <param name="e">An <see cref="PlayerEventArgs" /> that contains the event data. </param>
        public virtual void OnDeath(PlayerEventArgs e)
        {
            if (Died != null)
                Died(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="PlayerEnter" /> event.
        /// </summary>
        /// <param name="e">An <see cref="EnterVehicleEventArgs" /> that contains the event data. </param>
        public virtual void OnPlayerEnter(EnterVehicleEventArgs e)
        {
            if (PlayerEnter != null)
                PlayerEnter(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="PlayerExit" /> event.
        /// </summary>
        /// <param name="e">An <see cref="PlayerVehicleEventArgs" /> that contains the event data. </param>
        public virtual void OnPlayerExit(PlayerVehicleEventArgs e)
        {
            if (PlayerExit != null)
                PlayerExit(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="Mod" /> event.
        /// </summary>
        /// <param name="e">An <see cref="VehicleModEventArgs" /> that contains the event data. </param>
        public virtual void OnMod(VehicleModEventArgs e)
        {
            if (Mod != null)
                Mod(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="PaintjobApplied" /> event.
        /// </summary>
        /// <param name="e">An <see cref="VehiclePaintjobEventArgs" /> that contains the event data. </param>
        public virtual void OnPaintjobApplied(VehiclePaintjobEventArgs e)
        {
            if (PaintjobApplied != null)
                PaintjobApplied(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="Resprayed" /> event.
        /// </summary>
        /// <param name="e">An <see cref="VehicleResprayedEventArgs" /> that contains the event data. </param>
        public virtual void OnResprayed(VehicleResprayedEventArgs e)
        {
            if (Resprayed != null)
                Resprayed(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="DamageStatusUpdated" /> event.
        /// </summary>
        /// <param name="e">An <see cref="PlayerEventArgs" /> that contains the event data. </param>
        public virtual void OnDamageStatusUpdated(PlayerEventArgs e)
        {
            if (DamageStatusUpdated != null)
                DamageStatusUpdated(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="UnoccupiedUpdate" /> event.
        /// </summary>
        /// <param name="e">An <see cref="UnoccupiedVehicleEventArgs" /> that contains the event data. </param>
        public virtual void OnUnoccupiedUpdate(UnoccupiedVehicleEventArgs e)
        {
            if (UnoccupiedUpdate != null)
                UnoccupiedUpdate(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="StreamIn" /> event.
        /// </summary>
        /// <param name="e">An <see cref="PlayerEventArgs" /> that contains the event data. </param>
        public virtual void OnStreamIn(PlayerEventArgs e)
        {
            if (StreamIn != null)
                StreamIn(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="StreamOut" /> event.
        /// </summary>
        /// <param name="e">An <see cref="PlayerEventArgs" /> that contains the event data. </param>
        public virtual void OnStreamOut(PlayerEventArgs e)
        {
            if (StreamOut != null)
                StreamOut(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="TrailerUpdate" /> event.
        /// </summary>
        /// <param name="args">An <see cref="PlayerVehicleEventArgs" /> that contains the event data. </param>
        public virtual void OnTrailerUpdate(TrailerEventArgs args)
        {
            if (TrailerUpdate != null)
                TrailerUpdate(this, args);
        }

        /// <summary>
        ///     Raises the <see cref="SirenStateChanged" /> event.
        /// </summary>
        /// <param name="args">The <see cref="SirenStateEventArgs" /> instance containing the event data.</param>
        public virtual void OnSirenStateChanged(SirenStateEventArgs args)
        {
            if (SirenStateChanged != null)
                SirenStateChanged(this, args);
        }

        #endregion
    }
}