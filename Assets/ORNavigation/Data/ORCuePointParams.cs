using UnityEngine;
using System.Collections;

public class ORCuePointParams {
	
	public ORNavigation navigation;
	public ORCuePoint cuePoint;
	public int cuePointIndex;
	public float t;
	
	public ORCuePointParams(ORNavigation navigation, ORCuePoint cuePoint, int cuePointIndex, float t) {
		this.navigation = navigation;
		this.cuePoint = cuePoint;
		this.cuePointIndex = cuePointIndex;
		this.t = t;
	}
	
}
