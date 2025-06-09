using UnityEngine;
using UnityEditor;

public class ConicalCapsuleGenerator : EditorWindow
{
    private GameObject parentObject;
    private float bottomRadius = 2f;
    private float topRadius = 1f;
    private float height = 3f;
    private float capsuleRadius = 0.3f;
    private int segments = 12;
    private Axis symmetryAxis = Axis.Y;
    
    private enum Axis { X, Y, Z }

    [MenuItem("Tools/Conical Capsule Generator")]
    public static void ShowWindow()
    {
        GetWindow<ConicalCapsuleGenerator>("Conical Capsule Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Conical Capsule Settings", EditorStyles.boldLabel);

        parentObject = (GameObject)EditorGUILayout.ObjectField("Parent Object", parentObject, typeof(GameObject), true);
        bottomRadius = EditorGUILayout.FloatField("Bottom Radius", bottomRadius);
        topRadius = EditorGUILayout.FloatField("Top Radius", topRadius);
        height = EditorGUILayout.FloatField("Height", height);
        capsuleRadius = EditorGUILayout.FloatField("Capsule Radius", capsuleRadius);
        segments = EditorGUILayout.IntSlider("Segments", segments, 3, 36);
        symmetryAxis = (Axis)EditorGUILayout.EnumPopup("Symmetry Axis", symmetryAxis);

        if (GUILayout.Button("Generate Conical Capsules"))
        {
            GenerateConicalCapsules();
        }
    }

    private void GenerateConicalCapsules()
    {
        if (parentObject == null) return;

        // Clear old children
        while (parentObject.transform.childCount > 0)
        {
            DestroyImmediate(parentObject.transform.GetChild(0).gameObject);
        }

        float angleStep = 360f / segments;
        float slopeAngle = Mathf.Atan2(bottomRadius - topRadius, height) * Mathf.Rad2Deg;

        for (int i = 0; i < segments; i++)
        {
            float angle = i * angleStep;
            
            // Bottom position
            Vector3 bottomPos = GetCirclePoint(bottomRadius, angle, 0);
            
            // Top position
            Vector3 topPos = GetCirclePoint(topRadius, angle, height);
            
            // Capsule position (midpoint)
            Vector3 position = (bottomPos + topPos) / 2f;
            
            GameObject capsule = new GameObject($"Capsule_{i}");
            capsule.transform.SetParent(parentObject.transform);
            capsule.transform.localPosition = position;
            
            // Calculate direction and rotation
            Vector3 direction = (topPos - bottomPos).normalized;
            capsule.transform.localRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90f, 0f, 0f);

            CapsuleCollider col = capsule.AddComponent<CapsuleCollider>();
            col.height = (topPos - bottomPos).magnitude;
            col.radius = capsuleRadius;
            col.direction = 1; // Y-axis
            
            // Adjust width for perfect fit
            float avgRadius = (bottomRadius + topRadius) / 2f;
            float circumference = 2f * Mathf.PI * avgRadius;
            float optimalWidth = circumference / segments;
            capsule.transform.localScale = new Vector3(optimalWidth/(capsuleRadius*2f), 1f, 1f);
        }
    }

    private Vector3 GetCirclePoint(float radius, float angle, float heightOffset)
    {
        float angleRad = angle * Mathf.Deg2Rad;
        
        switch (symmetryAxis)
        {
            case Axis.X: 
                return new Vector3(heightOffset, 
                                Mathf.Sin(angleRad) * radius,
                                Mathf.Cos(angleRad) * radius);
                                
            case Axis.Y: 
                return new Vector3(Mathf.Sin(angleRad) * radius,
                                heightOffset,
                                Mathf.Cos(angleRad) * radius);
                                
            case Axis.Z: 
                return new Vector3(Mathf.Sin(angleRad) * radius,
                                Mathf.Cos(angleRad) * radius,
                                heightOffset);
                                
            default: return Vector3.zero;
        }
    }
}