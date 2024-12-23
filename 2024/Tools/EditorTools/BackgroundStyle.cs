# if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates a GUIStyle with the background color set
/// </summary>
public static class BackgroundStyle
{
    // Allow for quick access to GUIStyle backgrounds
    private static Dictionary<Color, Texture2D> _colorCache = new Dictionary<Color, Texture2D>();

    public static GUIStyle Get(Color color)
    {
        GUIStyle style = new GUIStyle();
        Texture2D texture = null;
        if (!_colorCache.TryGetValue(color, out texture))
        {
            // Create Style
            texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            _colorCache.Add(color, texture);
        }
        style.normal.background = texture;
        return style;
    }

    public static GUIStyle GetBackground() 
    {
        float colorValue = EditorGUIUtility.isProSkin ? 0.22f : 0.76f; // grayscale color of lightmode and darkmode background
        return Get(new Color(colorValue, colorValue, colorValue));
    }
    public static GUIStyle GetLightBackground()
    {
        float colorValue = EditorGUIUtility.isProSkin ? 0.245f : 0.785f; // grayscale color of lightmode and darkmode background (slightly lighter)
        return Get(new Color(colorValue, colorValue, colorValue));
    }

    public static GUIStyle GetListBackground(int index) 
    {
        bool isLight = index % 2 == 0;
        return GetListBackground(isLight);
    }

    public static GUIStyle GetListBackground(bool isLight)
    {
        return isLight ? GetLightBackground() : GetBackground();
    }
}
#endif