using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI gameStateText;
    [SerializeField] private TextMeshProUGUI instructionText;

    public static UIManager Instance;
    private Transform target;
    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(Instance);
        }
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void SetReady()
    {
        instructionText.text = "Waiting for other players to be ready";
    }
    public void SetWaitUI(GameState state, Player winner)
    {
        Debug.Log(state);
        if(state == GameState.Waiting)
        {
            if(winner == null)
            {
                gameStateText.text = "Waiting for the game to start";
                instructionText.text = "Press R to ready up";
            }
            else
            {
                gameStateText.text = $"{winner.Name} has won";
                instructionText.text = "Press R to play again";
            }

        }

        gameStateText.enabled = instructionText.enabled = state == GameState.Waiting;
    }
}
