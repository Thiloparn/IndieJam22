using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoseScript : MonoBehaviour
{

    public float loseSpeed = 2f;
    float alpha = 0f;
    public bool losing = false;

    Image image;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (losing)
        {
            if (alpha >= 1f)
            {
                SceneManager.LoadScene("LoadingScene");
            }
            alpha += Time.deltaTime * loseSpeed;
            alpha = Mathf.Min(alpha, 1f);
            image.color = new Color(0f, 0f, 0f, alpha);
        }
    }

    public void Lose()
    {
        losing = true;
    }
}
