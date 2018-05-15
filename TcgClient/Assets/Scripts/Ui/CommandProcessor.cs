using System.Collections.Generic;
using System.Linq;
using Game;
using Master;
using RSG;
using System;
using System.Reflection;
using UnityEngine;

using GameScene = MainScene;

public partial class CommandProcessor
{
	public static bool Log;

	public static Dictionary<string, MethodInfo> Processors = new Dictionary<string, MethodInfo>();

	static CommandProcessor()
	{
		var processorType = typeof(CommandProcessor);
		foreach (var method in processorType.GetMethods())
		{
			var parameters = method.GetParameters();
			if (!method.Name.StartsWith("Process"))
			{
				continue;
			}
			if (parameters.Length != 2)
			{
				Debug.LogError("invalid parameter size in " + method.Name + " , expect 2 but " + parameters.Length);
				continue;
			}
			if (parameters[0].ParameterType != typeof(GameScene))
			{
				Debug.LogError("invalid first parameter in " + method.Name);
				continue;
			}
			//Debug.Log("register command processor " + method.Name + " as type " + parameters[1].ParameterType.Name);
			Processors.Add(parameters[1].ParameterType.Name, method);
		}
	}

	static object[] paramCache_ = new object[2];

	public static IPromise Process(GameScene scene, object com)
	{
		MethodInfo method;
		if (Processors.TryGetValue(com.GetType().Name, out method))
		{
			paramCache_[0] = scene;
			paramCache_[1] = com;
			return (IPromise)method.Invoke(null, paramCache_);
		}
		throw new Exception("cannot find processor for " + com.GetType().Name); // みつからなかっった
	}
}
