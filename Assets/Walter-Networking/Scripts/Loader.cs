using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader
{


    public enum Scene
    {
        MenuScene,
        GameScene,
        LoadingScene,
        LobbyScene,
        CharacterSelectScene,   //figure out how to remove this option!?
    }


    private static Scene targetScene;



    public static void Load(Scene targetScene)
    {
        Loader.targetScene = targetScene;

        SceneManager.LoadScene(Scene.LoadingScene.ToString());
    }

    public static void LoadNetwork(Scene targetScene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }

    public static void LoaderCallback()
    {
        SceneManager.LoadScene(targetScene.ToString());
    }

}