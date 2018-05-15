using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RSG;
using DG.Tweening;
using System;
using UnityEngine;

using Game;
using Master;
using GameLog;
using UnityEngine.Profiling;

using GameScene = MainScene;

public partial class CommandProcessor
{
	public static IPromise ProcessCardPlayied(GameScene scene, CardPlayed com)
	{
		return Promise.Resolved();
	}
}
