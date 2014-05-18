﻿using UnityEngine;
using System.Collections;

public class DestroyOnContactGrenade : MonoBehaviour {

	public float mDamage = 50.0f;
	public float radius;
	private int enemyLayer;
	// Use this for initialization
	void Start () {
		radius = gameObject.GetComponent<FireTowersBasicAmmo> ().radius;
		enemyLayer = 1 << 10;
	}
	
	// Update is called once per frame
	void Update ()
	{
	}
	
	void OnTriggerEnter(Collider collision) {
		//Debug.Log("Hit something");
		GameObject collisionObject = collision.gameObject;
		if (collisionObject.tag == "Enemy") {
			Debug.Log ("Collided with enemy");
			
			EnemyStats enemyStats = collisionObject.GetComponent<EnemyStats>();
			enemyStats.mHealth -= mDamage;
			if(enemyStats.mHealth <= 0.0f)
				enemyStats.mResources += 50;
		}
		if(collisionObject.tag == "Enemy")
		{
			Collider [] hitColliders = Physics.OverlapSphere(transform.position, radius, enemyLayer);
			Debug.Log("On destroy" + hitColliders.Length);
			for(int i = 0; i < hitColliders.Length; i++)
			{
				if(hitColliders[i].gameObject.tag == "Enemy")
				{
					float proximity = (transform.position - hitColliders[i].gameObject.transform.position).magnitude;
					if(proximity < radius)
					{
						float effect = 1.0f - (proximity/radius);
						Debug.Log ("Enemy at " + proximity + " away!" + " damage: " + effect);
						EnemyStats es = hitColliders[i].gameObject.GetComponent<EnemyStats>();
						if(es)
						{
							es.mHealth -= mDamage * effect;
							if(es.mHealth <= 0.0f)
								es.mResources += 50;
						}
					}
				}
			}
			Destroy (gameObject);
		}
		//do physic.overlap here

	}
}