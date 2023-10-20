using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using Tags;

namespace MyProject.Editor
{
  [InitializeOnLoad]
  public class TagLoader
  {
    static TagLoader()
    {
      Type type = typeof(UI);
      foreach (var field in type.GetFields())
      {
        var tag = field.GetValue(null);
        if (tag.ToString().Length == 0) continue;
        if (!InternalEditorUtility.tags.Contains(tag.ToString()))
        {
          InternalEditorUtility.AddTag(tag.ToString());
        }
      }
    }
  }
}