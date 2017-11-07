using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

namespace DistantChess {

    public class GreenScreenView : MonoBehaviour {

        public GameObject multiSourceManagerObj;

        private MultiSourceManager multiSrcManager;

        private ComputeBuffer depthCoordsBuffer;
        private ComputeBuffer bodyIndexBuffer;

        DepthSpacePoint[] depthPoints;
        byte[] bodyIndexPoints;

        private int depthWidth;
        private int depthHeight;

        // Use this for initialization
        void Start() {
            ReleaseBuffers();

            multiSrcManager = multiSourceManagerObj.GetComponent<MultiSourceManager>();

            Texture2D renderTexture = multiSrcManager.GetColorTexture();
            if (renderTexture != null) {
                this.gameObject.GetComponent<Renderer>().material.SetTexture("_MainTex", renderTexture);
            }

            depthPoints = multiSrcManager.GetDepthCoords();
            if (depthPoints != null) {
                depthCoordsBuffer = new ComputeBuffer(depthPoints.Length, sizeof(float) * 2);
                this.gameObject.GetComponent<Renderer>().material.SetBuffer("depthCoordinates", depthCoordsBuffer);
            }

            bodyIndexPoints = multiSrcManager.GetBodyIndexData();
            if (bodyIndexPoints != null) {
                bodyIndexBuffer = new ComputeBuffer(bodyIndexPoints.Length, sizeof(float));
                this.gameObject.GetComponent<Renderer>().material.SetBuffer("bodyIndexBuffer", bodyIndexBuffer);
            }

            // TODO
            depthWidth = 512;
            depthHeight = 424;

        }

        // Update is called once per frame
        void Update() {

            gameObject.GetComponent<Renderer>().material.mainTexture = multiSrcManager.GetColorTexture();
            depthPoints = multiSrcManager.GetDepthCoords();
            bodyIndexPoints = multiSrcManager.GetBodyIndexData();

            depthCoordsBuffer.SetData(depthPoints);

            float[] buffer = new float[depthWidth * depthHeight];
            for (int i = 0; i < bodyIndexPoints.Length; ++i) {
                buffer[i] = (float)bodyIndexPoints[i];
            }
            bodyIndexBuffer.SetData(buffer);
            buffer = null;
        }

        private void ReleaseBuffers() {
            if (depthCoordsBuffer != null) depthCoordsBuffer.Release();
            depthCoordsBuffer = null;

            if (bodyIndexBuffer != null) bodyIndexBuffer.Release();
            bodyIndexBuffer = null;

            depthPoints = null;
            bodyIndexPoints = null;
        }

        void OnDisable() {
            ReleaseBuffers();
        }
    }

}
