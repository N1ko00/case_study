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

    [SerializeField] private bool isUnlocked = false;
    [SerializeField] private bool usePassword = false;
    bool isDoorOpen = false;

    public AudioSource audioSource;
    public AudioClip openClip;
    public AudioClip closeClip;

    void Start()
    {
        closedPosL = doorL.localPosition;
        closedPosR = doorR.localPosition;

        if (!usePassword)
            isUnlocked = true;
    }

    void OnTriggerEnter(Collider other)
    {

        if (!other.transform.root.CompareTag("Player"))
            return;

        Debug.Log("ドアに入った");    

        if (isUnlocked)
            OpenDoor();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.transform.root.CompareTag("Player"))
            return;

        if (isUnlocked)
            CloseDoor();
    }

    public void OpenDoor()
    {

        if (isDoorOpen)
            return;

        isDoorOpen = true;

        if (openClip != null)
            audioSource.PlayOneShot(openClip);

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);


        moveCoroutine = StartCoroutine(MoveDoor(
            closedPosL + openOffsetL,
            closedPosR + openOffsetR
        ));
    }

    public void CloseDoor()
    {
        if (!isDoorOpen)
            return;

        isDoorOpen = false;

        if (closeClip != null)
            audioSource.PlayOneShot(closeClip);

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveDoor(
            closedPosL,
            closedPosR
        ));

        Debug.Log("ドアを閉める");
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

    public void SetUnlocked(bool unlocked)
    {
        isUnlocked = unlocked;
    }

    public bool IsUnlocked()
    {
        return isUnlocked;
    }
}