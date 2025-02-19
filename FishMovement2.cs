using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FishMovement2 : MonoBehaviour
{
    [Header("魚のパラメータ")]
    public float minSpeed = 3.5f;           //最低速度
    public float maxSpeed = 10f;            //最高速度
    private float speed = 3f;               //初期速度
    public float rotationSpeed = 0.5f;      //回転の速さ
    private float stayTimer = 0f;           //滞留時間を計るタイマー
    public float minStayTime = 5f;          //最低滞留時間
    public float maxStayTime = 10f;         //最大滞留時間
    private float stay = 0f;                //滞留時間の閾値
    private float eatTime = 0f;             //食事時間を計るタイマー
    private float lookFeedTime = 0f;        //エサを見つめる時間を計るタイマー

    public enum State{ Thinking, Moving, Eating }   //魚の状態を管理するenum
    public State currentState = State.Thinking;     //現在の状態

    [Header("その他の設定")]
    public Vector3 movableRange = new Vector3(22f, 24f, 13f);   //移動可能範囲
    private Vector3 targetPosition;         //移動先等のポジション保存用
    public Button feedButton;               //エサのボタン
    private GameObject feedObject;          //エサオブジェクト
    
    private Rigidbody rb;                   //リジッドボディー登録用

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Think();
    }

    void Update()
    {
        //移動の制限
        Vector3 clampedPosition = new Vector3
        (
            Mathf.Clamp(transform.position.x, -movableRange.x, movableRange.x),
            Mathf.Clamp(transform.position.y, 0.5f, movableRange.y),
            Mathf.Clamp(transform.position.z, -movableRange.z, movableRange.z)
        );
        transform.position = clampedPosition;

        //X軸・Z軸を徐々に0度へ戻す
        Quaternion fishRotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, fishRotation, rotationSpeed * Time.fixedDeltaTime);
        
        //現在の状態別の処理(常時)
        switch(currentState)
        {
            //思考状態時の行動
            //3割の確率で再度移動先を考え始める
            case State.Thinking:
                stayTimer += Time.deltaTime;
                if(stayTimer >= stay)
                {
                    stayTimer = 0f;
                    float randomValue = Random.value;
                    if(randomValue <= 0.3f)
                    {
                        Think();
                    }
                    else
                    {
                        Move();
                    }
                }
                break;

            //移動状態時の行動
            //目的地を見つめ、一定距離まで目的地に近づくと、その場に留まり思考モードに入る 
            case State.Moving:
                transform.LookAt(targetPosition);
                float distance = Vector3.Distance(transform.position, targetPosition);
                if(distance > 1f)
                {
                    rb.linearVelocity = transform.forward * speed;
                }
                else
                {
                    Think();
                }
                break;

            //食餌状態時の行動
            //基本的には移動状態と同じだが、最初にエサを見る時間を追加している    
            case State.Eating:
                targetPosition = feedObject.transform.position;
                transform.LookAt(targetPosition);
                lookFeedTime += Time.deltaTime;

                if(lookFeedTime >= 2f)
                {
                    distance = Vector3.Distance(transform.position, targetPosition);
                    if(distance > 1f)
                    {
                        rb.linearVelocity = transform.forward * 15f;
                    }
                    else
                    {
                        rb.linearVelocity = Vector3.zero;
                        eatTime += Time.deltaTime;
                        if(eatTime >= 5f)
                        {
                            eatTime = 0f;
                            lookFeedTime = 0f;
                            stayTimer = 0f;
                            Think();
                        }
                            
                    }
                }
                break;

        }

    }

    //移動先と、停滞時間を決める
    void Think()
    {
        currentState = State.Thinking;
        rb.linearVelocity = Vector3.zero;
        targetPosition = new Vector3(Random.Range(-movableRange.x, movableRange.x), Random.Range(0.5f, movableRange.y), Random.Range(-movableRange.z, movableRange.z));
        stay = Random.Range(minStayTime, maxStayTime);
    }

    //移動速度を決める
    void Move()
    {
        currentState = State.Moving;
        speed = Random.Range(minSpeed, maxSpeed);
    }

    //エサオブジェクトを格納する
    public void Eat(GameObject feed)
    {
        currentState = State.Eating;
        stayTimer = 0f;
        feedObject = feed;
    }

}
