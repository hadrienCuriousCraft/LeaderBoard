using UnityEngine;
using System.Collections;
using System;

public class HighScore : MonoBehaviour
{
    //The GUI Text object we're going to base everything off.
    public GameObject BaseGUIText;

    ///Fill in your server data here.
    private string privateKey = "Cur!ousCraftKey";
    private string TopScoresURL = "http://curious-craft.com/businesscard/TopScores.php?";

    //Don't forget the question marks!
    private string AddScoreURL = "http://curious-craft.com/businesscard/AddScore.php?";
    private string RankURL = "http://curious-craft.com/businesscard/GetRank.php?";

    //The score and username we submit
    private int highscore;
    private string username;
    private int rank;
    
    //We use this to allow the user to start the game again.
    private bool pressspace;

    ///Our public access functions
    public void Setscore(int givenscore)
    {
        highscore = givenscore;
    }

    public void SetName(string givenname){
        username = givenname;
    }

    //Our standard Unity functions
    //Called as soon as the class is activated.
    void OnEnable()
    {
        pressspace = false; // The user can't press space yet.
        StartCoroutine(AddScore(username, highscore)); // We post our scores.
    }

    void Update()
    {
        if (pressspace && Input.GetKeyUp(KeyCode.Space))
        {
            Application.LoadLevel(0); //Restart the game if space is pressed after the scores are shown.
        }
    }

    ///Our encryption function: http://wiki.unity3d.com/index.php?title=MD5
    private string Md5Sum(string strToEncrypt)
    {
        System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
        byte[] bytes = ue.GetBytes(strToEncrypt);

        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);

        string hashString = "";

        for (int i = 0; i < hashBytes.Length; i++)
        {
            hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
        }

        return hashString.PadLeft(32, '0');
    }

    ///Our Error function
    void Error()
    {
        GetComponent<GUIText>().enabled = true;
        GetComponent<GUIText>().text = "Connection error.";
        GetComponent<GUIText>().fontSize = (int)(GetComponent<GUIText>().fontSize * 0.6f);
    }

    ///Our IEnumerators
    IEnumerator AddScore(string name, int score)
    {
        string hash = Md5Sum(name + score + privateKey);
        //curious-craft.com/businesscard/AddScore.php?name=bH&score=69&hash=
        WWW ScorePost = new WWW(AddScoreURL + "name=" + WWW.EscapeURL(name) + "&score=" + score + "&hash=" + hash); //Post our score
        yield return ScorePost; // The function halts until the score is posted.
        if (ScorePost.error == null)
        {
            print("Added score apaz");
            StartCoroutine(GrabRank(name)); // If the post is successful, the rank gets grabbed next.
        }
        else
        {
            Error(); // Otherwise we use our error protocol
        }
    }

    IEnumerator GrabRank(string name)
    {
        //Try and grab the Rank
        WWW RankGrabAttempt = new WWW(RankURL + "name=" + WWW.EscapeURL(name));

        yield return RankGrabAttempt;

        if (RankGrabAttempt.error == null)
        {
            print("Grabbed rank apaz");
            //rank = 10;
            rank = int.Parse(RankGrabAttempt.text); // Assign the rank to our variable. We could also use a TryParse and write an error dialogue.
            StartCoroutine(GetTopScores()); // Get our top scores
        }
        else
        {
            Error();
        }
    }

    IEnumerator GetTopScores()
    {
        WWW GetScoresAttempt = new WWW(TopScoresURL);
        yield return GetScoresAttempt;

        if (GetScoresAttempt.error != null)
        {
            Error();
        }
        else
        {
            print("Top scores apaz");
            //Collect up all our data
            string[] textlist = GetScoresAttempt.text.Split(new string[] { "\n", "\t" }, System.StringSplitOptions.RemoveEmptyEntries);

            //Split it into two smaller arrays
            string[] Names = new string[Mathf.FloorToInt(textlist.Length/2)];
            string[] Scores = new string[Names.Length];
            for (int i = 0; i < textlist.Length; i++)
            {
                if (i % 2 == 0)
                {     
                    Names[Mathf.FloorToInt(i / 2)] = textlist[i];
                }
                else Scores[Mathf.FloorToInt(i / 2)] = textlist[i];
            }

            //Choose our text positions
            Vector2 LeftTextPosition = new Vector2(0.22f,0.85f);
            Vector2 RightTextPosition = new Vector2(0.76f, 0.85f);
            Vector2 CentreTextPosition = new Vector2(0.33f, 0.85f);

            ///All our headers
            GameObject Scoresheader = Instantiate(BaseGUIText, new Vector2(0.5f,0.94f), Quaternion.identity) as GameObject;
            Scoresheader.tag = "Score";
            Scoresheader.GetComponent<GUIText>().text = "High Scores";
            Scoresheader.GetComponent<GUIText>().anchor = TextAnchor.MiddleCenter;
            Scoresheader.GetComponent<GUIText>().fontSize = 35;

            GameObject PressSpace = Instantiate(BaseGUIText, new Vector2(0.5f, 0.08f), Quaternion.identity) as GameObject;
            PressSpace.tag = "Score";
            PressSpace.GetComponent<GUIText>().text = "Press space to try again!";
            PressSpace.GetComponent<GUIText>().anchor = TextAnchor.MiddleCenter;
            PressSpace.GetComponent<GUIText>().fontSize = 30;

            GameObject Scoreheader = Instantiate(BaseGUIText, RightTextPosition, Quaternion.identity) as GameObject;
            Scoreheader.tag = "Score";
            Scoreheader.GetComponent<GUIText>().text = "Score";
            Scoreheader.GetComponent<GUIText>().anchor = TextAnchor.MiddleCenter;
            GameObject Nameheader = Instantiate(BaseGUIText, CentreTextPosition, Quaternion.identity) as GameObject;
            Nameheader.tag = "Score";
            Nameheader.GetComponent<GUIText>().text = "Name";
            GameObject Rankheader = Instantiate(BaseGUIText, LeftTextPosition, Quaternion.identity) as GameObject;
            Rankheader.tag = "Score";
            Rankheader.GetComponent<GUIText>().text = "Rank";
            Rankheader.GetComponent<GUIText>().anchor = TextAnchor.MiddleCenter;

            //Increment the positions
            LeftTextPosition -= new Vector2(0, 0.062f);
            RightTextPosition -= new Vector2(0, 0.062f);
            CentreTextPosition -= new Vector2(0, 0.062f);

            ///Our top 10 scores
            for(int i=0;i<Names.Length;i++){
                GameObject Score = Instantiate(BaseGUIText, RightTextPosition, Quaternion.identity) as GameObject;
                Score.tag = "Score";
                Score.GetComponent<GUIText>().text = Scores[i];
                Score.GetComponent<GUIText>().anchor = TextAnchor.MiddleCenter;

                GameObject Name = Instantiate(BaseGUIText, CentreTextPosition, Quaternion.identity) as GameObject;
                Name.tag = "Score";
                Name.GetComponent<GUIText>().text = Names[i];

                GameObject Rank = Instantiate(BaseGUIText, LeftTextPosition, Quaternion.identity) as GameObject;
                Rank.tag = "Score";
                Rank.GetComponent<GUIText>().text = "" + (i + 1);
                Rank.GetComponent<GUIText>().anchor = TextAnchor.MiddleCenter;
                if (i + 1 == rank) //If the player is within the top 10 colour their score yellow.
                {
                    Score.GetComponent<GUIText>().material.color = Color.yellow;
                    Name.GetComponent<GUIText>().material.color = Color.yellow;
                    Rank.GetComponent<GUIText>().material.color = Color.yellow;
                }

                //Increment the positions again
                LeftTextPosition -= new Vector2(0, 0.062f);
                RightTextPosition -= new Vector2(0, 0.062f);
                CentreTextPosition -= new Vector2(0, 0.062f);
            }

            //If our player isn't in the top 10, add them to the bottom.
            if (rank > 10)
            {
                GameObject Score = Instantiate(BaseGUIText, RightTextPosition, Quaternion.identity) as GameObject;
                Score.tag = "Score";
                Score.GetComponent<GUIText>().text = ""+highscore;
                Score.GetComponent<GUIText>().anchor = TextAnchor.MiddleCenter;
                GameObject Name = Instantiate(BaseGUIText, CentreTextPosition, Quaternion.identity) as GameObject;
                Name.tag = "Score";
                Name.GetComponent<GUIText>().text = username;
                GameObject Rank = Instantiate(BaseGUIText, LeftTextPosition, Quaternion.identity) as GameObject;
                Rank.tag = "Score";
                Rank.GetComponent<GUIText>().text = "" + (rank);
                Rank.GetComponent<GUIText>().anchor = TextAnchor.MiddleCenter;

                Score.GetComponent<GUIText>().material.color = Color.yellow;
                Name.GetComponent<GUIText>().material.color = Color.yellow;
                Rank.GetComponent<GUIText>().material.color = Color.yellow;
            }

            //Allows the user to restart the game
            pressspace = true;
        }
    }

}