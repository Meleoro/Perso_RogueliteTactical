using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SkillData))]
public class SkillDataEditor : Editor
{
    private SerializedObject so;
    private GUIStyle moduleNameStyle = new GUIStyle();
    private GUIStyle titreStyle = new GUIStyle();
    private SkillData currentScript;

    public SerializedProperty skillName;
    public SerializedProperty skillDescription;
    public SerializedProperty skillType;
    public SerializedProperty skillEffects;
    public SerializedProperty skillPatern;
    public SerializedProperty skillAOEPatern;
    public SerializedProperty skillAOEPaternVertical;
    public SerializedProperty skillAOEPaternHorizontal;

    private int paternSize;


    private void OnEnable()
    {
        so = serializedObject;
        currentScript = target as SkillData;

        moduleNameStyle.fontSize = 14;
        titreStyle.fontSize = 12;

        moduleNameStyle.fontStyle = FontStyle.Bold;
        titreStyle.fontStyle = FontStyle.Bold;

        moduleNameStyle.normal.textColor = Color.white;
        titreStyle.normal.textColor = Color.white;

        skillName = so.FindProperty("skillName");
        skillDescription = so.FindProperty("skillDescription");
        skillType = so.FindProperty("skillType");
        skillEffects = so.FindProperty("skillEffects");
        skillPatern = so.FindProperty("skillPatern");
        skillAOEPatern = so.FindProperty("skillAOEPatern");
        skillAOEPaternVertical = so.FindProperty("skillAOEPaternVertical");
        skillAOEPaternHorizontal = so.FindProperty("skillAOEPaternHorizontal");

        paternSize = 15;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        so.Update();

        GUILayout.Space(20);

        EditorGUILayout.LabelField("Launch Skill Patern");

        GUILayout.Space(10);

        using (new GUILayout.VerticalScope(EditorStyles.helpBox, new[] { GUILayout.MinWidth(22 * paternSize) }))
        {
            for (int y = 0; y < paternSize; y++)
            {
                using (new GUILayout.HorizontalScope())
                {
                    for (int x = 0; x < paternSize; x++)
                    {
                        EditorGUILayout.PropertyField(skillPatern.GetArrayElementAtIndex((y * paternSize) + x), GUIContent.none, GUILayout.MinWidth(EditorGUIUtility.labelWidth - 350));
                    }
                }
            }
        }

        GUILayout.Space(20);

        EditorGUILayout.LabelField("AOE Patern");

        GUILayout.Space(10);

        using (new GUILayout.VerticalScope(EditorStyles.helpBox, new[] { GUILayout.MinWidth(22 * 9) }))
        {
            for (int y = 0; y < 9; y++)
            {
                using (new GUILayout.HorizontalScope())
                {
                    for (int x = 0; x < 9; x++)
                    {
                        EditorGUILayout.PropertyField(skillAOEPatern.GetArrayElementAtIndex((y * 9) + x), GUIContent.none, GUILayout.MinWidth(EditorGUIUtility.labelWidth - 350));
                    }
                }
            }
        }

        GUILayout.Space(20);

        EditorGUILayout.LabelField("AOE Patern Vertical");

        GUILayout.Space(10);

        using (new GUILayout.VerticalScope(EditorStyles.helpBox, new[] { GUILayout.MinWidth(22 * 9) }))
        {
            for (int y = 0; y < 9; y++)
            {
                using (new GUILayout.HorizontalScope())
                {
                    for (int x = 0; x < 9; x++)
                    {
                        EditorGUILayout.PropertyField(skillAOEPaternVertical.GetArrayElementAtIndex((y * 9) + x), GUIContent.none, GUILayout.MinWidth(EditorGUIUtility.labelWidth - 350));
                    }
                }
            }
        }

        GUILayout.Space(20);

        EditorGUILayout.LabelField("AOE Patern Horizontal");

        GUILayout.Space(10);

        using (new GUILayout.VerticalScope(EditorStyles.helpBox, new[] { GUILayout.MinWidth(22 * 9) }))
        {
            for (int y = 0; y < 9; y++)
            {
                using (new GUILayout.HorizontalScope())
                {
                    for (int x = 0; x < 9; x++)
                    {
                        EditorGUILayout.PropertyField(skillAOEPaternHorizontal.GetArrayElementAtIndex((y * 9) + x), GUIContent.none, GUILayout.MinWidth(EditorGUIUtility.labelWidth - 350));
                    }
                }
            }
        }

        so.ApplyModifiedProperties();
    }
}
