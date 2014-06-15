using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class Buy_Shoot_Modes : MonoBehaviour {

	private const float TOWER_FIRE_X = -80.0f;
	private const float TOWER_FIRE_Y = 20.0f;
	private const float TOWER_FIRE_Z = 0.0f;

	private const int INPUT_MODE_SHOOT = 0;
	private const int INPUT_MODE_BUY = 1;

	private Vector3 TOWER_FIRE_VECTOR = new Vector3(TOWER_FIRE_X, TOWER_FIRE_Y, TOWER_FIRE_Z);

	public Transform mTowerAmmoSpawn;
	/** The maximum firing angle in radians */
	private const float MAX_FIRE_ANGLE = Mathf.PI / 2; /** 45 degrees */

	/** TODO: Replace with int and have constants defining what mode/weapon */
	private int mFireMode = 0;

	//private bool selecting = false;

	public int theWeapon = 0;
	public int theTowerWeapon = 0;

	private float mClickY;
	private float mClickX;
	private float mReleaseY;

	public Vector3 targetPosition;

	private GameObject mLastPlane;
	private GameObject mHoverObject;
	private GameObject mTowerAmmo;

	public Material mOldMaterial;
	public Material mHoverMaterial;
	public Material mCantBuyMaterial;

	public int mBuyPlane;
	private int mEnemeyLayter = 10;
	private int mPlacement = 8;

	/** Weapons and weapon types */
	public GameObject[] mWeapons;
	public GameObject[] mTowerWeapons;
	public GameObject[] mDummyWeapons;

	/** Materials and game objects for targeting reticles */
	public Material mTargetMaterial;
	private GameObject mTargetLocation;
	private GameObject mTargetPole;
	private GameObject mTargetLine;

	/** Materials used for grid */
	private GridForUnits mEnemyGrid;

	private bool mIsClickDown;


	/** List of paths for the enemies */
	public List<GameObject> mEnemyPath;
	public List<GameObject> mEnemyPath2;
	public List<GameObject> mEnemeyPath3;

	public GameObject mStart;
	public GameObject mStart2;
	public GameObject mStart3;

	public bool mPathWaysChanged = false;
	public GameObject mTheTaken;

	//for switching modes by clicking things on the scene
	private GameObject mLastButtonSelected;
	public LayerMask mButtonMask;
	public Material mOldButton;


	private const int STATE_GAME_STARTED = 0;
	private const int STATE_GAME_PAUSED = 1;
	private const int STATE_GAME_OVER = 2;

	// TODO: Refactor to have different game states. paused, started, and game over etc.
	public int mGameState = STATE_GAME_OVER;

	public bool gameover;

	//checking if everything is blocked;
	public bool mAllPathsBloked = false;
	List<GameObject> mDummyPaths = new List<GameObject>();


	//firing off the laser
	// TODO; Remove (no more laser?
	public float mLaserFireTime = 0.35f;
	private float mLaserTime = 0.35f;

	//upgrade multiplier for upgrading tower weapons
	public float mTowerUpgradeMultiplier = 1.0f;

	//multishot
	public bool mMultiShot = false;

	//radius upgrade
	public float mSplashRadiusMultiplier;
	//number of shots for spread
	public int mNumShots;
	public float mFireAngle;

	//for SOUND
	public AudioClip mTowerShot ;


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

	public List<GameObject> dijkstraPath(GameObject start, List<GameObject>paths)
	{
		GameObject walkablePlaneParent = GameObject.Find ("UnitsApproved");
		
		List<GameObject> currPath = new List<GameObject> ();
		BinaryHeap bhp = new BinaryHeap (200);
		Transform [] ts = walkablePlaneParent.GetComponentsInChildren<Transform> ();
		for(int i = 0; i < ts.Length; i++)
		{
			if(ts[i].gameObject.tag != "Taken" && ts[i].gameObject.name != walkablePlaneParent.name)
			{
				GridForUnits gfn = ts[i].gameObject.GetComponent<GridForUnits>();
				if(gfn)
				{
					gfn.distance = 10000;
					gfn.hasBeenVisited = false;
				}
			}
		}
		if(start)
		{
			GridForUnits gfnStart = start.GetComponent<GridForUnits> ();
			gfnStart.distance = 0;
			
			//minVisits.Enqueue(start);
			gfnStart.hasBeenVisited = true;
			bhp.add(start);
			//Debug.Log("bhp size: " + bhp.reserved);
			//Debug.Log(minVisits.Count);
			
			while(!bhp.empty())//minVisits.Count != 0)
			{
				GameObject curr = bhp.extractMin();//minVisits.Peek();
				//if(!curr)
				//	Debug.Log ("hello");
				//Debug.Log (curr.GetComponent<GridForUnits>().distance);
				GridForUnits visitCurr = curr.GetComponent<GridForUnits>();
				visitCurr.hasBeenVisited = true;
				for(int neigh = 0; neigh < visitCurr.nextTo.Length; neigh++)
				{
					int currentDist = visitCurr.distance;
					if(visitCurr.isAvailable)
					{
						GridForUnits nodesToVisitGFN = visitCurr.nextTo[neigh].GetComponent<GridForUnits>();
						int oldDis = nodesToVisitGFN.distance;
						int newDis = currentDist + 1;
						if(newDis < oldDis)
						{
							nodesToVisitGFN.distance = newDis;
							nodesToVisitGFN.previous = curr;
							bhp.add (visitCurr.nextTo[neigh]);
						}
					}

				}
				/*
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
				}*/
			}
			
			GameObject end = GameObject.Find("UnitsAllowedFinish");
			GridForUnits checkend = end.GetComponent<GridForUnits>();
			if(end && checkend.hasBeenVisited)
			{
				while(end.name != start.gameObject.name)
				{
					currPath.Add(end);
					GridForUnits gfnEnd = end.GetComponent<GridForUnits>();
					end = gfnEnd.previous;
				}
				currPath.Add(end);
				mAllPathsBloked = false;
			}
			else
			{
				currPath = paths;
				mAllPathsBloked = true;
			}
			//Debug.Log(currPath.Count);
		}
		Debug.Log (currPath.Count);
		return currPath;
	}

	// Use this for initialization
	void Start () 
	{
		int enemyMask = 1 << mEnemeyLayter;
		int placementMask = 1 << mPlacement;
		gameover = false;
		theWeapon = 1;
		theTowerWeapon = 0;
		mPathWaysChanged = false;
		//make this as a public function to easily use
		mStart = GameObject.Find ("UnitsAllowedStart");
		mEnemyPath = dijkstraPath (mStart,mEnemyPath);
		mStart2 = GameObject.Find ("UnitsAllowed18");
		mEnemyPath2 = dijkstraPath (mStart2,mEnemyPath2);
		mStart3 = GameObject.Find ("UnitsAllowed1");
		mEnemeyPath3 = dijkstraPath (mStart3, mEnemeyPath3);
		Debug.Log ("thepath3" + mEnemeyPath3.Count);
		mBuyPlane = enemyMask | placementMask;
		mTowerUpgradeMultiplier = 1.0f;
		mSplashRadiusMultiplier = 1.0f;
		mMultiShot = false;
		mNumShots = 1;
	}


	private void handleBuyModeInput() {

	}

	private void handleShootModeInput() {

	}


	// Update is called once per frame
	void Update () 
	{
		if (mInputMode == INPUT_MODE_SHOOT) {
			handleShootModeInput();
		} else {
			handleBuyModeInput();
		}

		if (mPathWaysChanged)
		{
			mPathWaysChanged = false;
		}
		if(gameover)
		{
			NewSpawnWaves sw = gameObject.GetComponent<NewSpawnWaves>();
			sw.waveDuration = -1.0f;
			sw.numEnemiesRemaining = 1000;

		}
		if(buyMode && !gameover)
		{
			if(Input.GetMouseButton(0)){
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if(Physics.Raycast(ray,out hit,1000,mBuyPlane))
			{
				if(mLastPlane)
				{
					mLastPlane.renderer.material = mOldMaterial;
					mLastPlane.renderer.enabled = false;
					Destroy(mHoverObject);
				}
				if(hit.collider.gameObject.tag != "Enemy")
				{
					mLastPlane = hit.collider.gameObject;
				//oldMaterial = lastPlane.renderer.material;
					GridForUnits lsp = mLastPlane.GetComponent<GridForUnits>();
					if(lsp)
					{
						mLastPlane.renderer.enabled = true;
						if(lsp.isAvailable)
						{
							int cstTBy = mWeapons[theWeapon].GetComponent<Weapons>().cost;
							GameObject towor = GameObject.FindGameObjectWithTag("TheTower");
							int rsrce = towor.GetComponent<TowerStats>().mResources;
							if((rsrce - cstTBy) >= 0)
								mLastPlane.renderer.material = mHoverMaterial;
							else
								mLastPlane.renderer.material = mCantBuyMaterial;
						}
						else
						{
							Weapons gtpgrd = lsp.whatsInside.GetComponent<Weapons>();
							GameObject whtTpgrd = gtpgrd.upgradeIt;
							if(whtTpgrd != null)
							{
								int csy = whtTpgrd.GetComponent<Weapons>().cost;
								GameObject tweer = GameObject.FindGameObjectWithTag("TheTower");
								int reesrc = tweer.GetComponent<TowerStats>().mResources;
								if(reesrc - csy >= 0)
									mLastPlane.renderer.material = mHoverMaterial;
								else
									mLastPlane.renderer.material = mCantBuyMaterial;
							}
						}
					
						if(mLastPlane != null && mLastPlane.GetComponent<GridForUnits>().whatsInside != null)
						{
							GameObject hoverUpgrade = mLastPlane.GetComponent<GridForUnits>().whatsInside.GetComponent<Weapons>().upgradeDummy;
							if(hoverUpgrade)
							{
								mHoverObject = (GameObject)Instantiate(hoverUpgrade, mLastPlane.transform.position, 
						     	                                 Quaternion.identity);
							}
						}
						else
						{
							mHoverObject = (GameObject)Instantiate(mDummyWeapons[theWeapon], mLastPlane.transform.position, 
						                                      Quaternion.identity);
						}
					}
				}
				else
				{
					//lastPlane.renderer.enabled = false;
					mLastPlane = null;
				}
			}
			else
			{
				if(mLastPlane)
				{
					mLastPlane.renderer.material = mOldMaterial;
					mLastPlane.renderer.enabled = false;
					mLastPlane = null;
						Destroy(mHoverObject);
				}
			}
		}
			if(Input.GetMouseButtonUp(0) && mLastPlane)
			{
				mLastPlane.renderer.enabled = false;
				Destroy(mHoverObject);
				Debug.Log("HELLO THERE");
				mEnemyGrid = mLastPlane.GetComponent<GridForUnits>();
				if(mEnemyGrid.isAvailable)
				{
					if(mWeapons.Length > 0)
					{	
						mLastPlane.gameObject.tag = "Taken";
						mEnemyGrid.isAvailable = false;
						//theTaken.gameObject.tag = "Taken";
						dijkstraPath(mStart,mDummyPaths);
						Debug.Log (mAllPathsBloked);
						if(!mAllPathsBloked)
						{
							//lastPlane.gameObject.tag = "NotTaken";
							int costToBuy = mWeapons[theWeapon].GetComponent<Weapons>().cost;
							GameObject twr = GameObject.FindGameObjectWithTag("TheTower");
							int resource = twr.GetComponent<TowerStats>().mResources;
							Debug.Log(resource);
							if((resource - costToBuy) >= 0)
							{
								twr.GetComponent<TowerStats>().mResources -= costToBuy;
								Vector3 spawn = mLastPlane.transform.position;
								GameObject currWeapon = (GameObject)Instantiate(mWeapons[theWeapon], spawn, 
						                                                	Quaternion.identity);


							//temp.transform.localEulerAngles = new Vector3(0.0f, Random.Range(0,360), 0.0f);
								mEnemyGrid.whatsInside = currWeapon;
								mEnemyGrid.isAvailable = false;
								mLastPlane.gameObject.tag = "Taken";

								//wall specific placement;
								if(currWeapon.tag == "WallWeapon")
								{
									wallObjectSurvive wos = currWeapon.GetComponent<wallObjectSurvive>();
									wos.planeItsOn = mLastPlane.gameObject;
								}
							//GameObject start = GameObject.Find ("UnitsAllowedStart");
								mTheTaken = mLastPlane;
								mTheTaken.gameObject.tag = "Taken";
								mPathWaysChanged = true;
								if(mEnemeyPath3.Contains(mTheTaken))
								{
									mEnemeyPath3 = dijkstraPath(mStart3,mEnemeyPath3);
								//thePathsHaveChanged = true;
								//Debug.Log("Change Paths");
								}
								if(mEnemyPath2.Contains(mTheTaken))
								{
									mEnemyPath2 = dijkstraPath(mStart2,mEnemyPath2);

								//thePathsHaveChanged1 = true;
								}
								NewSpawnWaves nsw = gameObject.GetComponent<NewSpawnWaves>();
								if(nsw.allBlue)
								{
									if(mEnemyPath.Contains(mTheTaken))
									{
										mEnemyPath = dijkstraPath(mStart,mEnemyPath);
										
										//thePathsHaveChanged1 = true;
									}
								}
							}
							else{
								mLastPlane.gameObject.tag = "NotTaken";
								mEnemyGrid.isAvailable = true;

							}
						//thePathsHaveChanged = false;
						}
						else
						{
							mLastPlane.gameObject.tag = "NotTaken";
							mEnemyGrid.isAvailable = true;
							//theTaken.gameObject.tag = "NotTaken";
						}
					}
				}
				else
				{
					if(mEnemyGrid.whatsInside != null)
					{
						GameObject gu = mEnemyGrid.whatsInside;
						Weapons getUpgrade = mEnemyGrid.whatsInside.GetComponent<Weapons>();
						GameObject whatToUpgrade = getUpgrade.upgradeIt;
						if(whatToUpgrade != null)
						{
							int costToBuy = whatToUpgrade.GetComponent<Weapons>().cost;
							GameObject twr = GameObject.FindGameObjectWithTag("TheTower");
							int resource = twr.GetComponent<TowerStats>().mResources;
							if(resource - costToBuy >= 0)
							{
								twr.GetComponent<TowerStats>().mResources -= costToBuy;
								Vector3 tempPos = mLastPlane.transform.position;
								Quaternion tempRot = getUpgrade.transform.rotation;
								Destroy(gu);
								mEnemyGrid.whatsInside = null;
								GameObject currWeapon = (GameObject)Instantiate(whatToUpgrade, 
							                                                tempPos, tempRot);
								if(currWeapon.tag == "WallWeapon")
								{
									wallObjectSurvive wos = currWeapon.GetComponent<wallObjectSurvive>();
									wos.planeItsOn = mLastPlane;
								}
								mEnemyGrid.whatsInside = currWeapon;
							}
						}
					}
				}
			}
		}

		if(shootMode && !gameover)
		{
			GameObject theTower = GameObject.FindGameObjectWithTag("TheTower");	
			float fireRate = theTower.GetComponent<TowerStats>().mFireRate;
			float lastFired = theTower.GetComponent<TowerStats>().mLastFired;
			int playerHealth = theTower.GetComponent<TowerStats>().mHealth;
			if(Input.GetMouseButtonDown(0) && !mIsClickDown)
			{
				if(mTowerWeapons.Length > 0)
				{
					Debug.Log("In onDown");
					mClickY = Input.mousePosition.y;
					mClickX = Input.mousePosition.x;
					mIsClickDown = true;
					Plane playerPlane = new Plane(Vector3.up, transform.position);
					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				
					float hitdist;
					if(playerPlane.Raycast(ray, out hitdist))
					{
						targetPosition = ray.GetPoint(hitdist);
						//Debug.Log(targetPosition);
					}
				}
			} else if (Input.GetMouseButtonUp(0) && Time.time - lastFired >= fireRate && playerHealth > 0 && theTowerWeapon < mTowerWeapons.Length-1) {
				Debug.Log("In onUp");
				mIsClickDown = false;
				mReleaseY = Input.mousePosition.y;
				/** Calculate the delta Y position of the mouse and get the angle */
				float maxAngleScreenRatio = Screen.height / 2;
				float deltaY = mReleaseY - mClickY;
				if (deltaY > maxAngleScreenRatio) {
					deltaY = maxAngleScreenRatio;
				} else if (deltaY < -maxAngleScreenRatio) {
					deltaY = -maxAngleScreenRatio;
				}
				mFireAngle = deltaY / maxAngleScreenRatio * MAX_FIRE_ANGLE;
				/** Get the tower */
				if(!mMultiShot)
				{
					//towerAmmo = (GameObject)Instantiate(towerWeapons[theTowerWeapon], 
				      //                             TOWER_FIRE_VECTOR,
				        //                          Quaternion.identity);
					//towerAmmo.GetComponent<TowerAmmoStats>().mDamage *= upgradeTowerMult;

					/** Create the cannon shot */
					//FireTowersBasicAmmo cannon = towerAmmo.GetComponent<FireTowersBasicAmmo>();
					//cannon.typeOfAmmo = theTowerWeapon;
					//cannon.radius *= theRadiusMult;
					//Vector3 dir = targetPosition - TOWER_FIRE_VECTOR;
					//cannon.dir = dir.normalized;
					//cannon.mAngle = fireAngle;
					spawnSpreadShots(mNumShots,targetPosition,mFireAngle);
				}
				else
				{
					StartCoroutine(spawnMultiShots(3,targetPosition,mFireAngle));
				}
				theTower.GetComponent<TowerStats>().mLastFired = Time.time;
			} else if(Input.GetMouseButton(0) && theTowerWeapon == mTowerWeapons.Length - 1) {
				if(mLaserFireTime <= 0.0f)
				{
					//fire laser
					GameObject tmpAmmo = (GameObject)Instantiate(mTowerWeapons[theTowerWeapon], TOWER_FIRE_VECTOR,
					                                             Quaternion.identity);
					tmpAmmo.transform.LookAt(targetPosition);
					mLaserFireTime = mLaserTime;
				}
				mLaserFireTime -= Time.deltaTime;
				if(mTowerWeapons.Length > 0)
				{
					mClickY = Input.mousePosition.y;
					Plane playerPlane = new Plane(Vector3.up, transform.position);
					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
					
					float hitdist;
					if(playerPlane.Raycast(ray, out hitdist))
					{
						targetPosition = ray.GetPoint(hitdist);
						//Debug.Log(targetPosition);
					}
				}
			} else if (mIsClickDown && theTowerWeapon != mTowerWeapons.Length - 1) {
				//Plane playerPlane = new Plane(Vector3.up, transform.position);
				//Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				
				//float hitdist;

				//Vector3 newTarget = new Vector3();
				//if(playerPlane.Raycast(ray, out hitdist))
				//{
				//	newTarget = ray.GetPoint(hitdist);
				//}
//				float deltaX = targetPosition.x - newTarget.x;
				float deltaX = (mClickX - Input.mousePosition.x) / 4;
				float deltaZ = deltaX * Mathf.Acos(targetPosition.z / targetPosition.x);
				mClickX = Input.mousePosition.x;
				targetPosition.z -= deltaX;

				float maxAngleScreenRatio = Screen.height / 2;
				float deltaY = Input.mousePosition.y - mClickY;
				if (deltaY > maxAngleScreenRatio) {
					deltaY = maxAngleScreenRatio;
				} else if (deltaY < -maxAngleScreenRatio) {
					deltaY = -maxAngleScreenRatio;
				}
				mFireAngle = deltaY / maxAngleScreenRatio * MAX_FIRE_ANGLE;
				//Debug.Log("Fire Angle: " + fireAngle);
				Vector3 dir = targetPosition - mTowerAmmoSpawn.position;
				dir = dir.normalized;
				dir.y += Mathf.Sin(mFireAngle);
				dir = dir.normalized;
				float yAccel = FireTowersBasicAmmo.ACCEL_GRAVITY;
				float yVelocity = 2.0f * dir.y;
				float yPosInit = mTowerAmmoSpawn.position.y;//TOWER_FIRE_Y;
				float timeToHitPlus = (-yVelocity + Mathf.Sqrt(yVelocity * yVelocity - 4 * (yAccel / 2) * yPosInit)) / yAccel;
				float timeToHitMinus = (-yVelocity - Mathf.Sqrt(yVelocity * yVelocity - 4 * (yAccel / 2) * yPosInit)) / yAccel;
				float timeToHit = Mathf.Max(timeToHitPlus, timeToHitMinus) / 90.0f;
				//Debug.Log( "Time to hit (max): " + timeToHit);
				Vector3 hitLocation = new Vector3(mTowerAmmoSpawn.position.x + dir.x * timeToHit * 100.0f, 0, mTowerAmmoSpawn.position.z + dir.z * timeToHit * 100.0f);
				//Debug.Log ("x, y, z: " + hitLocation.x + ", " + hitLocation.y + ", " + hitLocation.z);
				Destroy (mTargetLocation);
				Destroy (mTargetPole);
				Destroy (mTargetLine);
				mTargetLocation = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				mTargetLocation.renderer.material = mTargetMaterial;
	//			mTargetPole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
				mTargetLine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
				Destroy(mTargetLine.collider);
				float hitDistX = Mathf.Abs(hitLocation.x - mTowerAmmoSpawn.position.x);
				float hitDistZ = Mathf.Abs(hitLocation.z - mTowerAmmoSpawn.position.z);
				float totalHitDist = Mathf.Sqrt(hitDistX * hitDistX + hitDistZ * hitDistZ);
				float angle = Mathf.Atan(hitDistZ / hitDistX);

				//mTargetPole.transform.localScale = new Vector3(2, Mathf.Tan(fireAngle) * hitDistX / 4, 2);
				//float poleDistX = Mathf.Cos(angle) * totalHitDist / 2;
				//float poleDistZ = Mathf.Sin(angle) * totalHitDist / 2;
				//float poleX = towerAmmoSpawn.position.x + poleDistX;
				//float poleZ = towerAmmoSpawn.position.z + poleDistZ;
				//if (hitLocation.z < 0) {
				//	poleZ = -poleZ;
				//}
				//Vector3 halfWay = new Vector3(poleX, 0, poleZ);
				//mTargetPole.transform.position = halfWay;
				Vector3 lineLocation = TOWER_FIRE_VECTOR;
				mTargetLine.renderer.material.color = new Color32(255, 0, 0, 0);
				lineLocation.x = mTowerAmmoSpawn.position.x;
				lineLocation.y = 0;
				lineLocation.z = mTowerAmmoSpawn.position.z;
				if (hitLocation.z > 0) {
					angle = -angle;
				}
				mTargetLine.transform.localScale = new Vector3(400, 0.25f, 0.25f);
				mTargetLine.transform.position = lineLocation;
				mTargetLine.transform.RotateAround(Vector3.zero, Vector3.up, Mathf.Rad2Deg * angle); 
				mTargetLine.transform.position = lineLocation;
				mTargetLocation.transform.localScale = new Vector3(5, 5, 5);
				mTargetLocation.transform.position = hitLocation;
			}
		}
	}
	IEnumerator spawnMultiShots(int num, Vector3 multTarget, float multi_fireAngle)
	{
		for(int i = 0; i < num; i++)
		{
			//GameObject ta = (GameObject)Instantiate(towerWeapons[theTowerWeapon], 
			//                                    TOWER_FIRE_VECTOR,
			  //                                  Quaternion.identity);
			//ta.GetComponent<TowerAmmoStats>().mDamage *= upgradeTowerMult;
			//FireTowersBasicAmmo cannon = ta.GetComponent<FireTowersBasicAmmo>();
			//cannon.radius *= theRadiusMult;
			//cannon.typeOfAmmo = theTowerWeapon;
			//cannon.dir = multi_dir.normalized;
			//cannon.mAngle = multi_fireAngle;
			spawnSpreadShots(mNumShots, multTarget, multi_fireAngle);
			yield return new WaitForSeconds(0.2f);
		}

	}

	void spawnSpreadShots(int num, Vector3 spreadTarget, float multi_fireAngle)
	{
		Vector3 []spreadPositions = new Vector3[3];
		spreadPositions [0] = new Vector3 (spreadTarget.x, spreadTarget.y, spreadTarget.z);
		float spreadZn;
		float spreadZp;
		if((spreadTarget.z+5.0f) > 45)
			spreadZn = 45;
		else
			spreadZn = spreadTarget.z+5.0f;
		if((spreadTarget.z-5.0f) < -45)
			spreadZp = -45;
		else
			spreadZp = spreadTarget.z-5.0f;
		spreadPositions [1] = new Vector3 (spreadTarget.x, spreadTarget.y, spreadZn);
		spreadPositions [2] = new Vector3 (spreadTarget.x, spreadTarget.y, spreadZp );
		Vector3 theSpos = new Vector3 (0.0f, 0.0f, -10.0f);
		Vector3 taSpawnIt = mTowerAmmoSpawn.position;
		for(int i = 0; i < num; i++)
		{
			GameObject ta = (GameObject)Instantiate(mTowerWeapons[theTowerWeapon], 
			                                        taSpawnIt,
			                                        Quaternion.identity);
			ta.GetComponent<TowerAmmoStats>().mDamage *= mTowerUpgradeMultiplier;
			FireTowersBasicAmmo cannon = ta.GetComponent<FireTowersBasicAmmo>();
			cannon.radius *= mSplashRadiusMultiplier;
			cannon.typeOfAmmo = theTowerWeapon;
			cannon.dir = (spreadPositions[i] - mTowerAmmoSpawn.position).normalized;
			cannon.mAngle = multi_fireAngle;
			theSpos *= -1.0f;
			taSpawnIt = mTowerAmmoSpawn.position + theSpos;
			AudioListener.volume = 1.0f;
			audio.PlayOneShot(mTowerShot);
		}
		
	}

}
