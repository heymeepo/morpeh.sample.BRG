namespace Prototypes.SamplesBRG.UI.Metrics
{
    public interface IMetricsUI
    {
        public void Initialize();
        public void UpdateBatchesCount(int count);
        public void UpdateEntitiesCount(int count);
        public void UpdateFpsCount(int count);
    }
}