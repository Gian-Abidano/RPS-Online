using System.Collections;
using System.Collections.Generic;
using TMPro;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CardGameManager : MonoBehaviour
{
    public GameState State = GameState.ChooseAttackState;
    public GameObject netPlayerPrefab;
    public CardPlayer Player1;
    public CardPlayer Player2;
    private CardPlayer damagedPlayer;
    public GameObject gameOverPanel;
    public TMP_Text winnerText;
    
    public enum GameState
    {
        NetPlayersInitialization,
        ChooseAttackState,
        AttackingState,
        SuccessAttackState,
        DrawState,
        GameOver,
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(State);
        gameOverPanel.SetActive(false);
        PhotonNetwork.Instantiate(netPlayerPrefab.name, Vector3.zero, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        switch (State)
        {
            case GameState.NetPlayersInitialization:
                if(CardNetPlayer.NetPlayers.Count == 2)
                {
                    foreach (var netPlayer in CardNetPlayer.NetPlayers)
                    {
                        if(netPlayer.photonView.IsMine)
                            netPlayer.Set(Player1);
                        else
                            netPlayer.Set(Player2);
                    }
                }
                break;
            case GameState.ChooseAttackState:
                if(Player1.AttackValue != null && Player2.AttackValue != null)
                {
                    Player1.AnimateAttack();
                    Player2.AnimateAttack();
                    Player1.isClickable(false);
                    Player2.isClickable(false);
                    State = GameState.AttackingState;
                }
                break;

            case GameState.AttackingState:
                if(Player1.InAnimation() == false && Player2.InAnimation() == false)
                {
                    damagedPlayer = GetDamagePlayer();
                    if(damagedPlayer != null)
                    {
                        damagedPlayer.DamageAnimation();
                        State = GameState.SuccessAttackState;
                    }
                    else
                    {
                        Player1.DrawAnimation();
                        Player2.DrawAnimation();
                        State = GameState.DrawState;
                    }
                }
                break;

            case GameState.SuccessAttackState:
                if(Player1.InAnimation() == false && Player2.InAnimation() == false)
                {
                    if(damagedPlayer == Player1)
                    {
                        Player1.ChangingHealth(-10);
                        Player2.ChangingHealth(5);
                    }
                    else
                    {
                        Player2.ChangingHealth(-10);
                        Player1.ChangingHealth(5);
                    }
                

                    var winner = GetWinner();
                    
                    if(winner==null)
                    {
                        ResetPlayers();
                        Player1.isClickable(true);
                        Player2.isClickable(true);
                        State = GameState.ChooseAttackState;
                    }
                    else
                    {
                        gameOverPanel.SetActive(true);
                        winnerText.text = winner == Player1 ? "You Win" : "Player 2 Win";
                        Debug.Log("The Winner is" + winner);
                        State = GameState.GameOver;
                        
                    }
                }
                break;

            case GameState.DrawState:
                if(Player1.InAnimation() == false && Player2.InAnimation() == false)
                {
                    ResetPlayers();
                    Player1.isClickable(true);
                    Player2.isClickable(true);
                    State = GameState.ChooseAttackState;
                }
                break;

        }
    }

    // IEnumerator PingCoroutine()
    // {
    //     var wait = new WaitForSeconds(1);
    //     if()
    //     {
            
    //     }
    // }

    private void ResetPlayers()
    {
        damagedPlayer = null;
        Player1.Reset();
        Player2.Reset();
    }

    private CardPlayer GetDamagePlayer()
    {
        Attack? Player1Atk = Player1.AttackValue;
        Attack? Player2Atk = Player2.AttackValue;

        if(Player1Atk == Attack.Rock && Player2Atk == Attack.Paper)
        {
            return Player1;
        }
        else if(Player1Atk == Attack.Rock && Player2Atk == Attack.Scissor)
        {
            return Player2;
        }
        else if(Player1Atk == Attack.Paper && Player2Atk == Attack.Scissor)
        {
            return Player1;
        }
        else if(Player1Atk == Attack.Paper && Player2Atk == Attack.Rock)
        {
            return Player2;
        }
        else if(Player1Atk == Attack.Scissor && Player2Atk == Attack.Rock)
        {
            return Player1;
        }
        else if(Player1Atk == Attack.Scissor && Player2Atk == Attack.Paper)
        {
            return Player2;
        }
        
        return null;
    }

    private CardPlayer GetWinner()
    {
        if(Player1.health==0)
        {
            return Player2;
        }
        else if(Player2.health==0)
        {
            return Player1;
        }
        else
            return null;
    }

    public void LoadScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public void QuitGame()
    {
        Debug.Log("Game is exiting");
        Application.Quit();
    }
}
