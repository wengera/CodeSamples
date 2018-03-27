using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class BotController : MonoBehaviour {

	public int id = -1;
	public GameObject target;
	public Rigidbody2D rb;
	public bool testStrategy = false;
	public bool move = false;
	public bool grounded = false;
	public float groundCheckOffset = .1f;
	public float groundCheckDistance = .5f;
	public bool jump = false;
	public bool doubleJump = false;
	public bool testForce = false;
	public float attackForce = 10f;
	public float attackDistance = 2.0f;
	public float attackHeightOffset = 2.0f;
	public float dodgeForce = 10f;
	public float movementSpeed = 1.0f;
	public float jumpPower = 1.0f;
	public float stoppingDistance = 5.0f;
	public Vector2 movementDir;
	public float distanceToTarget;
	public float strategyCooldown = 0.0f;
	public bool strategyFinished = false;
	public float interuptCooldown = 1.0f;
	public Text damageLabel;
	public Text victoryLabel;
	public float damageTaken = 0.0f;
	public int victories = 0;
	Vector2 nullVector = new Vector2(999,999);
	BotConfig b_Conf;
	Coroutine activeStrategy;
	public bool guarding = false;
	public Transform canvasObject;
	public bool immunityDamage = false;
	public bool airAbility = true;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody2D>();
		StartCoroutine(lateSetup());
		canvasObject = transform.Find("Canvas");
	}

	IEnumerator lateSetup(){
		do{
			try {
			    b_Conf = GetComponent<BotConfig>();
			}
			catch (Exception e) {
			    print("BotConfig not setup");
			}  
			yield return new WaitForSeconds(Time.deltaTime);
		}while(b_Conf == null);

		yield return null;

		print("Successfully Setup BotConfig");
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		isGrounded();
		
		if (move && grounded){
			if (distanceToTarget > (stoppingDistance + Math.Max(0.0f, target.transform.position.y - transform.position.y - 2))){
				Vector2 newVelocity = rb.velocity;
				newVelocity.x = movementDir.x * movementSpeed;
				rb.velocity = newVelocity;
			}
		}

		if (jump){
			jump = false;
			Vector2 dir = new Vector2(0, 1);
			rb.AddForce(dir * jumpPower, ForceMode2D.Impulse);
		}

		strategyCooldown -= Time.deltaTime;
	}

	void Update(){
		//updateDirection();
		damageLabel.text = "" + damageTaken;
		obstacleCheck();
		debugMoveDirection();
		updateDistance();

		if (testStrategy){
			selectStrategy();
		}

		if (getXDirectionToPlayer().x == 1){
			transform.rotation = new Quaternion(0, 0, 0, 1);
			canvasObject.localRotation = new Quaternion(0, 0, 0, 1);
		}else{
			transform.rotation = new Quaternion(0, 180, 0, 1);
			canvasObject.localRotation = new Quaternion(0, 180, 0, 1);
		}

		if (transform.position.y < -10){
			rb.velocity = Vector2.zero;
			transform.position = new Vector2(0,0);
		}
		
	}

    void OnTriggerStay2D(Collider2D other) {
		if (other.gameObject.tag == "Player"){
			getUpOnOutaThatPersonalSpace();
		}
    }

    void isGrounded(){
    	LayerMask layerMask = 1 << 9;
    	Vector2 groundCheckPos = transform.position;
    	groundCheckPos.y += groundCheckOffset;
    	RaycastHit2D hit = Physics2D.Raycast(groundCheckPos, -Vector2.up, groundCheckDistance, layerMask);
    	Debug.DrawRay(groundCheckPos, -Vector2.up * groundCheckDistance, Color.white);
    	if (hit.collider != null) {
            if (hit.collider.tag == "Terrain"){
            	grounded = true;
            	airAbility = true;
            }
        }else{
        	grounded = false;
        }
    }

    Vector2 getGroundPos(){
    	LayerMask layerMask = 1 << 9;
    	Vector2 groundCheckPos = transform.position;
    	groundCheckPos.y += groundCheckOffset;
    	RaycastHit2D hit = Physics2D.Raycast(groundCheckPos, -Vector2.up, groundCheckDistance, layerMask);
    	Debug.DrawRay(groundCheckPos, -Vector2.up * groundCheckDistance, Color.white);
    	if (hit.collider != null) {
    		Debug.Log("GroundPos: " + hit.collider.name);
            if (hit.collider.tag == "Terrain"){
            	return hit.collider.transform.position;
            }else{
            	return nullVector;
            }
        }else{
        	return nullVector;
        }
    }

    void obstacleCheck(){
    	if (grounded){
    		LayerMask layerMask = 1 << 9;
	    	Vector2 groundCheckPos = transform.position;
	    	groundCheckPos.y += groundCheckOffset;
	    	Vector2 dir = getXDirectionToPlayer();
	    	RaycastHit2D hit = Physics2D.Raycast(groundCheckPos, dir, groundCheckDistance, layerMask);
	    	Debug.DrawRay(groundCheckPos, dir * groundCheckDistance, Color.white);
	    	if (hit.collider != null) {
	    		Debug.Log("Obstacle: " + hit.collider.name);
	            if (hit.collider.tag == "Terrain"){
	            	Vector2 groundPos = getGroundPos();
	            	if (groundPos != nullVector){
	            		Vector2 impactPoint = hit.point;
	            		Vector2 newMoveDir = impactPoint - (Vector2)transform.position;
	            		newMoveDir.Normalize();
	            		movementDir = newMoveDir;
	            	}else{
			        	updateDirection();
			        }
	            		            	
	            }else{
		        	updateDirection();
		        }
	        }else{
	        	updateDirection();
	        }
    	}
    	
    }

    void debugMoveDirection(){
    	Vector2 groundCheckPos = transform.position;
	    groundCheckPos.y += groundCheckOffset;
    	Debug.DrawRay(groundCheckPos, movementDir * groundCheckDistance, Color.blue);
    }

    void getUpOnOutaThatPersonalSpace(){
    	if (strategyCooldown <= 0.0f){
    		executeStrategy(2);
    	}
    }

	void setTarget(){
		GameObject[] bots = GameObject.FindGameObjectsWithTag("Player");
		foreach (GameObject b in bots){
			if (b.GetComponent<BotController>().id != id){
				target = b;
			}
		}
	}

	void updateDirection(){
		Vector2 dir = target.transform.position - transform.position;
		movementDir.y = 0;
		if (dir.x > 0){
			movementDir.x = 1;
		}else{
			movementDir.x = -1;
		}
	}

	Vector2 getXDirectionToPlayer(){
		Vector2 dir = target.transform.position - transform.position;
		dir.y = 0;
		if (dir.x > 0){
			dir.x = 1;
		}else{
			dir.x = -1;
		}
		return dir;
	}

	void updateDistance(){
		distanceToTarget = Vector2.Distance(target.transform.position, transform.position);
	}

	void selectStrategy(){
		int strategyID = -1;
		if (strategyCooldown <= 0.0f || strategyFinished){
			if (strategyFinished){
				strategyFinished = false;
				strategyCooldown = 0.0f;
			}
			if (grounded || airAbility){
				if (!grounded){
					airAbility = false;
				}
				
				executeStrategy(strategyID);
			}
		}
	}

	void executeStrategy(int strategyID){
		if (activeStrategy != null){
			StopCoroutine(activeStrategy);
			immunityDamage = false;
		}
		strategyID = b_Conf.selectStrategy(target.transform);
		strategyCooldown = b_Conf.getStrategyTimer(strategyID);
		if (strategyID == 0){
			activeStrategy = StartCoroutine(runStrategyAttack());
		}else if(strategyID == 1){
			activeStrategy = StartCoroutine(runStrategyGuard());
		}else if(strategyID == 2){
			activeStrategy = StartCoroutine(runStrategyDodge());
		}
	}

	IEnumerator runStrategyAttack(){
		Debug.Log("Attack");
		move = true;
		transform.Find("Sprite").SendMessage("moveAnim", target.transform);
		float newStoppingDistance = stoppingDistance;	
		do{
			yield return null;
		}while(distanceToTarget >= newStoppingDistance);
		move = false;
		yield return new WaitForSeconds(.025f);
		transform.Find("Sprite").SendMessage("attackAnim", target.transform);
		Vector2 dir = target.transform.position - transform.position;
		dir.Normalize();
		
		rb.velocity = Vector2.zero;
		rb.AddForce(dir * attackForce, ForceMode2D.Impulse);
		float attackTimer = 0.15f;
		do{
			bool success = attackTarget();
			if (success){
				break;
			}
			attackTimer -= Time.deltaTime;
			yield return new WaitForSeconds(Time.deltaTime);
		}while (attackTimer > 0);

		yield return new WaitForSeconds(.55f);

		strategyFinished = true;
		yield return null;
	}

	bool attackTarget(){
    	LayerMask layerMask = 1 << 8;
    	Vector2 attackPos = transform.position;
    	attackPos.y += attackHeightOffset;
    	Vector2 attackDir = target.transform.position - transform.position;
		attackDir.Normalize();
    	RaycastHit2D[] hits;
    	hits = Physics2D.RaycastAll(attackPos, attackDir, attackDistance, layerMask);
    	Debug.DrawRay(attackPos, attackDir * attackDistance, Color.red);
    	foreach (RaycastHit2D hit in hits){
            if (hit.collider.name != gameObject.name){
            	Debug.Log("[" + gameObject.name + "]" + "Attacked: " + hit.collider.name);
            	hit.transform.SendMessage("GotHit", attackPos);
            	return true;
            }
    	}
    	return false;
    	
    }

    void GotHit(Vector2 atkPos){
    	if (!immunityDamage && !guarding){
    		damageTaken += 10f;
	    	if (activeStrategy != null){
	    		StopCoroutine(activeStrategy);
	    	}
	    	transform.Find("Sprite").SendMessage("gotHitAnim", target.transform);
			move = false;
			strategyCooldown = interuptCooldown;
			strategyFinished = false;

			Vector2 dir = (Vector2)transform.position - atkPos;
			dir.Normalize();
			rb.velocity = Vector2.zero;
			rb.AddForce(dir * attackForce * 2, ForceMode2D.Impulse);
			if (damageTaken >= 200){
				target.transform.SendMessage("winner");
				damageTaken = 0f;
			}
    	}else{
    		if (guarding){
    			executeStrategy(0);
    		}
    	}
    	
    }

    void winner(){
    	damageTaken	= 0f;
    	victories += 1;
    	victoryLabel.text = "" + victories;
    }

	IEnumerator runStrategyGuard(){
		Debug.Log("Guard");
		transform.Find("Sprite").SendMessage("guardAnim", target.transform);
		guarding = true;
		do{
			yield return null;
		}while(strategyCooldown >= 0.01f);
		guarding = false;
		yield return null;
	}

	IEnumerator runStrategyDodge(){
		Debug.Log("Dodge");
		
		

		Vector2 dir = dodgeDirection();
		if (dir.x > 0){
			dir.x = 1;
		}else{
			dir.x = -1;
		}
		dir.y = .5f;

		if (doubleJump){
			transform.Find("Sprite").SendMessage("jumpAnim", target.transform);
			doubleJump = false;
			yield return new WaitForSeconds(.1f);
			transform.Find("Sprite").SendMessage("dodgeAnim", target.transform);
			immunityDamage = true;
			rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
			airAbility = false;
			rb.AddForce(dir * dodgeForce, ForceMode2D.Impulse);
		}else{
			if (grounded){
				transform.Find("Sprite").SendMessage("dodgeAnim", target.transform);
				immunityDamage = true;
				rb.AddForce(dir * dodgeForce, ForceMode2D.Impulse);
				yield return new WaitForSeconds(0.1f);
				if (distanceToTarget <= 3f){
					float roll = UnityEngine.Random.Range(0f, 1f);
					if (roll <= 0.5f){
						b_Conf.guardPriorityTemp = 1.0f;
					}
				}
			}
		}
		

		yield return new WaitForSeconds(.15f);
		
		immunityDamage = false;
	}

	Vector2 dodgeDirection(){
		Transform worldMiddlePos = getWorldMiddlePos();
		if (Vector2.Distance(worldMiddlePos.position, transform.position) > 8.0f){
			doubleJump = true;
			return worldMiddlePos.position - transform.position;
		}else{
			return transform.position - target.transform.position;
		}
	}

	Transform getWorldMiddlePos(){
		return GameObject.Find("World").transform.Find("MapPositions").Find("Middle");
	}

	IEnumerator runStrategyPersonalSpace(){
		move = true;
		transform.Find("Sprite").SendMessage("moveAnim", target.transform);
		do{
			yield return null;
		}while(distanceToTarget >= stoppingDistance);
		move = false;
		transform.Find("Sprite").SendMessage("attackAnim", target.transform);

		yield return new WaitForSeconds(.4f);
		strategyFinished = true;
		yield return null;
	}
}
