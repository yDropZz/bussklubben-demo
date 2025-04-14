using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class ClubHouseGame
{
  [DllImport("__Internal")]
  private static extern void ExtGameDone();

  [DllImport("__Internal")]
  private static extern void ExtSetScore(int score);

  [DllImport("__Internal")]
  private static extern int ExtGetScore();

  [DllImport("__Internal")]
  private static extern bool ExtGameRunning();

  [DllImport("__Internal")]
  private static extern void ExtRegisterStartMethod(string script, string method);

  public static void GameDone()
  {
    if (Application.platform == RuntimePlatform.WebGLPlayer)
    {
      ExtGameDone();
    }
  }

  public static void SetScore(int score)
  {
    if (Application.platform == RuntimePlatform.WebGLPlayer)
    {
      ExtSetScore(score);
    }
  }


  public static int GetScore()
  {
    if (Application.platform == RuntimePlatform.WebGLPlayer)
    {
      return ExtGetScore();
    }
    return 0;
  }


  public static bool GameRunning()
  {
    if (Application.platform == RuntimePlatform.WebGLPlayer)
    {
      return ExtGameRunning();
    }
    return false;

  }


  public static void RegisterStartMethod(string script, string method)
  {
    if (Application.platform == RuntimePlatform.WebGLPlayer)
    {
      ExtRegisterStartMethod(script, method);
    }

  }

}
