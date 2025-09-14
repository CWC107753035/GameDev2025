using UnityEngine;

public class Tralalero_Automove : MonoBehaviour
{
    [SerializeField] private AudioClip walkClip;
    private int speed = 2;  // unit
    private Vector3[] waypoints;
    private int currentIndex = 0;
    private Vector3 startPos;
    private Vector3 endPos;
    private float distance;
    private float startTime;
    private int NextTarget = 1;
    private Animator animator;
    private AudioSource audioSource;

    private float timer = 0f;

    //
    void SetNextTarget()
    {
        startPos = transform.position;
        endPos = waypoints[NextTarget];
        distance = Vector3.Distance(startPos, endPos);
        startTime = Time.time;
        switch (NextTarget)
        {
            case 1:
                animator.SetInteger("Direction", 0);
                break;
            case 2:
                animator.SetInteger("Direction", 1);
                break;
            case 3:
                animator.SetInteger("Direction", 2);
                break;
            case 0:
                animator.SetInteger("Direction", 3);
                break;
        }
    }
//
    void Start(){
        waypoints = new Vector3[]{
            new Vector3(-11.5f, 12.5f, 0f),  
            new Vector3(-6.5f, 12.5f, 0f),   
            new Vector3(-6.5f, 8.5f, 0f),    
            new Vector3(-11.5f, 8.5f, 0f)    
        };
        
        animator = GetComponent<Animator>(); //init animator
        audioSource = GetComponent<AudioSource>(); //init sound

        transform.position = waypoints[0];                  
        SetNextTarget();
    }

    void Update()
    {
        float distCovered = (Time.time - startTime) * speed;
        float finishPercent = distCovered / distance;

        transform.position = Vector3.Lerp(startPos, endPos, finishPercent);

        if (finishPercent >= 1f){
            NextTarget++;
            if (NextTarget == 4) NextTarget = 0;
            SetNextTarget();
        }
        timer += Time.deltaTime;
        if (timer >= 0.5){
            audioSource.PlayOneShot(walkClip);
            timer = 0f;
        }
    }

    
}
