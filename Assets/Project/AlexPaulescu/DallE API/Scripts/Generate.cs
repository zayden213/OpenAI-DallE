using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Project.Scripts
{
    public class Generate : MonoBehaviour
    {
        private enum ImageSize
        {
            Small = 256,
            Medium = 512,
            Large = 1024
        }
        
        [SerializeField] private string _prompt;
        [SerializeField] private ImageSize _imageSize;
        private void Start()
        {
            var size = _imageSize switch
            {
                ImageSize.Small => "256x256",
                ImageSize.Medium => "512x512",
                ImageSize.Large => "1024x1024",
                _ => throw new ArgumentOutOfRangeException()
            };
            
            var dallEGenerator = GetComponent<DallEGenerator>();

            void AssignTextureCallback(Texture2D texture)
            {
                var rendererL = GetComponent<Renderer>();
                rendererL.material.mainTexture = texture;
            }
            
            var serializableParams = new DallEGenerator.DallEParamsHelper
            {
                prompt = _prompt,
                size = size
            };

            dallEGenerator.GetImage(serializableParams, AssignTextureCallback).Forget();
        }
    }
}