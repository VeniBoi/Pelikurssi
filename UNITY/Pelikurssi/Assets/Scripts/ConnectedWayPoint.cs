using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System.Text;

public class ConnectedWayPoint : MonoBehaviour {

	[SerializeField]
	protected float _connectivityRadius = 50f;

	List<ConnectedWayPoint> _connections;

	public void Start()
	{
		//Grab all waypoint objects in scene
		GameObject[] allWayPoints = GameObject.FindGameObjectsWithTag("Waypoint");

		//Create a list of waypoints i can refer to later.
		_connections = new List<ConnectedWayPoint>();

		//Check if they're a connected waypoint.
		for(int i = 0; i < allWayPoints.Length; i++)
		{
			ConnectedWayPoint nextWayPoint = allWayPoints[i].GetComponent<ConnectedWayPoint>();

			//i.e we found a waypoint
			if(nextWayPoint != null)
			{
				if (Vector3.Distance(this.transform.position, nextWayPoint.transform.position) <= _connectivityRadius && nextWayPoint != this)
				{
					_connections.Add(nextWayPoint);
				}
			}
		}
	}

	

	public ConnectedWayPoint NextWaypoint(ConnectedWayPoint previousWaypoint)
	{
		if (_connections.Count == 0)
		{
			//No waypoints? Return null and complain
			
			return null;
		}
		else if(_connections.Count == 1 && _connections.Contains(previousWaypoint))
		{
			//Only one waypoint and it's the previous? Just use that.
			return previousWaypoint;
		}
		else //Otherwise find a random that's not the previous one.
		{
			ConnectedWayPoint nextWayPoint;
			int nextIndex = 0;

			do
			{
				nextIndex = Random.Range(0, _connections.Count);
				nextWayPoint = _connections[nextIndex];

			} while (nextWayPoint == previousWaypoint);

			return nextWayPoint;
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, _connectivityRadius);
	}

}


