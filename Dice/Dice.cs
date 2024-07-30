using UnityEngine;

public class Dice : MonoBehaviour
{
    Rigidbody rb;
    bool hasLanded;//是否落下
    bool throwm;//是否落到地面

    Vector3 initPosition; //初始位置
    int diceValue; //骰子的值

    [SerializeField] DiceSide[] diceSide;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        initPosition = transform.position;
        rb.useGravity = false;
        rb.isKinematic = true;
    }
    private void Update()
    {
        if(rb.IsSleeping() && !hasLanded && throwm)
        {
            hasLanded = true;
            rb.useGravity = false;
            rb.isKinematic = true;
            SideValueCheck();
        }
        else if (rb.IsSleeping() && hasLanded && diceValue == 0)
        {
            ReRollDice();
        }
    }

    public void RollDice()
    {
        Reset();
        if (!hasLanded && !throwm)
        {
            throwm = true;
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.AddTorque(Random.Range(0,500), Random.Range(0, 500), Random.Range(0, 500));
        }
        //else if(throwm && hasLanded)
        //{
        //    Reset();
        //}
    }
    private void Reset()
    {
        transform.position = initPosition;
        throwm = false;
        hasLanded = false;
        rb.useGravity = false;
        rb.isKinematic = true;
    }
    void ReRollDice()
    {
        Reset();
        throwm = true;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.AddTorque(Random.Range(0, 500), Random.Range(0, 500), Random.Range(0, 500));
    }

    void SideValueCheck()
    {
        diceValue = 0;
        foreach (var side in diceSide)
        {
            if (side.OnGround)
            {
                diceValue = side.SideValue();
                break;
            }     
        }
        if (diceValue != 0)
        {
            GameManager.instance.ReportDiceRolled(diceValue);
        }     
    }

}
