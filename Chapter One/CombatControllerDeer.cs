using UnityEngine;
using System.Collections;
using Player;

public class CombatControllerDeer : CombatControllerMechanim {

	public GameObject bodyMeshes;
	public GameObject antlerMesh;

	void Awake () {

	}

	public override void dodgeAttack(){
		base.dodgeAttack();
		StartCoroutine(PerformEtherealDodge());
	}

	public override void counterAttack(){
		base.counterAttack();
	}

	IEnumerator PerformEtherealDodge(){

		//Find Body Meshes
		GameObject deerMesh = bodyMeshes.transform.Find("Deer").gameObject;
		GameObject magicMesh = bodyMeshes.transform.Find("Magic").gameObject;

		//Activate Mesh Effect on Body Mesh
		deerMesh.GetComponent<MeshEffect>().activated = true;

		//Hide Meshes
		deerMesh.GetComponent<SkinnedMeshRenderer>().enabled = false;
		magicMesh.SetActive(false);
		antlerMesh.GetComponent<MeshRenderer>().enabled = false;		

		u_anim.Play("Idle", -1, 0.0f);

		//Allow "invisiblity" to last for a short period
		yield return new WaitForSeconds(0.5f);

		u_anim.Play("Attack", -1, 0.2f);

		//Move Deer behind target
		Vector3 newPos = enemy.transform.position + (enemy.transform.forward * -1f * 10f);
		newPos.y = Terrain.activeTerrain.SampleHeight(newPos);
		agent.Warp(newPos);
		transform.LookAt(enemy.transform.position);

		//Reveal Meshes
		deerMesh.GetComponent<SkinnedMeshRenderer>().enabled = true;
		magicMesh.SetActive(true);
		antlerMesh.GetComponent<MeshRenderer>().enabled = true;

		yield return null;
	}

	void attatchBodyMeshes(GameObject g){
		bodyMeshes = g;
	}

	void attatchAntlerMesh(GameObject g){
		antlerMesh = g;
	}

}