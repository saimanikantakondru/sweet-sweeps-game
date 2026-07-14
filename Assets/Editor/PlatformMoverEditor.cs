using UnityEditor;
using UnityEngine;
using SweetSweeps.Gameplay.Level;

namespace SweetSweeps.EditorTools
{
    [CustomEditor(typeof(PlatformMover))]
    public class PlatformMoverEditor : Sirenix.OdinInspector.Editor.OdinEditor
    {
        private void OnSceneGUI()
        {
            var mover = (PlatformMover)target;

            EditorGUI.BeginChangeCheck();

            Vector2 newA = Handles.PositionHandle(mover.PointA, Quaternion.identity);
            Vector2 newB = Handles.PositionHandle(mover.PointB, Quaternion.identity);

            Handles.color = new Color(0.5f, 0.6f, 1f);
            Handles.DrawDottedLine(newA, newB, 4f);

            Handles.Label(newA + Vector2.up * 0.5f, "A");
            Handles.Label(newB + Vector2.up * 0.5f, "B");

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(mover, "Move Platform Path Point");
                mover.SetPointA(newA);
                mover.SetPointB(newB);
                EditorUtility.SetDirty(mover);
            }
        }
    }
}
