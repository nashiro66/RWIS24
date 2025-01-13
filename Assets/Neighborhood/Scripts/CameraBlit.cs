using System;
using UnityEngine;
using UnityEngine.UI;

namespace Neighborhood
{
    public class CameraBlit : MonoBehaviour
    {
        public Camera left;
        public Camera right;
        public NetworkTestScript network;


        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            var rt = network.isServer ? left.targetTexture : right.targetTexture;
            Graphics.Blit(rt ?? source, destination);
        }
    }
}
