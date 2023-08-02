using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Prototypes.SamplesBRG.PrimitivesSample.UI.Graph
{ 
    public sealed class GraphUI : MonoBehaviour, IGraphUI
    {
        [SerializeField] private TMP_Text graphResolutionText;
        [SerializeField] private Slider graphResolutionSlider;

        private string[] samplesResolution;

        public void Initialize(int resMin, int resMax)
        {
            graphResolutionSlider.minValue = resMin;
            graphResolutionSlider.maxValue = resMax;

            samplesResolution = new string[resMax];

            for (int i = 0; i < resMax; i++)
            {
                samplesResolution[i] = string.Format("Resolution: {0}", i);
            }
        }

        public float GetResolutionSliderValue() => graphResolutionSlider.value;

        public void UpdateResolution(int resolution)
        {
            graphResolutionSlider.value = resolution;
            graphResolutionText.text = samplesResolution[resolution];
        }
    }
}
