using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[CustomEditor(typeof(MonopolyBoard))]
public class NodeSetEditor : Editor
{
    SerializedProperty nodeSetListProperty;

    private void OnEnable()
    {
        nodeSetListProperty = serializedObject.FindProperty("nodeSetList");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        MonopolyBoard monopolyBoard = (MonopolyBoard)target;
        EditorGUILayout.PropertyField(nodeSetListProperty,true);

        if (GUILayout.Button("改变图像颜色"))
        {
            Undo.RecordObject(monopolyBoard, "改变图像Colors");
            for (int i = 0; i < monopolyBoard.nodeSetList.Count; i++)
            {
                MonopolyBoard.NodeSet nodeSet = monopolyBoard.nodeSetList[i];

                for (int j = 0; j < nodeSet.nodeInSetList.Count; j++)
                {
                    MonopolyNode node = nodeSet.nodeInSetList[j];
                    Image image = node.propertyColoerField;
                    if (image != null)
                    {
                        Undo.RecordObject(image, "改变图像Color");
                        image.color = nodeSet.setColor;
                    }
                }
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}
