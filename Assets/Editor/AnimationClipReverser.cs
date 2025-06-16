using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

public class AnimationClipReverser : MonoBehaviour
{
    [MenuItem("Tools/Reverse Selected Animation Clip")]
    static void ReverseClip()
    {
        if (Selection.activeObject is AnimationClip originalClip)
        {
            string path = AssetDatabase.GetAssetPath(originalClip);
            string directory = Path.GetDirectoryName(path);
            string newName = originalClip.name + "_Reversed";

            AnimationClip reversedClip = new AnimationClip
            {
                frameRate = originalClip.frameRate
            };

            float clipLength = originalClip.length;

            foreach (var binding in AnimationUtility.GetCurveBindings(originalClip))
            {
                AnimationCurve originalCurve = AnimationUtility.GetEditorCurve(originalClip, binding);
                Keyframe[] reversedKeys = new Keyframe[originalCurve.keys.Length];

                for (int i = 0; i < originalCurve.keys.Length; i++)
                {
                    Keyframe originalKey = originalCurve.keys[i];
                    reversedKeys[i] = new Keyframe(
                        clipLength - originalKey.time,
                        originalKey.value,
                        -originalKey.outTangent,
                        -originalKey.inTangent
                    );
                }

                AnimationCurve reversedCurve = new AnimationCurve(reversedKeys.OrderBy(k => k.time).ToArray());
                reversedClip.SetCurve(binding.path, binding.type, binding.propertyName, reversedCurve);
            }

            string reversedPath = Path.Combine(directory, newName + ".anim");
            AssetDatabase.CreateAsset(reversedClip, reversedPath);
            AssetDatabase.SaveAssets();

            Debug.Log("Reversed animation created at: " + reversedPath);
        }
        else
        {
            Debug.LogWarning("Select an AnimationClip to reverse.");
        }
    }
}
