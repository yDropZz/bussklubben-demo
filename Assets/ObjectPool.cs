using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach(Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for(int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject GetObject(string tag, Vector3 position, Quaternion rotation)
    {
        if(!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }

        if(poolDictionary[tag].Count == 0)
        {
            Debug.Log("Pool was tag " + tag + " is empty. Instantiating new object.");
            Pool pool = pools.Find(p => p.tag == tag);
            if(pool != null)
            {
                GameObject newObj = Instantiate(pool.prefab);
                newObj.SetActive(false);
                poolDictionary[tag].Enqueue(newObj);
            }
            else
            {
                Debug.LogWarning("Pool with tag " + tag + " not found in pools list.");
                return null;
            }
        }

        GameObject objectToReuse = poolDictionary[tag].Dequeue();

        if(objectToReuse == null)
        {
            Debug.LogWarning("Pool with tag " + tag + " is empty.");
            Pool pool = pools.Find(p => p.tag == tag);
            objectToReuse = Instantiate(pool.prefab);
        }

        objectToReuse.SetActive(true);
        objectToReuse.transform.position = position;
        objectToReuse.transform.rotation = rotation;

        return objectToReuse;
    }

    public void ReturnObject(string tag, GameObject objectToReturn)
    {
        if(!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return;
        }

        objectToReturn.SetActive(false);
        poolDictionary[tag].Enqueue(objectToReturn);

    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
