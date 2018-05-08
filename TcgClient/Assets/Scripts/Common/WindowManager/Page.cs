using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using RSG;


/// <summary>
/// GroundMainの下の各画面のベース
///
/// 各状態のライフサイクルとイベント(GameObjectのものを含む)は下記のようになっている
///
///   ↓ はじめて表示される
///   ↓
/// Initialized
///   ↓
/// (MonoBehaviour.Awake)
///   ↓
/// (OnStartPage)
///   ↓
/// (OnLoadPageコルーチン)
///   ↓
/// (OnActivatePage)
///   ↓
/// (MonoBehaviour.OnEnable)
///   ↓
/// (MonoBehaviour.Start)
///   ↓
///   ↓ ←--(OnSaveState)--(OnChangeState)--+
///   ↓ ↑                                  ↓ PushState()で移動/バックする
///   ↓ ↑+---------------------------------+
///   ↓ ↑↓
/// Active ←--------------+
///   ↓ 別の画面に移動する  ↑
///   ↓                   ↑
/// (OnSaveState)         ↑
///   ↓                 (OnChangeState)
/// (OnDeactivatePage)   ↑
///   ↓                 (OnActivatePage)
///   ↓                   ↑
/// Deactive -------------+  次の画面からバックキーで戻ってくる
///   ↓
///   ↓ バックキーで前の画面にもどる/別の画面に移動する
///   ↓
/// (OnFinishPage)
///   ↓
/// (OnDestory)
///   ↓
/// Destroyed
///
/// </summary>
abstract public class Page : MonoBehaviour
{
	/// <summary>
	/// 開発モード
	/// </summary>
	public static bool DevelopMode = false;

	/// <summary>
	/// フレームが作られた時に呼ばれるコールバック
	/// </summary>
	/// <param name="opt"></param>
	public virtual void OnStartPage(PageOption opt)
	{
	}

	/// <summary>
	/// フレームが作られた時に呼ばれるコールバック
	/// </summary>
	/// <param name="opt"></param>
	public virtual IEnumerator OnLoadPage(PageOption opt)
	{
		yield return null;
	}

	/// <summary>
	/// フレームがアクティブになった時に呼ばれるコールバック
	/// </summary>
	public virtual void OnActivatePage(PageOption opt)
	{
	}

	/// <summary>
	/// フレームが非アクティブになった時に呼ばれるコールバック
	/// </summary>
	public virtual IEnumerator OnDeactivatePage()
	{
		return null;
	}

	/// <summary>
	/// フレームが削除される時に呼ばれるコールバック
	/// </summary>
	public virtual void OnFinishPage()
	{
	}

	/// <summary>
	/// フレームの状態変更の時に呼ばれるコールバック
	/// </summary>
	public virtual void OnChangeState(PageOption opt)
	{
	}

	/// <summary>
	/// フレームの状態変更の時に呼ばれるコールバック
	/// </summary>
	public virtual void OnSaveState(PageOption opt)
	{
	}


	/// <summary>
	/// フェードが開けた後に呼ばれるコールバック
	/// </summary>
	public virtual IEnumerator AfterActivate(PageOption opt)
	{
		yield return null;
	}

}

/// <summary>
/// Browser.Goto() や Page.OnStartPage()などで指定されるオプション
///
/// フレーム遷移時の各種パラメータを指定する
/// </summary>
public class PageOption
{
	/// <summary>
	/// オリジナルのURI
	/// </summary>
	public Uri Uri;

	/// <summary>
	/// URIのクエリの分解済みのもの（GroundMainが設定する)
	/// </summary>
	public Dictionary<string, string> UriParam;

	/// <summary>
	/// その他、複雑な処理用のメモリ
	/// </summary>
	public object Param;

	/// <summary>
	/// 戻るボタンのヒストリをクリアするかどうか（フッターメニューからの遷移など）
	/// </summary>
	public bool ClearHistory;

	/// <summary>
	/// フェードアウト/フェードインするかどうか
	/// </summary>
	public bool Fade;

	/// <summary>
	/// フェードの色(Color.clearならデフォルトの色）
	/// </summary>
	public Color FadeColor = Color.clear;

	/// <summary>
	/// フェードインのみしない
	/// </summary>
	public bool NoFadein;

	/// <summary>
	/// 「戻る」ボタンで戻ったかどうか
	/// </summary>
	public bool IsBack;

	/// <summary>
	/// 「戻る」ボタンで戻った場合の帰り値
	/// </summary>
	public object BackResult;

	/// <summary>
	/// PushState()での移動かどうか
	/// </summary>
	public bool IsPushState;

	/// <summary>
	/// メッセージログを閉じるか？
	/// </summary>
	public bool HideMessageLog = true;

	/// <summary>
	/// ミニマップを表示
	/// </summary>
	public bool ShowMiniMap;

	/// <summary>
	/// チュートリアルのメッセージパネルを表示するか
	/// </summary>
	public bool ShowTutorialPanel = true;

	/// <summary>
	/// BGM
	/// </summary>
	public string Bgm;

	/// <summary>
	/// UI（ミニマップとファングパネルを表示するかどうか）
	/// </summary>
	public bool ShowUi = true;

	/// <summary>
	/// ページ移動前にディレイ
	/// </summary>
	public float DelayTime = 0f;

	public enum ShowFangType
	{
		Hidden,
		Closed,
		Opened,
	}

	/// <summary>
	/// ファングパネルを表示するかどうか
	/// </summary>
	public ShowFangType ShowFang = ShowFangType.Hidden;

	/// <summary>
	/// URIのパラメータを取得する便利関数
	/// </summary>
	/// <param name="key">キー</param>
	/// <param name="defaultValue">デフォルト値</param>
	/// <returns></returns>
	public float GetFloatParam(string key, float defaultValue = 0)
	{
		string val;
		if (UriParam.TryGetValue(key, out val))
		{
			float result;
			if (float.TryParse(val, out result))
			{
				return result;
			}
		}
		return defaultValue;
	}

	/// <summary>
	/// URIのパラメータを取得する便利関数
	/// </summary>
	/// <param name="key">キー</param>
	/// <param name="defaultValue">デフォルト値</param>
	/// <returns></returns>
	public int GetIntParam(string key, int defaultValue = 0)
	{
		string val;
		if (UriParam.TryGetValue(key, out val))
		{
			int result;
			if (int.TryParse(val, out result))
			{
				return result;
			}
		}
		return defaultValue;
	}

	/// <summary>
	///
	/// </summary>
	/// <param name="key"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public string GetStringParam(string key, string defaultValue = "")
	{
		string val;
		if (UriParam.TryGetValue(key, out val))
		{
			return val;
		}
		return defaultValue;
	}

	/// <summary>
	///
	/// </summary>
	/// <param name="key"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public bool GetBoolParam(string key, bool defaultValue = false)
	{
		string val;
		if (UriParam.TryGetValue(key, out val))
		{
			// 無い場合・大文字小文字の区別無しのfalse
			if (String.IsNullOrEmpty(val) || val.ToLowerInvariant() == "false")
			{
				return false;
			}
			return true;
		}
		return defaultValue;
	}
}
