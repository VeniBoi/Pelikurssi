using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


namespace Assets.Code
{


	public class EnemyController : MonoBehaviour
	{

		public float lookRadius = 10f;

		Transform target;
		NavMeshAgent agent;



		// Use this for initialization
		void Start()
		{

			target = PlayerManager.instance.player.transform;
			agent = GetComponent<NavMeshAgent>();


		}

		// Update is called once per frame
		void Update()
		{

			float distance = Vector3.Distance(target.position, transform.position);

			if (distance <= lookRadius)
			{

				agent.SetDestination(target.position);
				agent.speed = 8.3f;
				gameObject.GetComponent<KarhuPatrol>().enabled = false;
				StartCoroutine(stop());
			}
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, lookRadius);
		}

		IEnumerator stop()
		{
			yield return new WaitForSeconds(17);
			agent.speed = 1;
			gameObject.GetComponent<KarhuPatrol>().enabled = true;
			yield return new WaitForSeconds(3);
			agent.speed = 3.5f;

		}
	}
}
