using UnityEngine;

public class StartGame : Popup
{
    public void Begin()
    {
        Punch();
        Decay = true;
        SceneLoader sceneLoader = FindAnyObjectByType<SceneLoader>();
        sceneLoader.StartCoroutine(sceneLoader.ChangeScene("Lobby", 0));

        PlayerMovement playerMovement = FindAnyObjectByType<PlayerMovement>();
        Mouse          mouse          = FindAnyObjectByType<Mouse>();
        
        playerMovement.Pause(false);
        playerMovement.CanMove = true;
        mouse.HideMouse();
    }
}
