using UnityEngine;
using System.Collections;

public class AutoDoor : MonoBehaviour
{
    [SerializeField] private Transform doorL;
    [SerializeField] private Transform doorR;

    public Vector3 openOffsetL = new Vector3(-1.5f, 0, 0);
    public Vector3 openOffsetR = new Vector3(1.5f, 0, 0);

    public float speed = 2f;

    Vector3 closedPosL;
    Vector3 closedPosR;

    Coroutine moveCoroutine;

    void Start()
    {
        closedPosL = doorL.localPosition;
        closedPosR = doorR.localPosition;
        Debug.Log("”½‰ž‚µ‚Ä‚¢‚éH");
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("ui-nnn");
        if (other.CompareTag("Player"))
        {
            OpenDoor();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CloseDoor();
        }
    }

    //”ñ“¯Šú‚â‚©‚çd‚­‚È‚¢‚æ
    void OpenDoor()
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveDoor(
            closedPosL + openOffsetL,
            closedPosR + openOffsetR
        ));
    }

    void CloseDoor()
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveDoor(
            closedPosL,
            closedPosR
        ));
    }

    IEnumerator MoveDoor(Vector3 targetL, Vector3 targetR)
    {
        while (Vector3.Distance(doorL.localPosition, targetL) > 0.01f)
        {
            doorL.localPosition = Vector3.Lerp(doorL.localPosition, targetL, Time.deltaTime * speed);
            doorR.localPosition = Vector3.Lerp(doorR.localPosition, targetR, Time.deltaTime * speed);
            yield return null;
        }

        doorL.localPosition = targetL;
        doorR.localPosition = targetR;
    }
}
