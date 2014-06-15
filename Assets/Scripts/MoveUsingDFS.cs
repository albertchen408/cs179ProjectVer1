using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoveUsingDFS : MonoBehaviour {
	
	//public Transform[] wayPoints;
	public float walkSpeed = 10.0f;
	public float turnSpeed = 1.0f;
	public GameObject mTheStart;
	public bool stop;
	// Use this for initialization
	public int i;
	public Vector3 dir;
	//List<Vector3> lst;
	public List<GameObject> wayPoints;
	//Stack<GameObject> stk;
	public Transform rotateEnem;
	public Transform rot;
	public bool fromSpawner = false;
	void Start () 
	{
		walkSpeed = gameObject.GetComponent<EnemyStats> ().mSpeed;
		if(!fromSpawner)
			i = wayPoints.Count-1;
		/*
		mTheStart = GameObject.Find ("UnitsAllowedStart");
		stk = new Stack<GameObject> ();
		lst = new List<Vector3> ();
		visited = new List<GameObject> ();
		stk.Push (mTheStart);
		while(stk.Count != 0)
		{
			GameObject tmp = stk.Pop();
			Vector3 pos = new Vector3(tmp.transform.position.x, 0.0f, tmp.transform.position.z);
			lst.Add(pos);
			if(tmp.name == "UnitsAllowedFinish")
				break;
			GridForUnits grid = tmp.GetComponent<GridForUnits>();

			visited.Add(tmp);
			for(int j= grid.nextTo.Length-1; j >= 0; j--)
			{
				GameObject next = grid.nextTo[j];
				if(visited.Contains(next) == false)
				{
					//visited.Add(next);
					//GridForUnits nextTo = next.GetComponent<GridForUnits>();
				 	stk.Push (next);
				}
			}
		}
		dir = lst[0] - transform.position;
		dir = dir.normalized;
		*/
		stop = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		stop = false;
		GameObject gct = GameObject.FindGameObjectWithTag ("GameController");
		if(gct)
		{
			Buy_Shoot_Modes bsm = gct.GetComponent<Buy_Shoot_Modes>();
			if(bsm.mPathWaysChanged)
			{
				//wayPoints = bsm.thePath;
				if(wayPoints.Contains(bsm.mTheTaken))
				{
					int currI = wayPoints.IndexOf(bsm.mTheTaken);
					//Debug.Log (currI);
					if(currI < i)
					{
					
						wayPoints = bsm.dijkstraPath(wayPoints[i],wayPoints);
						//Debug.Log (wayPoints.Count);
						i = wayPoints.Count-1;
						dir = wayPoints[i].transform.position - transform.position;
						dir = dir.normalized;
					}
					//Debug.Log ("change it");
				}
			}
			/*
			if(bsm.thePathsHaveChanged1)
			{
				if(wayPoints.Contains(bsm.theTaken))
				{
					int currI = wayPoints.IndexOf(bsm.theTaken);
					Debug.Log ("hello");
					if(currI <= i)
					{
						wayPoints = bsm.dijkstraPath(wayPoints[i]);
						//Debug.Log (wayPoints.Count);
						i = wayPoints.Count-1;
						dir = wayPoints[i].transform.position - transform.position;
						dir = dir.normalized;
					}
					//Debug.Log ("change it");
				}
			}
			*/
			//bsm.thePathsHaveChanged = false;

		}
		if(i >= 0)
		{

			walkSpeed = gameObject.GetComponent<EnemyStats>().mSpeed;
			transform.Translate (dir * Time.deltaTime * walkSpeed);
			//transform.position = Vector3.Lerp(transform.position, wayPoints[i].position, 
			//Time.deltaTime * walkSpeed);
			if((transform.position - wayPoints[i].transform.position).sqrMagnitude <= 1.0f)
			{
				i--;
				stop = true;
				//Debug.Log(stop);
				if(i >= 0)
				{
					rotateEnem.LookAt(wayPoints[i].transform.position);
					rot.eulerAngles = new Vector3(rot.eulerAngles.x, rotateEnem.eulerAngles.y, rot.eulerAngles.z);
					dir = wayPoints[i].transform.position - transform.position;
					dir = dir.normalized;
				}
			}
			Debug.Log(i);
			if(i > 1)
			{
				if((transform.position - wayPoints[i].transform.position).sqrMagnitude > 150.0f)
				{
					rotateEnem.LookAt(wayPoints[i].transform.position);
					rot.eulerAngles = new Vector3(rot.eulerAngles.x, rotateEnem.eulerAngles.y, rot.eulerAngles.z);
					Debug.Log("Distance greater than 100" + (transform.position - wayPoints[i].transform.position).sqrMagnitude);
					dir = wayPoints[i].transform.position - transform.position;
					dir = dir.normalized;
				}
			}
		}
		else
		{
			GameObject gc = GameObject.FindGameObjectWithTag("GameController");
			if(gc)
			{
				NewSpawnWaves sw = gc.GetComponent<NewSpawnWaves>();
				sw.numEnemiesRemaining--;
			}
			Destroy(gameObject);
		}

	}
}

