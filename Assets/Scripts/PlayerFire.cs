using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFire : MonoBehaviour
{
    public GameObject m_shell;
    public Transform m_firePosition;
    GoPManager pManager;
    private void Start()
    {
        pManager = FindObjectOfType<PoolManager>().poolManager;
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            GameObject go = pManager.Spawn(m_shell, m_firePosition.position, m_firePosition.rotation);
            go.GetComponent<Rigidbody>().velocity = m_firePosition.forward * 10f;
            StartCoroutine(Recycle(go, 1f));
        }
    }
    IEnumerator Recycle(GameObject obj,float t)
    {
        yield return new WaitForSeconds(t);
        pManager.Recycle(m_shell, obj);
    }
}
