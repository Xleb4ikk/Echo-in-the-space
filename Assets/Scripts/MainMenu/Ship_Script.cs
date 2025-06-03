using UnityEngine;
using System.Collections;

public class Ship_Script : MonoBehaviour
{
    public float rotationSpeed = 10f;

    private bool isReady = false;

    void Start()
    {
        StartCoroutine(WaitAndEnableAction());
        Debug.Log("Update ����� ����� �������� ����� ��������ddsdsdafdsgsghsdgh");
    }

    void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        if (isReady)
        {
            MyUpdateMethod();
        }
    }

    void MyUpdateMethod()
    {
        // ���, ������� ������ ����������� � Update ����� ��������
        Debug.Log("Update ����� ����� �������� ����� ��������");
    }

    IEnumerator WaitAndEnableAction()
    {
        float delay = Random.Range(1f, 3f); // ��������� �������� �� 1 �� 3 ������
        yield return new WaitForSeconds(delay);
        isReady = true;
    }
}
