using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class KarhuPatrol : MonoBehaviour
{

	public NavMeshAgent agent;
	private Vector3 startPosition;
    public float wanderSpeed = 3f;
	public float wanderRange = 50f;
	bool chasing;
	public float lookRadius = 10f;
	Transform target;
	public Transform pelaaja;
	public EnemyController EnemyController;
		

	

	


	void Start()
	{
		//Get the NavMeshAgent so we can send it directions and set start position to the initial location

		//target = PlayerManager.instance.player.transform;
		agent = GetComponent("NavMeshAgent") as NavMeshAgent;
		agent.speed = wanderSpeed;
		startPosition = this.transform.position;
		InvokeRepeating("Wander", 1f, 5f);
		target = PlayerManager.instance.player.transform;
	}

	private void Update()
	{
		float distance = Vector3.Distance(target.position, transform.position);

		if (distance <= lookRadius)
		{


			EnemyController.enabled = true;
			this.enabled = false;

		}
		
	}
	
	void Wander()
	{
		//Pick a random location within wander-range of the start position and send the agent there
		Vector3 destination = startPosition + new Vector3(Random.Range(-wanderRange, wanderRange),
														  0,
														  Random.Range(-wanderRange, wanderRange));
		NewDestination(destination);

	}

	public void NewDestination(Vector3 targetPoint)
	{
		//ERROR LOCATION ******* ERROR LOCATION ******* ERROR LOCATION ******** ERROR LOCATION
		//Sets the agents new target destination to targetPoint parameter
		agent.SetDestination(targetPoint);
	}

	

	/*void StopChasing()
	{
		chasing = false;
		//Return Home then start wandering
		agent.speed = wanderSpeed;
		agent.SetDestination(startPosition);
		InvokeRepeating("Wander", 0.5f, 5f);
	}
	*/
	
	

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, wanderRange);
	}
} //Class End
