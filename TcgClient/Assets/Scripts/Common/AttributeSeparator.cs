using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>区切り線の色</summary>
public enum SeparatorColor
{
	Default = 0,

	Black = 0,
	Gray,
	White,
	Red,
	Orange,
	Yellow,
	YellowGreen,
	Green,
	Cyan,
	Blue,
	Magenta,
}

/// <summary>
/// インスペクタ内部で区切り線を引く
/// </summary>
[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
public class SeparatorAttribute : PropertyAttribute
{
	/// <summary>区切り線サイズ</summary>
	float lineSize = 1f;
	public float LineSize { get { return lineSize; } }

	/// <summary>題字</summary>
	string title = null;
	public string Title { get { return title; } }

	/// <summary>区切り線色</summary>
	SeparatorColor lineColor;
	public Color LineColor { get { return GetColor(lineColor, DefaultLineColor); } }

	/// <summary>フォント色</summary>
	SeparatorColor fontColor;
	public Color FontColor { get { return GetColor(fontColor, DefaultFontColor); } }

	/// <summary>フォントスタイル</summary>
	GUIStyle style = new GUIStyle();
	public GUIStyle GuiStyle { get { return style; } }

	/// <summary>題字サイズ</summary>
	public Vector2 TextSize
	{
		get { return GuiStyle.CalcSize(new GUIContent(title)); }
	}

	/// <summary>標準区切り線サイズ</summary>
	public const float DefaultLineSize = 1f;
	/// <summary>フォント太字</summary>
	const FontStyle defaultFontStyle = FontStyle.Bold;
	/// <summary>標準区切り色</summary>
	public static readonly Color DefaultLineColor = Color.gray;
	/// <summary>標準フォント色</summary>
	public static readonly Color DefaultFontColor = Color.black;


	public SeparatorAttribute(float size = DefaultLineSize, SeparatorColor lineCol = SeparatorColor.Default)
	{
		SetInfomation(null, SeparatorColor.Default, size, lineCol);
	}

	public SeparatorAttribute(SeparatorColor lineCol)
	{
		SetInfomation(null, SeparatorColor.Default, DefaultLineSize, lineCol);
	}

	public SeparatorAttribute(string title, float size = DefaultLineSize, SeparatorColor lineCol = SeparatorColor.Default)
	{
		SetInfomation(title, SeparatorColor.Default, size, lineCol);
	}

	public SeparatorAttribute(string title, SeparatorColor fontColor, float size = DefaultLineSize, SeparatorColor lineCol = SeparatorColor.Default)
	{
		SetInfomation(title, fontColor, size, lineCol);
	}

	/// <summary>
	/// 区切り線情報を設定
	/// </summary>
	void SetInfomation(string title, SeparatorColor fontColor, float size, SeparatorColor lineCol)
	{
		this.title = title;

		this.style.fontStyle = defaultFontStyle;
		this.fontColor = fontColor;
		var styleState = new GUIStyleState();
		styleState.textColor = FontColor;
		this.style.normal = styleState;

		this.lineSize = size;
		this.lineColor = lineCol;
	}

	/// <summary>
	/// SeparatorColor -> Color 変換
	/// </summary>
	Color GetColor(SeparatorColor selColor, Color defColor)
	{
		switch (selColor)
		{
			case SeparatorColor.White:
				return Color.white;
			case SeparatorColor.Red:
				return Color.red;
			case SeparatorColor.Orange:
				return new Color(1f, 0.5f, 0f);
			case SeparatorColor.Yellow:
				return Color.yellow;
			case SeparatorColor.YellowGreen:
				return new Color(0.5f, 1f, 0f);
			case SeparatorColor.Green:
				return Color.green;
			case SeparatorColor.Cyan:
				return Color.cyan;
			case SeparatorColor.Blue:
				return Color.blue;
			case SeparatorColor.Magenta:
				return Color.magenta;
			case SeparatorColor.Black:
				return Color.black;
			case SeparatorColor.Gray:
				return Color.grey;
			default:
				return defColor;
		}
	}
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SeparatorAttribute))]
public class SeparatorDrawer : DecoratorDrawer
{
	/// <summary>テキストと区切りの余白</summary>
	const float spc = 2f;

	/// <summary>属性情報</summary>
	SeparatorAttribute spAttr
	{
		get { return (SeparatorAttribute)attribute; }
	}

	public override void OnGUI(Rect position)
	{
		Color lineColor = spAttr.LineColor;
		if (string.IsNullOrEmpty(spAttr.Title))
		{
			Rect line = position;
			line.yMin += (GetHeight() - spAttr.LineSize) / 2f;
			line.height = spAttr.LineSize;
			EditorGUI.DrawRect(line, lineColor);
		}
		else
		{
			// 区切りの幅
			float lineWidth = (position.width - spAttr.TextSize.x) / 2.0f - spc;

			// 左ライン
			Rect line = position;
			line.yMin += (GetHeight() - spAttr.LineSize) / 2f;
			line.width = lineWidth;
			line.height = spAttr.LineSize;
			EditorGUI.DrawRect(line, lineColor);

			// テキスト
			Rect strPos = new Rect(position.position, spAttr.TextSize);
			strPos.x += lineWidth + spc;
			strPos.y += (GetHeight() - spAttr.TextSize.y) / 2f;
			GUI.Label(strPos, spAttr.Title, spAttr.GuiStyle);

			// 右ライン
			line = position;
			line.xMin += lineWidth + spc + spAttr.TextSize.x + spc;
			line.yMin += (GetHeight() - spAttr.LineSize) / 2f;
			line.width = lineWidth;
			line.height = spAttr.LineSize;
			EditorGUI.DrawRect(line, lineColor);
		}
	}

	/// <summary>
	/// ピクセル単位でGUIの高さをInspectorに通知
	/// </summary>
	public override float GetHeight()
	{
		if (!string.IsNullOrEmpty(spAttr.Title))
		{
			if (spAttr.LineSize < spAttr.TextSize.y)
			{
				return spAttr.TextSize.y;
			}
		}
		return spAttr.LineSize + 4f;
	}
}
#endif
