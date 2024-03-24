using UnityEngine;
using System.Collections;

/*
 * MyCustomPath class shows you how to implement your own path.
 * In this example we're going to describe a line as a path.
 */
 
public class MyCustomPath : ORPath {
	
	// Customize the height of the line.
	
	public float height = 10f;
	
	// ------ Required Methods ------
		
	/*
	 * This is the only method that is required. You must return a 3D
	 * point for the given 't' value. The variable 't' is ALWAYS
	 * between 0 and 1.
	 */
	
	override public Vector3 PointOnPath(float t) {
		
		return new Vector3(0, t * height, 0);
		
		/*
		 * Return this point if you want to consider the position
		 * and rotation of the gameObject.
		 */
		 
		// return transform.position + transform.rotation * new Vector3(0, t * height, 0);
		
	}
	
	// ------ Optional Methods ------
		
	/*
	 * Draw custom gizmos for your path here.
	 * Althougth it isn't mandatory, you can optimize the default
	 * behaviour by creating you own method.
	 */
	
	override public void DrawGizmos() {
		
		base.DrawGizmos();
		 
		/*
		Gizmos.color = color;
		Gizmos.DrawLine(Vector3.zero, new Vector(0, height, 0));
		*/
		
	}

	/*
	 * Implement this method if you know how to calculate the path length,
	 * otherwise it will be calculated numerically.
	 */
		
	override public float PathLength() {
		
		return base.PathLength();
		
		// return height;
		
	}
		
	/*
	 * If you have an expression for derivative of the path e might want
	 * to use it so you can save CPU cycles.
	 */
	
	override public Vector3 Derivative(float t) {
		
		return base.Derivative(t);
		 
		 // return new Vector(0, 1, 0);
		 
	}
	
}
