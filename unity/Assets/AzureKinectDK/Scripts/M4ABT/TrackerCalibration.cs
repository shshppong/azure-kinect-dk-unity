// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Microsoft.Azure.Kinect.Sensor
{
    [StructLayout(LayoutKind.Sequential)]
    [Native.NativeReference("k4abt_tracker_configuration_t ")]
    public struct TrackerCalibration
    {

        public SensorOrientation sensor_orientation;

        // k4abt_tracker_configuration_t layout for Body Tracking SDK 1.1.x.
        public TrackerProcessingMode processing_mode;

        public int gpu_device_id;

        [MarshalAs(UnmanagedType.LPStr)]
        public string model_path;
    }

    public enum TrackerProcessingMode
    {
        Gpu = 0,
        Cpu = 1,
        Cuda = 2,
        TensorRT = 3,
        DirectML = 4
    }
}
