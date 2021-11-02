using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SpriteAtlasTools
{
    [MenuItem("Assets/SpriteAtlas/Rename with txt file")]
    private static void RenameSpriteSlices()
    {
        Texture2D spriteAtlasTexture;
        if ((spriteAtlasTexture = (Selection.activeObject as Texture2D)) != null)
        {
            var texturePath = UnityEditor.AssetDatabase.GetAssetPath(spriteAtlasTexture);

            //Get txt file with sprite paths that was packed into atlas
            var dir = Path.GetDirectoryName(texturePath);
            var filename = Path.GetFileNameWithoutExtension(texturePath);
            var spriteAtlasContentPath = Path.Combine(dir, filename + ".txt");
            var spriteAtlasFileListAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(spriteAtlasContentPath);

            if (spriteAtlasFileListAsset != null)
            {
                //Prepare new sprite names
                var linesFromTextFile = spriteAtlasFileListAsset.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var spriteNames = linesFromTextFile.Select(packedSpritePath => Path.GetFileNameWithoutExtension(packedSpritePath)).ToArray();

                //Get sliced sprites
                var sprites = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(texturePath).OfType<Sprite>().ToArray();

                var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;

                // Rename the slices
                var spritesheet = importer.spritesheet;
                bool isMatchingSpritesheet = spritesheet != null && spritesheet.Length == spriteNames.Length;
                if (isMatchingSpritesheet)
                {
                    for (int i = 0; i < spritesheet.Length; i++)
                    {
                        spritesheet[i].name = spriteNames[i];
                    }
                    importer.spritesheet = spritesheet;

                    // Reimport the asset
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                }
            }
            else
            {
                Debug.LogError($"Txt file '{spriteAtlasContentPath}' with sprite list not found");
            }
        }
    }

    [MenuItem("Assets/SpriteAtlas/Rename with txt file", true)]
    private static bool RenameSpriteSlicesValidation()
    {
        Texture2D spriteAtlasTexture;
        if ((spriteAtlasTexture = (Selection.activeObject as Texture2D)) != null)
        {
            var texturePath = UnityEditor.AssetDatabase.GetAssetPath(spriteAtlasTexture);
            //Get sliced sprites
            var sprites = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(texturePath).OfType<Sprite>().ToArray();
            return sprites.Length > 0;
        }
        return false;
    }
}