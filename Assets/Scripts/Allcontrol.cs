using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Allcontrol : MonoBehaviour
{
    // Start is called before the first frame update
    //单例模式
    public sealed class GameManager
    {
        private GameManager() { }
        //字段
        private static GameManager _instance;
        //属性
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameManager();
                }
                return _instance;
            }
        }
        public int scores  = 0;
    }
}
