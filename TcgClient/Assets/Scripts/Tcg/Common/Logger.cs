using System.Collections.Generic;
using System.Linq;
using System;
#if UNITY_5_5_OR_NEWER
using UnityEngine;
#endif

namespace Game
{
	public class Logger
	{

		public enum LogLevel
		{
			Trace,
			Info,
			Warn,
			Error,
			Fatal
		}

		public static LogLevel Level = LogLevel.Info;

		[System.Diagnostics.DebuggerNonUserCode]
		static void write(LogLevel level, string format, params object[] obj )
		{
			if( level < Level)
			{
				return;
			}

			if (obj.Length == 0)
			{
				#if UNITY_5_5_OR_NEWER
				if( level == LogLevel.Error )
				{
					Debug.LogError(format);
				}
				else if (level == LogLevel.Warn)
				{
					Debug.LogWarning(format);
				}
				else
				{
					Debug.Log(format);
				}
				#else
				Console.WriteLine(format);
				#endif
			}
			else
			{
				#if UNITY_5_5_OR_NEWER
				if( level == LogLevel.Error )
				{
					Debug.LogErrorFormat(format, obj);
				}
				else
				{
					Debug.LogFormat(format, obj);
				}
				#else
				Console.WriteLine(string.Format(format, obj.Select(o => o.ToString()).ToArray()));
				#endif
			}
		}

		[System.Diagnostics.DebuggerNonUserCode]
		public static void Trace (string format, params object[] obj)
		{
			write (LogLevel.Trace, format, obj);
		}

		[System.Diagnostics.DebuggerNonUserCode]
		public static void Info (string format, params object[] obj)
		{
			write (LogLevel.Info, format, obj);
		}

		[System.Diagnostics.DebuggerNonUserCode]
		public static void Warn(string format, params object[] obj)
		{
			write (LogLevel.Warn, format, obj);
		}

		[System.Diagnostics.DebuggerNonUserCode]
		public static void Error(string format, params object[] obj)
		{
			write (LogLevel.Error, format, obj);
		}

		[System.Diagnostics.Conditional("DEBUG")]
		[System.Diagnostics.DebuggerNonUserCode]
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
		public static void Assert(bool mustTrue)
		{
			if (!mustTrue)
			{
				const int callerFrameIndex = 1; // 2つ前の呼び出し元メソッド名.
				var callerFrame = new System.Diagnostics.StackFrame(callerFrameIndex);
				var callerMethod = callerFrame.GetMethod();
				throw new Exception("Assert Failed in " + callerMethod.DeclaringType + "." + callerMethod.Name);
			}
		}
	}
}
