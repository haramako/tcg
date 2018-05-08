using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

// BEGIN-NO-TRANSLATE
public class SpriteSet : ScriptableObject
{
	public Sprite[] Sprites;

	Dictionary<string, Sprite> cache_ = new Dictionary<string, Sprite>();

	private void Awake()
	{
		if (Sprites == null)
		{
			return;
		}
		cache_ = Sprites.ToDictionary(s => s.name);
	}

	public Sprite Find(string name)
	{
		Sprite result;
		if( cache_.TryGetValue(name, out result))
		{
			return result;
		}
		return null;
	}

	public Sprite FindOrNull(string name)
	{
		Sprite result;
		if (cache_.TryGetValue(name, out result))
		{
			return result;
		}
		return null;
	}

	/// <summary>
	/// スプライトセットに存在するか
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public bool Exist(string name)
	{
		return (Sprites.FirstOrDefault(s => s.name == name) != null);
	}

	/// <summary>
	/// 補完ありFind
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public Sprite FindEx(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}

		var found = Sprites.FirstOrDefault(s => s.name == name);
		if (!found)
		{
			// 見つからなかったので向き番号を0にして試す
			var index = name.IndexOf("_");
			if (index != -1)
			{
				name = string.Format("{0}_0{1}", name.Substring(0, index), name.Substring(index + 2));
				found = Sprites.FirstOrDefault(s => s.name == name);
				if (!found)
				{
					// 見つからなかったので名前をmoveにして試す
					name = string.Format("move_0{0}", name.Substring(index + 2));
					found = Sprites.FirstOrDefault(s => s.name == name);
				}
			}
		}
		return found;
	}

	#if UNITY_EDITOR
	[MenuItem("Tools/CreateSpriteSet")]
	public static void Create()
	{
		try
		{
			AssetDatabase.StartAssetEditing();
			var objs = Selection.objects;
			foreach (var obj in Selection.objects)
			{
				var dir = obj as DefaultAsset;
				CreateFromDir (dir);
			}
		}
		finally
		{
			AssetDatabase.StopAssetEditing();
		}
	}

	public static void CreateFromDir(DefaultAsset dir)
	{
		var dirPath = AssetDatabase.GetAssetPath (dir.GetInstanceID ());

		// パッキングタグをつける
		var texGuids = AssetDatabase.FindAssets ("t:texture", new string[] { dirPath });
		var textures = texGuids
					   .Select (guid => AssetDatabase.GUIDToAssetPath (guid))
					   .Select (path => AssetDatabase.LoadAssetAtPath<Texture2D> (path))
					   .ToArray ();
		foreach (var tex in textures)
		{
			var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tex));
			var log = new System.Text.StringBuilder();
			if (importer.spriteImportMode != SpriteImportMode.Single)
			{
				importer.spriteImportMode = SpriteImportMode.Single;
				log.Append("spriteImportModeを設定しました。\n");
			}

			if (importer.spritePackingTag != dir.name.ToLowerInvariant())
			{
				importer.spritePackingTag = dir.name.ToLowerInvariant();
				log.Append("PackingTagを設定しました。\n");
			}
			if (importer.textureType != TextureImporterType.Sprite)
			{
				importer.textureType = TextureImporterType.Sprite;
				log.Append("importTypeを設定しました。");
			}

			if (importer.alphaIsTransparency == false)
			{
				importer.alphaIsTransparency = true;
				log.Append("alphaIsTransparencyを設定しました。");
			}

			if (importer.mipmapEnabled == true)
			{
				importer.mipmapEnabled = false;
				log.Append("mipmapEnabledを設定しました。");
			}

			if (importer.wrapMode != TextureWrapMode.Clamp)
			{
				importer.wrapMode = TextureWrapMode.Clamp;
				log.Append("wrapModeを設定しました。");
			}

			if (!string.IsNullOrEmpty(log.ToString()))
			{
				importer.SaveAndReimport();
			}

			Debug.Log(log.ToString());
		}

		var spriteGuids = AssetDatabase.FindAssets ("t:sprite", new string[] { dirPath });
		var sprites = spriteGuids
					  .Select (guid => AssetDatabase.GUIDToAssetPath (guid))
					  .Select (path => AssetDatabase.LoadAssetAtPath<Sprite> (path))
					  .ToArray ();

		var obj = ScriptableObject.CreateInstance<SpriteSet> ();
		obj.Sprites = sprites;
		AssetDatabase.CreateAsset (obj, dirPath + "/" + dir.name + ".asset");

		EditorUtility.SetDirty (obj);

	}
	#endif
}
// END-NO-TRANSLATE