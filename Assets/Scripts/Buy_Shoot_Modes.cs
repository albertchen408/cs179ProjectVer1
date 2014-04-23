﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Buy_Shoot_Modes : MonoBehaviour {

	private const float TOWER_FIRE_X = -80.0f;
	private const float TOWER_FIRE_Y = 20.0f;
	private const float TOWER_FIRE_Z = 0.0f;

	private Vector3 TOWER_FIRE_VECTOR = new Vector3(TOWER_FIRE_X, TOWER_FIRE_Y, TOWER_FIRE_Z);

	/** The maximum firing angle in radians */
	private const float MAX_FIRE_ANGLE = Mathf.PI / 2; /** 45 degrees */

	/** TODO: Replace with int and have constants defining what mode/weapon */
	private bool buyMode =	 false;
	private bool shootMode = true;
	private bool whichWeapon = true;

	private int theWeapon = 0;

	private float mClickY;
	private float mReleaseY;

	private Vector3 targetPosition;

	private GameObject lastPlane;
	private GameObject theTower;

	public Material oldMaterial;
	public Material hoverMaterial;
	
	public LayerMask buyPlane;
	
	public GameObject[] weapons;
	public GameObject[] towerWeapons;

	//for the upgrades.
	private GridForUnits grid;


	//for pathfinding
	List<GameObject> distances;
	Queue<GameObject> minVisits;
	public List<GameObject> thePath;
	public GameObject findMinInList(List<GameObject> lst)
	{
		GameObject temp = null;
		int compare = 10000;
		for(int i = 0; i < lst.Count; i++)
		{
			GridForUnits gfn = lst[i].GetComponent<GridForUnits>();
			if(gfn)
			{
				if(!gfn.hasBeenVisited)
				{
					if(gfn.distance < compare)
						temp = lst[i];
					compare = gfn.distance;
				}
			}
		}
		return temp;
	}

	// Use this for initialization
	void Start () 
	{
		//make this as a public function to easily use
		GameObject walkablePlaneParent = GameObject.Find ("UnitsApproved");

		distances = new List<GameObject> ();
		minVisits = new Queue<GameObject> ();
		thePath = new List<GameObject> ();
		foreach(Transform child in walkablePlaneParent.transform)
		{
			GridForUnits gfn = child.gameObject.GetComponent<GridForUnits>();
			gfn.distance = 10000;
			gfn.hasBeenVisited = false;
			distances.Add(child.gameObject);
		}
		//Debug.Log (distances.Count);
		GameObject start = GameObject.Find ("UnitsAllowedStart");
		if(start)
		{
			GridForUnits gfnStart = start.GetComponent<GridForUnits> ();
			gfnStart.distance = 0;

			minVisits.Enqueue(start);
			gfnStart.hasBeenVisited = true;
			//Debug.Log(minVisits.Count);

			while(minVisits.Count != 0)
			{
				GameObject curr = minVisits.Peek();
				GridForUnits visitCurr = curr.GetComponent<GridForUnits>();
				for(int neigh = 0; neigh < visitCurr.nextTo.Length; neigh++)
				{
					int currentDist = visitCurr.distance;
					GridForUnits nodesToVisitGFN = visitCurr.nextTo[neigh].GetComponent<GridForUnits>();
					int oldDis = nodesToVisitGFN.distance;
					int newDis = currentDist + 1;
					if(newDis < oldDis)
					{
						nodesToVisitGFN.distance = newDis;
						nodesToVisitGFN.previous = curr;
					}
				}
				GameObject theMin = findMinInList(distances);
				if(theMin != null)
				{
					GridForUnits theMinGFN = theMin.GetComponent<GridForUnits>();
					theMinGFN.hasBeenVisited = true;
					minVisits.Enqueue(theMin);
				}
				else
				{
					minVisits.Dequeue();
				}
			}

			GameObject end = GameObject.Find("UnitsAllowedFinish");
			if(end)
			{
				while(end.name != "UnitsAllowedStart")
				{
					thePath.Add(end);
					GridForUnits gfnEnd = end.GetComponent<GridForUnits>();
					end = gfnEnd.previous;
				}
				thePath.Add(end);
			}
			Debug.Log(thePath.Count);
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Input.GetKey (KeyCode.B))
		{	
			buyMode = true;
			shootMode = false;
		}

		if(Input.GetKey(KeyCode.S))
		{
			buyMode = false;
			shootMode = true;
			if(lastPlane)
			{
				lastPlane.renderer.material = oldMaterial;
				lastPlane = null;
			}
		}

		if(buyMode)
		{
			if(Input.GetKey(KeyCode.W))
			{
				whichWeapon = false;
				//theWeapon = 1;
			}
			if(Input.GetKey(KeyCode.T))
			{
				whichWeapon = true;
				//theWeapon = 0;
			}
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if(Physics.Raycast(ray,out hit,1000,buyPlane))
			{
				if(lastPlane)
					lastPlane.renderer.material = oldMaterial;
				lastPlane = hit.collider.gameObject;
				//oldMaterial = lastPlane.renderer.material;
				lastPlane.renderer.material = hoverMaterial;
			}
			else
			{
				if(lastPlane)
				{
					lastPlane.renderer.material = oldMaterial;
					lastPlane = null;
				}
			}
			if(Input.GetMouseButtonDown(0) && lastPlane)
			{
				grid = lastPlane.GetComponent<GridForUnits>();
				if(grid.isAvailable)
				{
					if(weapons.Length > 0)
					{
						Vector3 spawn = lastPlane.transform.position;

						if(!whichWeapon)
						{
							//spawn.y+=3.0f;
							theWeapon = 1;
						}
						else
						{
							//spawn.y += 6.5f;
							theWeapon = 0;
						}
						GameObject currWeapon = (GameObject)Instantiate(weapons[theWeapon], spawn, 
						                                                	Quaternion.identity);
						//temp.transform.localEulerAngles = new Vector3(0.0f, Random.Range(0,360), 0.0f);
						grid.whatsInside = currWeapon;
						grid.isAvailable = false;
						lastPlane.gameObject.tag = "Taken";
					}
				}
				else
				{
					if(grid.whatsInside != null)
					{
						Weapons getUpgrade = grid.whatsInside.GetComponent<Weapons>();
						GameObject whatToUpgrade = getUpgrade.upgradeIt;
						if(whatToUpgrade != null)
						{
							Vector3 tempPos = getUpgrade.transform.position;
							Quaternion tempRot = getUpgrade.transform.rotation;
							Destroy(getUpgrade.gameObject);
							GameObject currWeapon = (GameObject)Instantiate(whatToUpgrade, 
							                                                tempPos, tempRot);
							grid.whatsInside = currWeapon;
						}
					}
				}
			}
		}

		if(shootMode)
		{
			if(Input.GetMouseButtonDown(0))
			{
				if(towerWeapons.Length > 0)
				{
					mClickY = Input.mousePosition.y;
					Plane playerPlane = new Plane(Vector3.up, transform.position);
					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				
					float hitdist;
					if(playerPlane.Raycast(ray, out hitdist))
					{
						targetPosition = ray.GetPoint(hitdist);
						Debug.Log(targetPosition);
					}
				}
			} else if (Input.GetMouseButtonUp(0)) {
				mReleaseY = Input.mousePosition.y;
				/** Calculate the delta Y position of the mouse and get the angle */
				float maxAngleScreenRatio = Screen.height / 2;
				float deltaY = mReleaseY - mClickY;
				if (deltaY > maxAngleScreenRatio) {
					deltaY = maxAngleScreenRatio;
				} else if (deltaY < -maxAngleScreenRatio) {
					deltaY = -maxAngleScreenRatio;
				}
				float fireAngle = deltaY / maxAngleScreenRatio * MAX_FIRE_ANGLE;
				/** Get the tower */
				theTower = (GameObject)Instantiate(towerWeapons[0], 
				                                   TOWER_FIRE_VECTOR,
				                                   Quaternion.identity);
				/** Create the cannon shot */
				FireTowersBasicAmmo cannon = theTower.GetComponent<FireTowersBasicAmmo>();
				Vector3 dir = targetPosition - TOWER_FIRE_VECTOR;
				cannon.dir = dir.normalized;
				cannon.mAngle = fireAngle;
			}
		}
	}

}
