using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<GameDataManager>();
            }
            return instance;
        }
    }

    private static GameDataManager instance;
}