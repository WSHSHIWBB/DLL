using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CreatSpritePrefab : Editor
{
    private static string prefabDir = "Assets/Resources/Sprites/Prefabs/";
    [MenuItem("Tools/CreatSpritePrefab")]
    static private void CreatePrefab()
    {
        
        Object[] selectedObjects=Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
        for(int i=0;i<selectedObjects.Length;++i)
        {
            string parth = AssetDatabase.GetAssetPath(selectedObjects[i]);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(parth);
            GameObject go = new GameObject(sprite.name);
            go.AddComponent<SpriteRenderer>().sprite = sprite;
            string prefabParth = prefabDir + sprite.name + ".prefab";
            PrefabUtility.CreatePrefab(prefabParth, go);
            DestroyImmediate(go);
        }
       
    }
	
}
