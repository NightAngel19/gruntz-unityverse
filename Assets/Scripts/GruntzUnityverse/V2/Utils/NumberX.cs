﻿using UnityEngine;

namespace GruntzUnityverse.V2.Utils {
public static class NumberX {
	public static int AsInt(this float value) {
		return Mathf.RoundToInt(value);
	}

	public static float SnappedToIncrement(this float value, float increment) {
		return Mathf.Round(value / increment) * increment;
	}

	public static int SnappedToIncrement(this int value, int increment) {
		return Mathf.RoundToInt(value / increment) * increment;
	}
}
}
