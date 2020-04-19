# Unity 对象池

在做游戏开发的时候我们通常会遇到一种情况，就是某个对象使用的频率非常高，但是很快又将销毁。比如在射击游戏中的子弹，一个玩家在短时间内可以发射很多的子弹，但是子弹的生命周期却是非常的短暂。如果频繁的使用`Instantiate Destroy`进行控制，势必会导致游戏的性能下降，并且在频繁生成销毁过程中也会导致生成大量的内存碎片，不利于游戏的整体性能。

对象池顾名思义是用来存储对象的池子，在我们需要某个对象的时候，可以直接从对象池中取出来，不用的时候在放回池子中，避免了频繁的创建对象和销毁对象带来的性能影响。

## 实现思路

由于在一个项目中可能存在多个不用类型的对象池子，所以采用`GoPManager`来管理多个类型的池子。并且定义`PoolType`来存放池子的基本信息，然后池子信息需要在`GoPManager`中进行注册`Register`。在注册池子信息给我们的`GoPManager`之后，程序可以在任何地方创建我们已经注册的池子`CreatePool`，在创建池子之后，池子中所有的对象默认处于未激活状态，并且默认挂在在`GoPManager`的附加物体上，可以运行游戏进行查看。

在管理不同类型的池子采用`Dictionary`进行管理，键为池子对应的Prefab。同一个池子中的对象管理采用`Queue`进行管理，使用时取出队列头部对象，使用完毕回收至队列尾部。

## 使用流程

1. 实例化一个`GoPManager`对象
2. 注册`PoolType`信息到`GoPManager`
3. 创建对象池`CreatePool`
4. 生成对象`Spawn`
5. 使用……
6. 回收对象`Recycle`

## 实现效果

![img](http://chinacjxs.ticp.net/typora/20200419100249.gif)

## Demo

新建一个游戏对象，并且添加一个控制脚本`PoolManager`

<img src="http://chinacjxs.ticp.net/typora/20200419002035.png" alt="image-20200418235635276"  />

在脚本中实例化`GopManager`，注册并创建出对象池中的对象

```c#
public class PoolManager : MonoBehaviour
{
    public List<GoPManager.PoolType> pools = new List<GoPManager.PoolType>();
    public GoPManager poolManager;
    private void Awake()
    {
        poolManager = new GoPManager(gameObject);
        poolManager.Register(pools);
        foreach (var item in pools)
            poolManager.CreatePool(item.prefab);
    }
}
```

此时运行我们游戏，可以看出我们注册的对象池已经创建完毕

![image-20200419094622694](http://chinacjxs.ticp.net/typora/20200419094631.png)

在使用时我们在玩家的开火代码中进行控制，原本的`GameObject.Instantiate`更换为`GoPManager.Spawn`即可。在对象使用完毕时使用`GoPManager.Recycle`进行对象的回收。

```c#
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
```

