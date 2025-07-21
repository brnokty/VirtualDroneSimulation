using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Direction
{
    Up,
    Down,
    Left,
    Right,
    Forward,
    Backward
}

public class UIManager : MonoBehaviour
{
    #region Singleton

   public static UIManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion
    
    public Color trueColor;
    public Color falseColor;
   public List<Image> directionFrames= new List<Image>();
   
   
   
   public void SetDirectionFrameColor(Direction direction, bool isTrue)
   {
       if (directionFrames.Count == 0)
       {
           Debug.LogWarning("Direction frames are not set in the UIManager.");
           return;
       }

       int index = (int)direction;
       if (index < 0 || index >= directionFrames.Count)
       {
           Debug.LogError("Invalid direction index: " + index);
           return;
       }

       Image frame = directionFrames[index];
       if (frame != null)
       {
           frame.color = !isTrue ? trueColor : falseColor;
       }
       else
       {
           Debug.LogError("Frame for direction " + direction + " is not assigned.");
       }
   }
   
   
}
