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
        private const double depthScale = 0.1f;

        private MultiSourceManager multiSrcManager;
        private CoordinateMapper mapper;

        private ComputeBuffer depthCoordsBuffer;
        private ComputeBuffer bodyIndexBuffer;

        DepthSpacePoint[] depthPoints;
        byte[] bodyIndexPoints;

        // Use this for initialization
        void Start() {
            kinectSensor = KinectSensor.GetDefault();
            if (kinectSensor != null) {
                var frameDesc = kinectSensor.DepthFrameSource.FrameDescription;
                mapper = kinectSensor.CoordinateMapper;
                CreateMesh(frameDesc.Width / downSampleSize, frameDesc.Height / downSampleSize);
                if (!kinectSensor.IsOpen) {
                    kinectSensor.Open();
                }
            }
            multiSrcManager = multiSourceManagerObj.GetComponent<MultiSourceManager>();

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
        }

        // Update is called once per frame
        void Update() {
            // TODO
            if (multiSrcManager == null) {
                Debug.Log("Can not find multiSrcManager ... !!!");
                return;
            }
            gameObject.GetComponent<Renderer>().material.mainTexture = multiSrcManager.GetColorTexture();
            RefreshData(multiSrcManager.GetDepthData(),
                multiSrcManager.colorWidth,
                multiSrcManager.colorHeight);

            //Depth and BodyIndex Buffer
            depthPoints = multiSrcManager.GetDepthCoords();
            bodyIndexPoints = multiSrcManager.GetBodyIndexData();

            depthCoordsBuffer.SetData(depthPoints);

            float[] buffer = new float[512 * 424];
            for (int i = 0; i < bodyIndexPoints.Length; ++i) {
                buffer[i] = (float)bodyIndexPoints[i];
            }
            bodyIndexBuffer.SetData(buffer);
            buffer = null;
        }

        void CreateMesh(int width, int height) {
            mesh = new Mesh();
            this.GetComponent<MeshFilter>().mesh = mesh;
            vertices = new Vector3[width * height];
            UV = new Vector2[width * height];
            triangles = new int[(width-1) * (height-1) * 6]; // Multiply by 6 cuz there're 2 triangles in each pixel, each 3 vertices

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

        void RefreshData(ushort[] depthData, int colorWidth, int colorHeight) {
            var frameDesc = kinectSensor.DepthFrameSource.FrameDescription;

            ColorSpacePoint[] colorSpace = new ColorSpacePoint[depthData.Length];
            mapper.MapDepthFrameToColorSpace(depthData, colorSpace);

            for (int y = 0; y < frameDesc.Height; y += downSampleSize) {
                for (int x = 0; x < frameDesc.Width; x += downSampleSize) {
                    int indexX = x / downSampleSize;
                    int indexY = y / downSampleSize;
                    int smallIndex = (indexY * (frameDesc.Width / downSampleSize)) + indexX;

                    double avg = GetAvg(depthData, x, y, frameDesc.Width, frameDesc.Height);

                    avg = avg * depthScale;

                    vertices[smallIndex].z = (float)avg;

                    // Update UV mapping with CDRP
                    var colorSpacePoint = colorSpace[(y * frameDesc.Width) + x];
                    UV[smallIndex] = new Vector2(colorSpacePoint.X / colorWidth, colorSpacePoint.Y / colorHeight);
                }
            }

            mesh.vertices = vertices;
            mesh.uv = UV;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
        }

        double GetAvg(ushort[] depthData, int x, int y, int width, int height) {
            double sum = 0.0;

            for (int y1 = y; y1 < y + 4; y1++) {
                for (int x1 = x; x1 < x + 4; x1++) {
                    int fullIndex = (y1 * width) + x1;

                    if (depthData[fullIndex] == 0)
                        sum += 4500;
                    else
                        sum += depthData[fullIndex];

                }
            }

            return sum / 16;
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
