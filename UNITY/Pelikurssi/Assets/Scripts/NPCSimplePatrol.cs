using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using UnityEngine.AI;



public class NPCSimplePatrol : MonoBehaviour {

	//Dictates whether the agent waits on each node.
	[SerializeField]
	bool _patrolWaiting;

	//The total time to wait at each node.
	[SerializeField]
	float _totalWaitTime = 3f;

	//The probability of switching direction.
	[SerializeField]
	float _switchProbability = 0.2f;

	//Private variables for base behaviour.
	NavMeshAgent _navMeshAgent;
	ConnectedWayPoint _currentWayPoint;
	ConnectedWayPoint _previousWayPoint;

	
	
	bool _travelling;
	bool _waiting;
	float _waitTimer;
	int _waypointsVisited;

	//Use this for initialization
	public void Start()
	{
		_navMeshAgent = this.GetComponent<NavMeshAgent>();

		if (_navMeshAgent == null)
		{
			Debug.Log("The nav mesh agent component is not attached to " + gameObject.name);
		}
		else
		{
			if (_currentWayPoint == null)
			{
				//Set it at random.
				//Grab all waypoint objects in scene.
				GameObject[] allWaypoints = GameObject.FindGameObjectsWithTag("Waypoint");

				if(allWaypoints.Length > 0)
				{
					while (_currentWayPoint == null)
					{
						int random = Random.Range(0, allWaypoints.Length);
						ConnectedWayPoint startingWaypoint = allWaypoints[random].GetComponent<ConnectedWayPoint>();

						//i.e we found a waypoint.
						if (startingWaypoint != null)
						{
							_currentWayPoint = startingWaypoint;
						}
					}

				}
				else
				{
					Debug.LogError("Failed to find any waypoints for usein the scene");
				}
			}
			SetDestination();
		}

	}

	public void Update()
	{
		//Check if we're close to the destination.
		if (_travelling && _navMeshAgent.remainingDistance <= 1.0f)
		{
			_travelling = false;
			_waypointsVisited++;

			//If we're going to wait, then wait.
			if (_patrolWaiting)
			{
				_waiting = true;
				_waitTimer = 0f;
			}
			else
			{
				
				SetDestination();
			}
		}

		//Instead if we're waiting.
		if (_waiting)
		{
			_waitTimer += Time.deltaTime;
			if(_waitTimer >= _totalWaitTime)
			{
				_waiting = false;

				
				SetDestination();
			}
		}
	}

	private void SetDestination()
	{
		if (_waypointsVisited > 0)
		{
			ConnectedWayPoint nextWayPoint = _currentWayPoint.NextWaypoint(_previousWayPoint);
			_previousWayPoint = _currentWayPoint;
			_currentWayPoint = nextWayPoint;
		}

		Vector3 targetVector = _currentWayPoint.transform.position;
		_navMeshAgent.SetDestination(targetVector);
		_travelling = true;
	}

	

}
