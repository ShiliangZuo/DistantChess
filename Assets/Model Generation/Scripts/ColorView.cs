﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DistantChess {

    public class ColorView : MonoBehaviour {

        public GameObject multiSourceManagerObj;

        private MultiSourceManager multiSrcManager;

        // Use this for initialization
        void Start() {
            gameObject.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, 1));
        }

        // Update is called once per frame
        void Update() {
            multiSrcManager = multiSourceManagerObj.GetComponent<MultiSourceManager>();
            if (multiSrcManager == null) {
                Debug.Log("Cannot find multiScrManager ... !!!");
                return;
            }

            gameObject.GetComponent<Renderer>().material.mainTexture = multiSrcManager.GetColorTexture();
        }
    }
}
