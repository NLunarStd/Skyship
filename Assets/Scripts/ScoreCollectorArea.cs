using UnityEngine;
using static GameEnum;
using Unity.Netcode;
public class ScoreCollectorArea : NetworkBehaviour
{
    public PlayerTeam ownerTeam;
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent<ScorableItem>(out var item))
        {
            int scoreValue = item.GetScoreValue();
            Debug.Log($"[Server] Score collected by {ownerTeam}: {scoreValue}");

            UpdateScoreToTeam(scoreValue);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent<ScorableItem>(out var item))
        {
            int scoreValue = item.GetScoreValue();
            Debug.Log($"[Server] Item left {ownerTeam} area. Reducing score: {scoreValue}");

            UpdateScoreToTeam(-scoreValue);
        }
    }
    void UpdateScoreToTeam(int score)
    {
        switch (ownerTeam)
        {
            case PlayerTeam.Team1:
                GameManager.instance.UpdatePlayerScore(0, score);
                break;
            case PlayerTeam.Team2:
                GameManager.instance.UpdatePlayerScore(1, score);
                break;
            case PlayerTeam.Team3:
                GameManager.instance.UpdatePlayerScore(2, score);
                break;
            case PlayerTeam.Team4:
                GameManager.instance.UpdatePlayerScore(3, score);
                break;
        }
    }
}
