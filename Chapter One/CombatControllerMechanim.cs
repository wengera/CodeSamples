using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Player;

[RequireComponent(typeof(MoveToMechanim))]

public class CombatControllerMechanim : MonoBehaviour {

	//Script Dependencies
	Enemy creatureStats;
	PlayerCombatController player_Cmb_Cntrl;
	CombatControllerMechanim npc_Cmb_Cntrl;
	public MoveToMechanim move_controller;

	//Components
	Rigidbody r_body;
	public Animator u_anim;
	StatusController statusController;

	//Targetting
	public Transform enemy;
	public LayerMask targetLayer;

	//States
	bool aggressive = false;
	bool combat_State = false;
	bool dead = false;
	public bool ally = false;
	public bool dummy = false;

	//Cooldowns
	Dictionary<string, float> AbilityCooldowns;
	Dictionary<string, float> PhysicalAbilityCooldowns;
	
	//Movement
	public UnityEngine.AI.NavMeshAgent agent;
	public float angularSpeed = 360f;
	public bool inRange = false;
	
	//Coroutines
	Coroutine focusCoroutine;	
	Coroutine interuptCoroutine;

	//Attacking
	public Coroutine sequenceCoroutine;
	public WeaponStatsEnemy weaponScript;
	public bool attacking = false;

	//Interupts
	public int interuptCounter = 0;
	public int interuptMax = 3;

	//Misc
	AudioSource audioHandler;
	
	
	void Start () {

		loadComponents();

		StartCoroutine(loadScriptDependencies());

		setupAI();
	
		updateEnemy();

		loadMisc();

	}

	void Update(){
		
		//Check if AI is dead
		if (creatureStats.getHealth() <= 0f && !dead && !dummy){
			dead = true;
			move_controller.dead = true;
			StartCoroutine(Die());
		}

		if (!dummy){
			statusController.updateStatus();
		}

	}

	//Important function that handles the next move when returning to Idle
	//***This function requires the Idle animation to have an animation event***
	void ReturnedToIdle(){
		Debug.Log("Returned To Idle");
		if (!move_controller.friendly || ally){
			u_anim.ResetTrigger("Attack");
			if (move_controller.aggressive){
				updateEnemy();
				if (checkCDPhysical("Turn")){
					PhysicalAbilityCooldowns["Turn"] = 0.2f;
					move_controller.faceEnemy(enemy);
				}else{
					Debug.Log("Turn is on CD");
				}
				
				sequenceCoroutine = StartCoroutine(Combat());
			}
		}
	}

	//Manages when the AI can perform an attack
	//This Coroutine is refreshed every time the AI returns to Idle
	IEnumerator Combat(){
		while(true){
			if (!dead){

				r_body.velocity = Vector3.zero;

				while(!inRange){
					yield return new WaitForSeconds(0.1f);
				}

				while(!move_controller.isFacingEnemy(enemy)){
					//move_controller.faceEnemy(enemy);
					yield return new WaitForSeconds(Time.deltaTime);
				}

				if (u_anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") || u_anim.GetCurrentAnimatorStateInfo(0).IsName("Running") || u_anim.GetCurrentAnimatorStateInfo(0).IsName("Turn")){
					if (checkCDPhysical("Kick")){
						Kick();
						break;
					}else{
						Attack();
						break;
					}
				}
			}
			yield return new WaitForSeconds(0.1f);
		}
		yield return null;

	}

	//Handles Dealing Damage
	//***This function requires the Attack animation to have an animation event***
	public void Strike (AnimationEvent animationEvent)
	 {
	 	weaponScript.Strike(animationEvent);
		
		if (animationEvent.objectReferenceParameter != null){
			Debug.Log("Object Reference Not Null");
        	if (animationEvent.stringParameter == "attatchObject"){
	            GameObject obj = Instantiate(animationEvent.objectReferenceParameter, transform.root.position, transform.root.rotation) as GameObject;
	            obj.transform.parent = transform.root;
	        }else{
	        	Debug.Log("Instantiating Object...");
	        	Instantiate(animationEvent.objectReferenceParameter, transform.root.position, Quaternion.identity);
	        }
        	
        }else{
        	Debug.Log("Object Reference Null");
        }

	    int attack = animationEvent.intParameter;
	    if (attack == 0){
	    	focusCoroutine = StartCoroutine(Focus());
		}else if (attack == 1){
	    	attacking = true;
		}else if (attack == 2){
			u_anim.SetBool("Attack", false);
			attacking = false;
			if (focusCoroutine != null){
				StopCoroutine(focusCoroutine);
			}			
		}else{
			Debug.Log("Strike Do Nothing");
		}
	 }

	//Handles Receiving Damage
    public void strikeHandler(double rawDamage, bool hitTrigger, Transform enemy = null){
    	if (!dummy){
    		if (!move_controller.aggressive){
				move_controller.StartCombat();
	    	}
			if (interuptCounter > 0){
				if (interuptCounter == interuptMax){
					if (interuptCoroutine != null){
						StopCoroutine(interuptCoroutine);
					}
					
					interuptCoroutine = StartCoroutine(InteruptHandler());
				}
				interuptCounter -= 1;
				if (!canDodge()){
					double damage = rawDamage - transform.root.GetComponent<AttributeController>().getArmour();
					creatureStats.modifyHealth(damage);
					GotHit(hitTrigger, enemy);
				}else{
					//Chance to Parry only when Dodge is successful
					if (canParry()){
						counterAttack();
					}else{
						dodgeAttack();
					}
				}
			}else{
				if (interuptCoroutine != null){
					StopCoroutine(interuptCoroutine);
				}
				resetInteruptCounter();
				counterAttack();
			}
		}else{
			double damage = rawDamage - transform.root.GetComponent<AttributeController>().getArmour();
			creatureStats.modifyHealth(damage);
			GotHit(hitTrigger, enemy);
		}
    	
    }

    void Attack(){
		if (inRange && canAttackWithStatus()){
			Debug.Log("Attack");
			u_anim.SetTrigger("Attack");			
		}
	}

    public virtual void counterAttack(){
    	Debug.Log("Counter Attack");
    	u_anim.SetTrigger("Counter");
    	transform.LookAt(enemy.position);
    }

    public virtual void dodgeAttack(){
    	Debug.Log("Dodge Attack");
    	u_anim.SetTrigger("Dodge");
    	transform.LookAt(enemy.position);
    }

	void Kick(bool noCD = false){
		u_anim.SetTrigger("Kick");
		weaponScript.attacking = false;
		if (!noCD){
			PhysicalAbilityCooldowns["Kick"] = Random.Range(10,20);
		}
	}

	public void kickImpact(AnimationEvent animationEvent){
		weaponScript.attacking = false;
		if (!ally){
			player_Cmb_Cntrl.combatEffect("KnockDown");
		}else{
			npc_Cmb_Cntrl.KnockDown();
		}
		
		audioHandler.clip = animationEvent.objectReferenceParameter as AudioClip;
		audioHandler.Play();
	}

	public void counterImpact(AnimationEvent animationEvent){
		weaponScript.attacking = false;
		if (!ally){
			player_Cmb_Cntrl.combatEffect("GotHit");
		}else{
			npc_Cmb_Cntrl.GotHit(true);
		}
		audioHandler.clip = animationEvent.objectReferenceParameter as AudioClip;
		audioHandler.Play();
	}

	IEnumerator Focus(){
		transform.LookAt(enemy.transform.position);
		float maxAngle = 90f;
		Vector3 startDirection = transform.forward;
		float angleDifference = getAngleToEnemy(startDirection);
		float timer = 1f;
		while (timer > 0.0f){

			angleDifference = getAngleToEnemy(startDirection);

			if (Mathf.Abs(angleDifference) <= maxAngle){
				transform.LookAt(enemy.transform.position);
			}
			
			timer -= Time.deltaTime;

			yield return null;
		}

		yield return null;

	}

	public void GotHit(bool hitTrigger, Transform enmy = null){
		Transform audioVoice = transform.Find("Audio").Find("Voice");
		int audioIndex = Random.Range(0, audioVoice.childCount);
		Debug.Log("Audio Index: " + audioIndex);
		audioVoice.GetChild(audioIndex).GetComponent<AudioSource>().Play();

		if (weaponScript != null){
			weaponScript.stopAttacking();
		}
		
		
		statusController.updateStatus(Status.InteruptPhysical);
		if (!u_anim.GetCurrentAnimatorStateInfo(0).IsName("Hit") && hitTrigger){
			if (enmy != null){
				move_controller.updateTurnAngle(enemy);
			}else{
				u_anim.SetFloat("TurnAngle", 0f);
			}

			u_anim.SetTrigger("Hit");
		}	
	}

	public void Interupt(){
		StopCoroutine(Combat());
		u_anim.SetBool("Attack", false);
		statusController.updateStatus(Status.InteruptPhysical);
		u_anim.SetTrigger("Hit");
	}

	public void KnockDown(){
		StopCoroutine(Combat());
		u_anim.SetBool("Attack", false);
		attacking = false;
		statusController.updateStatus(Status.KnockedDown);
		weaponScript.stopAttacking();
		if (!u_anim.GetCurrentAnimatorStateInfo(0).IsName("KnockDown")){
			u_anim.SetTrigger("KnockDown");
		}
	}

	private float getAngleToEnemy(Vector3 startDirection){
		Vector3 selfToEnemyDir = enemy.position - transform.position;
		float angle = Vector3.Angle(startDirection, selfToEnemyDir);

    	return angle;
	}

	public bool canAttackWithStatus(){
	 	return statusController.canAttack();
	}

	bool canDodge(){
		float dodgeChance = transform.root.GetComponent<AttributeController>().getDodge();
		if (Random.Range(0.0f, 1.0f) <= dodgeChance){
			return true;
		}else{
			return false;
		}
	}

	bool canParry(){
		if (!u_anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && !u_anim.GetCurrentAnimatorStateInfo(0).IsName("Attack_Counter")){
			float parryChance = transform.root.GetComponent<AttributeController>().getParry();
			if (Random.Range(0.0f, 1.0f) <= parryChance){
				if (Vector3.Distance(transform.position, enemy.position) < 7.0f){
					return true;
				}else{
					return false;
				}
				
			}else{
				return false;
			}
		}else{
			return false;
		}
		
	}

	private bool checkCDPhysical(string atk){
		if (PhysicalAbilityCooldowns.ContainsKey(atk)){
		}
		return !PhysicalAbilityCooldowns.ContainsKey(atk);
	}

	private bool checkCD(string atk){
		if (AbilityCooldowns.ContainsKey(atk)){
		}
		return !AbilityCooldowns.ContainsKey(atk);
	}

	private void resetInteruptCounter(){
    	interuptCounter = interuptMax;
    }

    IEnumerator InteruptHandler(){
    	yield return new WaitForSeconds(5);
    	resetInteruptCounter();
    	yield return null;
    }

	IEnumerator UpdateCooldowns(){

		Debug.Log("Start Updating Cooldowns");

		while (move_controller.aggressive == false){
			//Debug.Log("Waiting For Aggressive");
			yield return null;
		}

		Debug.Log("Updating Cooldowns");

		while (true){
			if (AbilityCooldowns.Count > 0){
				Debug.Log(AbilityCooldowns.ToString());
				List<string> keys = new List<string> (AbilityCooldowns.Keys);
 				foreach (string key in keys) {
					Debug.Log("Key: " + key);
					if (AbilityCooldowns[key] <= 0f){
						AbilityCooldowns.Remove(key);
					}else{
						AbilityCooldowns[key] = AbilityCooldowns[key] - Time.deltaTime;
					}
					yield return null;
				}
			}
			if (PhysicalAbilityCooldowns.Count > 0){
				Debug.Log(PhysicalAbilityCooldowns.ToString());
				List<string> keys = new List<string> (PhysicalAbilityCooldowns.Keys);
 				foreach (string key in keys) {
					Debug.Log("Key: " + key + "[" + PhysicalAbilityCooldowns[key] + "]");
					if (PhysicalAbilityCooldowns[key] <= 0f){
						PhysicalAbilityCooldowns.Remove(key);
					}else{
						PhysicalAbilityCooldowns[key] = PhysicalAbilityCooldowns[key] - Time.deltaTime;
					}
					yield return null;
				}
			}
			yield return new WaitForSeconds(Time.deltaTime);
		}
		yield return null;
	}

	void attachWeapon(GameObject w){
		weaponScript = w.GetComponent<WeaponStatsEnemy>();
		weaponScript.combatController = this;
	}

	private void loadComponents(){
		r_body = GetComponent<Rigidbody>();
		agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
		u_anim = GetComponent<Animator>();
		statusController = new StatusController(u_anim);
	}

	IEnumerator	loadScriptDependencies(){

		try {
		    move_controller = gameObject.GetComponent<MoveToMechanim>();
		    Debug.Log("Success! MoveToMechanim Loaded");
		} catch {
		    Debug.Log("MoveToMechanim failed to load!");
		}

		try {
		    creatureStats = GetComponent<Enemy>();
			Debug.Log("Success! Enemy Script Loaded");
		} catch {
		    Debug.Log("Enemy Script failed to load!");
		}

		while(true){
			try {
			    move_controller = gameObject.GetComponent<MoveToMechanim>();
			    Debug.Log("Success! MoveToMechanim Loaded");
			    break;
			} catch {
			    Debug.Log("MoveToMechanim failed to load!");
			}

			yield return null;
		}

		while(true){
			try {
			    creatureStats = GetComponent<Enemy>();
				Debug.Log("Success! Enemy Script Loaded");
				break;
			} catch {
			    Debug.Log("Enemy Script failed to load!");
			}

			yield return null;
		}

		yield return null;

	}

	private bool loadScriptsFinished(){
		if (move_controller == null){
			return false;
		}else if(creatureStats == null){
			return false;
		}

		return true;
	}

	private void updateEnemy(){
		if (!ally){
			enemy = GameObject.Find("Player").transform;
			player_Cmb_Cntrl = enemy.GetComponent<PlayerCombatController>();
		}else{
			if (move_controller.enemy != null){
				enemy = move_controller.enemy;
				npc_Cmb_Cntrl = enemy.GetComponent<CombatControllerMechanim>();
			}
		}		
	}

	private void setupAI(){
		if (!dummy){
			//Setup Cooldowns
			AbilityCooldowns = new Dictionary<string, float>();
			PhysicalAbilityCooldowns = new Dictionary<string, float>();			
			
			interuptCounter = interuptMax;

			//Start Attack Coroutine
			if (!move_controller.friendly){
				sequenceCoroutine = StartCoroutine(Combat());
			}

			StartCoroutine(UpdateCooldowns());
		}
	}

	private void loadMisc(){
		audioHandler = transform.Find("Audio").GetComponent<AudioSource>();
	}

	IEnumerator Die(){
		StopCoroutine(Combat());

		gameObject.tag = "Loot";
		move_controller.enabled = false;
		StartCoroutine(move_controller.Game.FadeOut());
		float timer = 2.3f;

		agent.speed = 0;
		agent.angularSpeed = 0;

		creatureStats.enabled = false;
		u_anim.SetTrigger("isAlive");

		Destroy(transform.Find("Barrier").gameObject);
		
		do{
			timer -= Time.deltaTime;
			yield return new WaitForSeconds(Time.deltaTime);
		}while(timer > 0f);

		yield return null;
		this.enabled = false;
        

    }

}