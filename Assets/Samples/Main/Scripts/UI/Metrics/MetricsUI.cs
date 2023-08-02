using TMPro;
using UnityEngine;

namespace Prototypes.SamplesBRG.UI.Metrics
{
    public class MetricsUI : MonoBehaviour, IMetricsUI
    {
        [SerializeField] private TMP_Text entitiesText;
        [SerializeField] private TMP_Text batchesText;
        [SerializeField] private TMP_Text fpsText;

        private string[] samplesFps;
        private string[] samplesBatches;

        public void Initialize()
        {
            samplesFps = new string[4096];
            for (int i = 0; i < samplesFps.Length; i++)
            {
                samplesFps[i] = string.Format("FPS: {0}", i);
            }

            samplesBatches = new string[16384];
            for (int i = 0; i < samplesBatches.Length; i++)
            {
                samplesBatches[i] = string.Format("Batches: {0}", i);
            }
        }

        public void UpdateFpsCount(int count) => fpsText.text = samplesFps[count];

        public void UpdateBatchesCount(int count) => batchesText.text = samplesBatches[count];

        public void UpdateEntitiesCount(int count) => entitiesText.text = string.Format("Entities: {0}", count);
    }
}
