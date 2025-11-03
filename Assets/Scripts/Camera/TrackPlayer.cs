using UnityEngine;

public class TrackPlayer : MonoBehaviour
{
	GameObject player;
	
    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }
	void LateUpdate()
	{
		if( player != null )
			transform.position = player.transform.position;
	}
}