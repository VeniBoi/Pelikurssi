using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class characterAnimator : MonoBehaviour {


	const float locomotionAnimationSmoothTime = 0.1f;
	Animator animator;
	NavMeshAgent agent;

	// Use this for initialization
	void Start () {

		agent = GetComponent<NavMeshAgent>();
		animator = GetComponent<Animator>();


	}
	
	// Update is called once per frame
	void Update () {
		float speedPercent = agent.velocity.magnitude / agent.speed;
		animator.SetFloat("speedPercent", speedPercent, locomotionAnimationSmoothTime, Time.deltaTime);
	}
}
