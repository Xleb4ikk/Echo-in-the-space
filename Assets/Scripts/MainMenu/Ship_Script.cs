using UnityEngine;
using System.Collections;

public class Ship_Script : MonoBehaviour
{
    public float rotationSpeed = 10f;

    private bool isReady = false;

    void Start()
    {
        StartCoroutine(WaitAndEnableAction());
        Debug.Log("Update метод начал работать после задержкиddsdsdafdsgsghsdgh");
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
        // Код, который должен выполняться в Update после задержки
        Debug.Log("Update метод начал работать после задержки");
    }

    IEnumerator WaitAndEnableAction()
    {
        float delay = Random.Range(1f, 3f); // Рандомная задержка от 1 до 3 секунд
        yield return new WaitForSeconds(delay);
        isReady = true;
    }
}
