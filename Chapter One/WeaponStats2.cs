using UnityEngine;
using System.Collections;
using Player;

[RequireComponent(typeof(WeaponAttributes))]

public class WeaponStats2 : MonoBehaviour {

	public bool aimAssist = true;
	public float finalDamage = 0.0f;
	public float damage = 0.00f;
	public bool attacking = false;

	public float attributeModifier = 3.5f;

	WeaponAttributes attributes;
	Coroutine StrikeHandler;

	PlayerStats p_Stats;
	GameObject weaponAudio;

	Animator c_anim;
	GameObject weaponFX;

	public bool ikActive = false;
	public Transform weaponImpactPos;

	public bool debug = false;
	public GameObject debugSphere;
	Vector3 previousTip = Vector3.zero;
	Vector3 previousMid = Vector3.zero;
	Vector3 previousHilt = Vector3.zero;

	void Awake(){
		p_Stats = transform.root.GetComponent<PlayerStats>();
		setupAudio();
		c_anim = transform.root.GetComponent<Animator>();
		weaponFX = transform.GetChild(0).gameObject;
		attacking = false;
		attributes = GetComponent<WeaponAttributes>();
	}

	void Update(){
		if (attacking && StrikeHandler == null){
			StrikeHandler = StartCoroutine(StrikeCalculationHandler());
		}else if (!attacking && StrikeHandler != null){
			StopCoroutine(StrikeHandler);
			StrikeHandler = null;
		}
		finalDamage = getDamage();
	}

	//sets up weapon audio
	void setupAudio(){
		//TODO: Diferent audio for different types of weapons;
		weaponAudio = transform.Find("Audio").Find("Sword").gameObject;
	}

	//TODO: Calculate damage based on player stats and enemy stats
	float calculateDamage(){
		return transform.GetComponent<WeaponAttributes>().getFinalDamage() + transform.root.GetComponent<AttributeController>().getStrength();
	}

	//TODO: Play Weapon Strike/Impact Audio
	void playWeaponStrikeAudio(){
		if (weaponAudio != null){
			AudioSource audioClip = weaponAudio.transform.GetChild(Random.Range(0, weaponAudio.transform.childCount)).gameObject.GetComponent<AudioSource>();
			if (audioClip != null){
				audioClip.Play();
			}
		}
	}

	void strikeEnemy(Transform target){
		if (attacking){
			attacking = false;
			playWeaponStrikeAudio();
			target.GetComponent<CombatControllerMechanim>().strikeHandler(calculateDamage(), false, transform.root);
		}
	}

	void calculateStrikePath(){
			GameObject tip = transform.Find("Tip").gameObject;
			GameObject hilt = transform.Find("Hilt").gameObject;

			Vector3 hiltPos = hilt.transform.position;
			Vector3 tipPos = tip.transform.position;

			checkStrikeForwards(hiltPos, tipPos);

			if (previousHilt != Vector3.zero || previousTip != Vector3.zero){
				checkStrikeDiagonal(hiltPos, tipPos);
			}

			if (previousHilt != Vector3.zero || previousTip != Vector3.zero){
				checkStrikeBackwards(hiltPos, tipPos);
			}

			if (debug){
				Instantiate(debugSphere, hiltPos, Quaternion.identity);
				Instantiate(debugSphere, tipPos, Quaternion.identity);
			}			
			
	}

	IEnumerator StrikeCalculationHandler(){
		GameObject tip = transform.Find("Tip").gameObject;
		GameObject hilt = transform.Find("Hilt").gameObject;

		while (attacking){

			Vector3 hiltPos = hilt.transform.position;
			Vector3 tipPos = tip.transform.position;

			checkStrikeForwards(hiltPos, tipPos);

			//yield return null;

			if (!attacking){
				yield break;
			}

			if (previousHilt != Vector3.zero || previousTip != Vector3.zero){
				checkStrikeDiagonal(hiltPos, tipPos);
			}

			if (!attacking){
				yield break;
			}

			//yield return null;

			if (previousHilt != Vector3.zero || previousTip != Vector3.zero){
				checkStrikeBackwards(hiltPos, tipPos);
			}

			

			if (debug){
				Instantiate(debugSphere, hiltPos, Quaternion.identity);
				Instantiate(debugSphere, tipPos, Quaternion.identity);
			}

			yield return null;

		}

	}

	//ray cast from hilt/tip of sword to the previously recorded hilt/tip of sword
	void checkStrikeBackwards(Vector3 hilt, Vector3 tip){

		//Calculate strike from hilt
		RaycastHit hit;
		Vector3 fromPosition = hilt;
		Vector3 toPosition = previousHilt;
		Vector3 direction = toPosition - fromPosition;


		if(Physics.Raycast(fromPosition,direction,out hit, Vector3.Distance(fromPosition, toPosition)))
		{
			if (hit.collider.transform.root.tag == "AIEnemy"){
				print("Striked[Backward]: "+ hit.collider.gameObject.name);
				strikeEnemy(hit.collider.transform.root);
			}
		}else{
			//print("Striked[Backward]: None");
		}

		//Calculate strike from tip
		fromPosition = tip;
		toPosition = previousTip;
		direction = toPosition - fromPosition;


		if(Physics.Raycast(fromPosition,direction,out hit, Vector3.Distance(fromPosition, toPosition)))
		{
			if (hit.collider.transform.root.tag == "AIEnemy"){
				print("Striked[Backward]: "+ hit.collider.gameObject.name);
				strikeEnemy(hit.collider.transform.root);
			}
			
		}else{
			//print("Striked[Backward]: None");
		}
	}

	void checkStrikeDiagonal(Vector3 hilt, Vector3 tip){

		//Calculate strike from hilt
		RaycastHit hit;
		Vector3 fromPosition = hilt;
		Vector3 toPosition = previousTip;
		Vector3 direction = toPosition - fromPosition;


		if(Physics.Raycast(fromPosition,direction,out hit, Vector3.Distance(fromPosition, toPosition)))
		{
			if (hit.collider.transform.root.tag == "AIEnemy"){
				print("Striked[Diagonal]: "+ hit.collider.gameObject.name);
				strikeEnemy(hit.collider.transform.root);
			}
		}

		//Calculate strike from tip
		fromPosition = tip;
		toPosition = previousHilt;
		direction = toPosition - fromPosition;


		if(Physics.Raycast(fromPosition,direction,out hit, Vector3.Distance(fromPosition, toPosition)))
		{
			if (hit.collider.transform.root.tag == "AIEnemy"){
				print("Striked[Diagonal]: "+ hit.collider.gameObject.name);
				strikeEnemy(hit.collider.transform.root);
			}
			
		}
	}

	//ray cast from hilt of sword to tip of sword
	void checkStrikeForwards(Vector3 start, Vector3 end){
		RaycastHit hit;
		Vector3 fromPosition = start;
		Vector3 toPosition = end;
		Vector3 direction = toPosition - fromPosition;


		if(Physics.Raycast(fromPosition,direction,out hit, Vector3.Distance(fromPosition, toPosition)))
		{
			if (hit.collider.transform.root.tag == "AIEnemy"){
				print("Striked[Forward]: "+ hit.collider.gameObject.name);
				strikeEnemy(hit.collider.transform.root);
			}
		}else{
			//print("Striked[Forward]: None");
		}
	}

	public float getDamage(){
			return attributes.getFinalDamage();
	}

	public void AttackStart(){
		attacking = true;
		weaponFX.SetActive(true);
		testWeaponIK(true);
		//b_collider.enabled = true;
	}

	public void EffectsEnd(){
		weaponFX.SetActive(false);
	}

	public void AttackEnd(){
		attacking = false;
		previousTip = Vector3.zero;
		previousMid = Vector3.zero;
		previousHilt = Vector3.zero;
		testWeaponIK(false);
	}

	void testWeaponIK(bool start){
		if (c_anim && start){
			if(weaponImpactPos != null) {
				Debug.Log("Yiii");
                    c_anim.SetIKPositionWeight(AvatarIKGoal.RightHand,1);
                    c_anim.SetIKRotationWeight(AvatarIKGoal.RightHand,1);  
                    c_anim.SetIKPosition(AvatarIKGoal.RightHand,weaponImpactPos.position);
                    c_anim.SetIKRotation(AvatarIKGoal.RightHand,weaponImpactPos.rotation);
                } 
		}else{
			Debug.Log("Yiii End");
			c_anim.SetIKPositionWeight(AvatarIKGoal.RightHand,0);
            c_anim.SetIKRotationWeight(AvatarIKGoal.RightHand,0); 
            c_anim.SetLookAtWeight(0);
		}
	}

	/*//a callback for calculating IK
    void OnAnimatorIK()
    {
        if(c_anim) {
            
            //if the IK is active, set the position and rotation directly to the goal. 
            if(ikActive) {    

                // Set the right hand target position and rotation, if one has been assigned
                if(weaponImpactPos != null) {
                    c_anim.SetIKPositionWeight(AvatarIKGoal.RightHand,1);
                    c_anim.SetIKRotationWeight(AvatarIKGoal.RightHand,1);  
                    c_anim.SetIKPosition(AvatarIKGoal.RightHand,weaponImpactPos.position);
                    c_anim.SetIKRotation(AvatarIKGoal.RightHand,weaponImpactPos.rotation);
                } 
                
            }
            
            //if the IK is not active, set the position and rotation of the hand and head back to the original position
            else {          
                c_anim.SetIKPositionWeight(AvatarIKGoal.RightHand,0);
                c_anim.SetIKRotationWeight(AvatarIKGoal.RightHand,0); 
                c_anim.SetLookAtWeight(0);
        	}
    	} 
	}*/
}
