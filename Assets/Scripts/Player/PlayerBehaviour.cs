using Player;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerBehaviour : MonoBehaviour
{
    
    PlayerController _playerController;

    #region InitData

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    void Start()
    {
        SubscribeToDelegatesAndUpdateValues();
    }
    
    void SubscribeToDelegatesAndUpdateValues()
    {
    }

    #endregion


    #region Loop

    void Update()
    {
        Move();
    }

    #endregion

    void Move()
    {
        Debug.Log("Move");
    }
    
}
