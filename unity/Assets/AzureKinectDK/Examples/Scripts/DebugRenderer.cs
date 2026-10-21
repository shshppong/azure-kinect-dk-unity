using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Azure.Kinect.Sensor.BodyTracking;

public class DebugRenderer : MonoBehaviour
{
    Device device;
    BodyTracker tracker;
    Skeleton skeleton;
    GameObject[] debugObjects;
    public Renderer renderer;

    private void Awake()
    {
        if (renderer == null)
            renderer = GetComponent<Renderer>();
    }

    private void OnEnable()
    {
        string stage = "opening device 0";
        try
        {
            this.device = Device.Open(0);
            var config = new DeviceConfiguration
            {
                ColorResolution = ColorResolution.r720p,
                ColorFormat = ImageFormat.ColorBGRA32,
                DepthMode = DepthMode.NFOV_Unbinned
            };
            stage = "starting cameras";
            device.StartCameras(config);

            stage = "reading calibration";
            var calibration = device.GetCalibration(config.DepthMode, config.ColorResolution);
            stage = "creating body tracker";
            var modelPath = System.IO.Path.Combine(Application.dataPath, "Plugins", "dnn_model_2_0_op11.onnx");
            if (!System.IO.File.Exists(modelPath))
                throw new System.IO.FileNotFoundException("Azure Kinect body tracking model not found.", modelPath);
            this.tracker = BodyTracker.Create(calibration, modelPath);
            stage = "creating joint objects";
            debugObjects = new GameObject[(int)JointId.Count];
            for (var i = 0; i < (int)JointId.Count; i++)
            {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = Enum.GetName(typeof(JointId), i);
                cube.transform.localScale = Vector3.one * 0.4f;
                debugObjects[i] = cube;
            }
            Debug.Log("Azure Kinect cameras and body tracker started.", this);
        }
        catch (Exception exception)
        {
            Debug.LogError($"Azure Kinect initialization failed while {stage}: {exception}", this);
            enabled = false;
            ReleaseResources();
        }
    }

    private void OnDisable()
    {
        ReleaseResources();
    }

    private void ReleaseResources()
    {
        try
        {
            if (tracker != null)
            {
                tracker.Dispose();
                tracker = null;
            }
        }
        finally
        {
            if (device != null)
            {
                device.Dispose();
                device = null;
            }
            if (debugObjects != null)
            {
                foreach (var obj in debugObjects)
                    if (obj != null) Destroy(obj);
                debugObjects = null;
            }
        }
    }

    void Update()
    {
        if (device == null || tracker == null)
            return;

        using (Capture capture = device.GetCapture())
        {
            tracker.EnqueueCapture(capture);
            var color = capture.Color;
            if (color != null && color.WidthPixels > 0 && renderer != null)
            {
                Texture2D tex = new Texture2D(color.WidthPixels, color.HeightPixels, TextureFormat.BGRA32, false);
                tex.LoadRawTextureData(color.GetBufferCopy());
                tex.Apply();
                renderer.material.mainTexture = tex;
            }
        }
        
        using (var frame = tracker.PopResult())
        {
            Debug.LogFormat("{0} bodies found.", frame.NumBodies);
            if (frame.NumBodies > 0)
            {
                var bodyId = frame.GetBodyId(0);
                Debug.LogFormat("bodyId={0}", bodyId);
                this.skeleton = frame.GetSkeleton(0);
                for (var i = 0; i < (int)JointId.Count; i++)
                {
                    var joint = this.skeleton.Joints[i];
                    var pos = joint.Position;
                    var rot = joint.Orientation;
                    var v = new Vector3(pos[0], -pos[1], pos[2]) * 0.004f;
                    var r = new Quaternion(rot[1], rot[2], rot[3], rot[0]);
                    var obj = debugObjects[i];
                    obj.transform.SetPositionAndRotation(v, r);
                }
                
            }
        }
    }
}
