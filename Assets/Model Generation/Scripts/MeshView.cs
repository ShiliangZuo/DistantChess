using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

namespace DistantChess {

    public class MeshView : MonoBehaviour {

        private KinectSensor kinectSensor;
        public GameObject multiSourceManagerObj;

        private Mesh mesh;
        private Vector3[] vertices;
        private Vector2[] UV;
        private int[] triangles;
        private const int downSampleSize = 4;
        private MultiSourceManager multiSrcManager;

        // Use this for initialization
        void Start() {
            kinectSensor = KinectSensor.GetDefault();
            if (kinectSensor != null) {
                var frameDesc = kinectSensor.DepthFrameSource.FrameDescription;
                CreateMesh(frameDesc.Width / downSampleSize, frameDesc.Height / downSampleSize);
                if (!kinectSensor.IsOpen) {
                    kinectSensor.Open();
                }
            }
            multiSrcManager = multiSourceManagerObj.GetComponent<MultiSourceManager>();
        }

        // Update is called once per frame
        void Update() {
            // Lets first try doing a greenscreen effect
            // TODO
            if (multiSrcManager == null) {
                Debug.Log("Can not find multiSrcManager ... !!!");
                return;
            }

        }

        void CreateMesh(int width, int height) {
            mesh = new Mesh();
            this.GetComponent<MeshFilter>().mesh = mesh;
            vertices = new Vector3[width * height];
            UV = new Vector2[width * height];
            triangles = new int[width * height * 6]; // Multiply by 6 cuz there're 2 triangles in each pixel, each 3 vertices

            int triangleIndex = 0;
            for (int y = 0; y < height; ++y) {
                for (int x = 0; x < width; ++x) {
                    int index = (y * width) + x;

                    vertices[index] = new Vector3(x, -y, 0);
                    UV[index] = new Vector2(((float)x / (float)width), ((float)y / (float)height));

                    // Skip the last row/col
                    if (x != (width - 1) && y != (height - 1)) {
                        int topLeft = index;
                        int topRight = topLeft + 1;
                        int bottomLeft = topLeft + width;
                        int bottomRight = bottomLeft + 1;

                        triangles[triangleIndex++] = topLeft;
                        triangles[triangleIndex++] = topRight;
                        triangles[triangleIndex++] = bottomLeft;
                        triangles[triangleIndex++] = bottomLeft;
                        triangles[triangleIndex++] = topRight;
                        triangles[triangleIndex++] = bottomRight;
                    }
                }
            }

            mesh.vertices = vertices;
            mesh.uv = UV;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
        }


    }

}
