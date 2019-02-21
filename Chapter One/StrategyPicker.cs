using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class StrategyPicker : MonoBehaviour {

	public StrategyPickerSerializable jsonData;

	public int attackID = 0;
	public int guardID = 1;
	public int dodgeID = 2;

	public float distanceToTarget = 0.0f;

	Transform target;


	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

    //Chooses and returns a strategy
	public int selectStrategy(Transform target){
		this.target = target;
		getDistance(target.position);
		float roll = Random.Range(0f, 1f);
		float guardChance = getPriority(guardID);
		float attackChance = getPriority(attackID);
		float dodgeChance = getPriority(dodgeID);
		float prioritySum = guardChance + attackChance + dodgeChance;
		int id = -1;
		if (roll <= (guardChance / prioritySum)){
			id = 1; //Guard
		}else if (roll <= ((dodgeChance + guardChance) / prioritySum)){
			id = 2; //Dodge
		}else{
			id = 0; //Attack
		}

		resetTempPriorities();
		return id;
	}

    //Sets the temp value for a strategy
    public void SetTemp(int strategyId, float value)
    {
        jsonData.strategies[strategyId].priorityTemp = value;
    }

    //returns the cooldown for a strategy
    public float getStrategyTimer(int strategyID){
        return jsonData.strategies[strategyID].cooldown;
	}

    //returns the distance from the transform to a target
    private void getDistance(Vector3 p){
		distanceToTarget = Vector3.Distance(p, transform.position);
	}

    //returns the final priority value for a strategy
    private float getPriority(int strategyID){
		float priority = 0.0f;

        priority = jsonData.strategies[strategyID].priority + jsonData.strategies[strategyID].priorityTemp;
        
        return priority;
	}

    //clears temporary priority values for strategies
    private void resetTempPriorities(){
        foreach (StrategySerializable strategy in jsonData.strategies)
            strategy.priorityTemp = 0.0f;
	}

    //returns the base priority value for a strategy
    private float getBasePriority(int strategyID){
		float basePriority = 0.0f;

        basePriority = jsonData.strategies[strategyID].priority * getRangeModifier(strategyID);


        return basePriority;
	}

    //returns a strategy's range modifier which is used to determine the final priority value for a strategy
	private float getRangeModifier(int strategyID){
        float rangeMax = jsonData.strategies[strategyID].rangeMax;
        float rangeMin = jsonData.strategies[strategyID].rangeMin;
        if (distanceToTarget > rangeMin && distanceToTarget < rangeMax)
        {
            return ((rangeMax - distanceToTarget) / (rangeMax - rangeMin)) + 0.25f;
        }
        else
        {
            return .2f;
        }

    }
}
