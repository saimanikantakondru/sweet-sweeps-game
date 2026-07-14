using UnityEngine;

namespace SweetSweeps.Data
{
    [CreateAssetMenu(fileName = "WorldData", menuName = "SweetSweeps/WorldData")]
    public class WorldDataSO : ScriptableObject
    {
        [SerializeField] private string worldId;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite previewImage;
        [SerializeField] private int recordScore;

        public string WorldId => worldId;
        public string DisplayName => displayName;
        public Sprite PreviewImage => previewImage;
        public int RecordScore => recordScore;

#if UNITY_EDITOR
        public void SetRecordScore(int score) => recordScore = score;
#endif
    }
}