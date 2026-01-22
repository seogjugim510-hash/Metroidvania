using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PivotChanger : EditorWindow
{
    [MenuItem("Tools/피벗 강제 일괄 변경")]
    public static void ChangePivots()
    {
        Object[] textures = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets);

        if (textures.Length == 0)
        {
            Debug.LogWarning("변경할 이미지를 프로젝트 창에서 먼저 선택해주세요!");
            return;
        }

        foreach (var tex in textures)
        {
            string path = AssetDatabase.GetAssetPath(tex);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer != null)
            {
                // 핵심: Multiple 모드인지 다시 확인
                importer.spriteImportMode = SpriteImportMode.Multiple;

                var sheet = importer.spritesheet;
                var newSheet = new List<SpriteMetaData>();

                for (int i = 0; i < sheet.Length; i++)
                {
                    var temp = sheet[i];

                    // 1. 커스텀 피벗으로 설정 (이게 가장 확실합니다)
                    temp.alignment = (int)SpriteAlignment.Custom;

                    // 2. 중앙(0.5, 0.5)으로 설정 (원하는 위치에 따라 수정)
                    temp.pivot = new Vector2(0.4f, 0.4f);

                    newSheet.Add(temp);
                }

                importer.spritesheet = newSheet.ToArray();

                // 3. 변경사항 강제 적용
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();

                // 4. 에셋 데이터베이스 갱신
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }
        Debug.Log("강제 피벗 변경 완료!");
    }
}