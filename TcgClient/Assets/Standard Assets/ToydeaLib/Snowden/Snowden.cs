#if (UNITY_ANDROID && !UNITY_EDITOR) || SNOWDEN_TEST
#define SNOWDEN_ENABLE
#endif
	//#define SNOWDEN_LOG_ENABLE // テストの時以外は、無効化すること！
	using UnityEngine;
using UnityEngine.Profiling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Text;
using Ionic.Zip;
using UnityEngine.Networking;
using System.Text.RegularExpressions;

/// <summary>
/// 不正なアプリを防ぐ機能
///
/// SecurityCheckerなどの名前にしていないのは、名前から機能を推測されるのを防ぐため
///
/// エディタでテストする場合は、SNOWDEN_TESTのシンボルを有効にする
/// </summary>
public class Snowden : MonoSingleton<Snowden>
{
	/// <summary>
	/// ファイルとハッシュのペア
	/// </summary>
	[Serializable]
	class Guy
	{
		public string f = null;
		public string h = null;
	}

	[Serializable]
	class Bison
	{
		public string id;
		public string vr;
		public string dm;
		public string dt;
		public string ui;
		public string hs;
		public List<Guy> fh;
	}


	/// <summary>
	/// 不正の判定が終わったかどうか
	/// </summary>
	public bool Vega { get; private set; }

	/// <summary>
	/// 検証結果で不正が存在するかどうか
	/// 0: 確認中
	/// 1: 確認できない（ネットワーク接続など）
	/// 2: OK
	/// 3: 不正
	/// </summary>
	public int Karin { get; private set; }

	/// <summary>
	/// 開発端末かどうかを判定する
	/// </summary>
	public bool Honda { get; private set; }

	/// <summary>
	/// APK内のファイル名とHashの対
	/// </summary>
	List<Guy> mika_;

	/// <summary>
	/// ロック用の変数
	/// </summary>
	object ken_ = new object();

	/// <summary>
	/// ハッシュの計算が終わったかどうか
	/// </summary>
	bool dj_;

	/// <summary>
	/// 全体のハッシュ
	/// </summary>
#pragma warning disable 649
	string gen_;
#pragma warning restore 649

	public static string Chunli;

	/// <summary>
	/// ログを出力する
	/// 解析されないように、define で無効化する
	/// </summary>
	/// <param name="obj"></param>
	[System.Diagnostics.Conditional("SNOWDEN_LOG_ENABLE")]
	static void log(object obj)
	{
		Debug.Log("Snowden: " + obj);
	}

	IEnumerator Start()
	{
		yield return new WaitWhile(() => Chunli == null);

		StartCoroutine(zangief());

		mika_ = new List<Guy>();
		blanka();

		// ハッシュの計算が終わるまで待つ
		for(;;)
		{
			lock (ken_)
			{
				if( dj_)
				{
					break;
				}
			}

			yield return null;
		}

		#if !SNOWDEN_ENABLE
		// EDITOR上では動かさない
		Vega = true;
		Karin = 2;
		#else
		yield return sagat();
		#endif

		log("Karin:" + Karin);

	}

	/// <summary>
	/// ネットワークのリザルト
	/// </summary>
	class Dhalsim
	{
		/// <summary>
		/// 返答
		/// </summary>
		public string Yoga;
		/// <summary>
		/// 通信に成功したかどうか
		/// </summary>
		public bool Fire;
	}

	/// <summary>
	/// ネットワークの情報を送信する
	/// </summary>
	/// <param name="uri"></param>
	/// <param name="json"></param>
	/// <param name="dhalsim"></param>
	/// <returns></returns>
	IEnumerator adon(string uri, string json, Dhalsim dhalsim)
	{
		// データを送信する
		var data = hawk(Encoding.UTF8.GetBytes(json));

		var form = new WWWForm();
		form.AddBinaryData("data", data);
		var req = UnityWebRequest.Post(uri, form);
		req.SetRequestHeader("toydearune", "1");
		#if UNITY_2017_1_OR_NEWER
		var ao = req.SendWebRequest();
		#else
		var ao = req.Send();
		#endif

		yield return ao;

		#if UNITY_2017_1_OR_NEWER
		if( req.isNetworkError || req.isHttpError )
		#else
		if (req.isError)
		#endif
		{
			log("Error: " + req.error);
			dhalsim.Fire = false;
			yield break;
		}

		log("Success:" + req.downloadHandler.text);

		dhalsim.Yoga = req.downloadHandler.text;
		dhalsim.Fire = true;
	}

	/// <summary>
	/// 簡単な暗号化を行う
	/// </summary>
	byte[] hawk(byte[] data)
	{
		byte mask = (byte)UnityEngine.Random.Range(24, 223);
		var buf = new byte[data.Length + 1];
		buf[0] = mask;
		for( int i = 0; i < data.Length; i++)
		{
			buf[i + 1] = (byte)(data[i] ^ mask);
		}
		return buf;
	}

	/// <summary>
	/// 簡単な暗号化の復号を行う
	/// </summary>
	byte[] unhawk(byte[] data)
	{
		byte mask = data[0];
		var buf = new byte[data.Length - 1];
		for (int i = 1; i < data.Length; i++)
		{
			buf[i - 1] = (byte)(data[i] ^ mask);
		}
		return buf;
	}

	/// <summary>
	/// 開発機かどうかを判定する
	/// </summary>
	/// <returns></returns>
	IEnumerator zangief()
	{
		#if !SNOWDEN_ENABLE
		log("dev always");
		Honda = true;
		yield break;
		#else
		var keyArray = unhawk(Encoding.UTF8.GetBytes(SystemInfo.deviceUniqueIdentifier));
		var key = string.Join("", keyArray.Select(a => a.ToString("x2")).ToArray());
		if( PlayerPrefs.GetString(key) == SystemInfo.deviceUniqueIdentifier)
		{
			log("dev saved");
			Honda = true;
			yield break;
		}

		// 送信データを作成する
		string json = JsonUtility.ToJson(new Bison()
		{
			dm = SystemInfo.deviceModel,
			dt = SystemInfo.deviceType.ToString(),
			ui = SystemInfo.deviceUniqueIdentifier,
		});

		var dhalsim = new Dhalsim();
		yield return adon(Chunli + "hello", json, dhalsim);

		if (dhalsim.Fire)
		{
			if (dhalsim.Yoga == "dev")
			{
				log("dev ok");
				PlayerPrefs.SetString(key, SystemInfo.deviceUniqueIdentifier);
				Honda = true;
			}
			else
			{
				log("dev ng");
			}
		}
		else
		{
			log("dev failed");
		}
		#endif
	}

	/// <summary>
	///
	/// </summary>
	/// <returns></returns>
	IEnumerator sagat()
	{
		// 送信データを作成する
		string json = JsonUtility.ToJson(new Bison()
		{
			id = Application.identifier,
			vr = Application.version,
			dm = SystemInfo.deviceModel,
			dt = SystemInfo.deviceType.ToString(),
			ui = SystemInfo.deviceUniqueIdentifier,
			hs = gen_,
			fh = mika_,
		});

		var dhalsim = new Dhalsim();
		yield return adon(Chunli + "login", json, dhalsim);

		// 返信によって状態を変える
		if (dhalsim.Fire)
		{
			if (dhalsim.Yoga == "ok")
			{
				Karin = 2;
			}
			else
			{
				Karin = 3;
			}
		}
		else
		{
			Karin = 1;
		}
		Vega = true;
	}

	#if SNOWDEN_ENABLE
	string guile_; // APKのパス

	void blanka()
	{
		#if UNITY_EDITOR
		// MEMO: Editorでの実験用, test.apk を特定の場所に置く
		guile_ = Path.Combine(Path.Combine(Application.dataPath, ".."), "test.apk");
		#else
		guile_ = Application.dataPath;
		#endif
		new Thread(blanka2).Start();
	}

	void blanka2()
	{
		try
		{
			var pat = new Regex(@"AndroidManifest\.xml$|\.(dll|so|dex|.arsc)$");
			log(guile_);
			using (var zip = new ZipFile(guile_))
			{
				var maxSize = zip.Max(e => e.UncompressedSize);
				//Debug.LogFormat("maxSize: {0}", maxSize);
				var buf = new byte[maxSize];
				var stream = new MemoryStream(buf);

				foreach (var entry in zip)
				{
					if( !pat.IsMatch(entry.FileName))
					{
						continue;
					}
					stream.Position = 0;
					stream.SetLength(0);
					entry.Extract(stream);

					var md5 = new MD5CryptoServiceProvider();
					var hash = md5.ComputeHash(buf, 0, (int)stream.Length);
					var hashStr = string.Join("", hash.Select(b => b.ToString("x2")).ToArray());

					mika_.Add(new Guy { f = entry.FileName, h = hashStr });
					log(hashStr + " " + entry.FileName);
				}

				{
					var sb = new StringBuilder();
					foreach (var fh in mika_.OrderBy(fh => fh.f))
					{
						sb.Append(fh.f);
						sb.Append('\t');
						sb.Append(fh.h);
						sb.Append('\n');
					}

					var md52 = new MD5CryptoServiceProvider();
					var hash = md52.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
					gen_ = string.Join("", hash.Select(b => b.ToString("x2")).ToArray());
				}
			}
		}
		catch (Exception ex)
		{
			log(ex);
		}

		lock (ken_)
		{
			dj_ = true;
		}
	}
	#else
	void blanka()
	{
		dj_ = true;
	}
	#endif
}
