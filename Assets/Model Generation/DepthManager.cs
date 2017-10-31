using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class DepthManager : MonoBehaviour {

    private KinectSensor sensor;
    private DepthFrameReader depthReader;
    private ushort[] depthData;

    public ushort[] getDepthData() {
        return depthData;
    }

	// Use this for initialization
	void Start () {
        sensor = KinectSensor.GetDefault();

        if (sensor != null) {
            depthReader = sensor.DepthFrameSource.OpenReader();
            depthData = new ushort[sensor.DepthFrameSource.FrameDescription.LengthInPixels];
        }
	}
	
	// Update is called once per frame
	void Update () {
		if (depthReader != null) {

        }
	}
}
