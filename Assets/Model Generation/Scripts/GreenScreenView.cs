using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

namespace DistantChess {

    public class GreenScreenView : MonoBehaviour {

        public GameObject multiSourceManagerObj;

        private CoordinateMapper coordinateManager;

        private ComputeBuffer depthBuffer;
        private ComputeBuffer bodyIndexBuffer;

        DepthSpacePoint depthPoints;
        byte[] bodyIndexPoints;

        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {

        }
    }

}