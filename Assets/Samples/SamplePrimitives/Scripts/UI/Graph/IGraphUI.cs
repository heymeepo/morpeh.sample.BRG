namespace Prototypes.SamplesBRG.PrimitivesSample.UI.Graph
{
    public interface IGraphUI
    {
        public void Initialize(int resMin, int resMax);
        public float GetResolutionSliderValue();
        public void UpdateResolution(int resolution);
    }
}