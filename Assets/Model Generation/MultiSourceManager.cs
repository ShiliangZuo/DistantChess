using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

namespace DistantChess {


    // A Class that manages all types of frame data,
    // including Depth, Color, BodyIndex
    public class MultiSourceManager : MonoBehaviour {

        public int colorWidth { get; private set; }
        public int colorHeight { get; private set; }

        private KinectSensor kinectSensor;
        private MultiSourceFrameReader multiSourceReader;
        private Texture2D colorTexture;
        private ushort[] depthData;
        private byte[] colorData;

	    // Use this for initialization
	    void Start () {
            kinectSensor = KinectSensor.GetDefault();
            if (kinectSensor != null) {
                multiSourceReader = kinectSensor.OpenMultiSourceFrameReader(
                    FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.BodyIndex);

                // Init fields related to ColorFrame
                var colorFrameDesc = kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
                colorWidth = colorFrameDesc.Width;
                colorHeight = colorFrameDesc.Height;
                colorTexture = new Texture2D(colorWidth, colorHeight, TextureFormat.RGBA32, false);
                colorData = new byte[colorFrameDesc.BytesPerPixel * colorFrameDesc.LengthInPixels];

                //Init fields related to DepthFrame
                var depthFrameDesc = kinectSensor.DepthFrameSource.FrameDescription;
                depthData = new ushort[depthFrameDesc.LengthInPixels];

                //Init fields related to BodyIndex
                // TODO


                if (kinectSensor.IsOpen == false) {
                    kinectSensor.Open();
                }
            }
	    }
	
	    // Update is called once per frame
	    void Update () {
            if (multiSourceReader != null) {
                var frame = multiSourceReader.AcquireLatestFrame();
                if (frame != null) {

                    // Update the colorFrame
                    var colorFrame = frame.ColorFrameReference.AcquireFrame();
                    if (colorFrame != null) {
                        colorFrame.CopyConvertedFrameDataToArray(colorData, ColorImageFormat.Rgba);
                        colorTexture.LoadRawTextureData(colorData);
                        colorTexture.Apply();
                        colorFrame.Dispose();
                        colorFrame = null;
                    }

                    // Update the depthFrame
                    var depthFrame = frame.DepthFrameReference.AcquireFrame();
                    if (depthFrame != null) {
                        depthFrame.CopyFrameDataToArray(depthData);
                        depthFrame.Dispose();
                        depthFrame = null;
                    }
                }
                frame = null;
            }
	    }

        void OnApplicationQuit() {
            if (multiSourceReader != null) {
                multiSourceReader.Dispose();
                multiSourceReader = null;
            }

            if (kinectSensor != null) {
                if (kinectSensor.IsOpen) {
                    kinectSensor.Close();
                }
                kinectSensor = null;
            }
        }
    }

}