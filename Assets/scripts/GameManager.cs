using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public GameObject gameBoard;
    public GameObject[] ballPrefabs;
    public GameObject linePrefab;
    public GameObject pressEffect;
    public GameObject floatingTextPrefab;

    Line activeLine;
    public bool[,] grid;
    public int boardWidth, boardHeight;

    private static int lowestNewBallValue = 1;
    private static int highestNewBallValue = 3;
    private static int loveBallValue = 6;
    private static int sadBallValue = 7;
    private int maxValue = 6;
    private int points;
    public List<GameObject> balls;
    public List<GameObject> lines;
    public SwipeManager swipeControls;
    public TimeManager timeControls;
    public BGColorManager bgColorControls;
    private RaycastHit2D hit;
    private GameObject _press;

    private Vector2 startPos;
    private Vector2 endPos;

    private enum State
    {
        Loaded,
        Playing,
        GameOver
    }

    private enum DottedLineStatus
    {
        upDown,
        leftRight,
        none
    }

    private State state;
    private DottedLineStatus dottedLineStatus;

    #region monodevelop
    private void Awake()
    {
        // set 60 fps for mobile devices
        Application.targetFrameRate = 60;

        // init games
        state = State.Loaded;
        dottedLineStatus = DottedLineStatus.none;
        balls = new List<GameObject>();

        boardWidth = (int)gameBoard.transform.localScale.x;
        boardHeight = (int)gameBoard.transform.localScale.y;
        grid = new bool[boardWidth, boardHeight];
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case State.GameOver:
                break;
            case State.Loaded:
                state = State.Playing;
                GenerateRandomBall();
                GenerateRandomBall();
                GenerateRandomBall();
                break;
            case State.Playing:
                if (!UpgradeableBallsLeft()) { GenerateRandomBall();}

                if (swipeControls.SwipingLeft || swipeControls.SwipingRight)
                {
                    //DottedLine.Instance.DestroyDottedLine();
                    if (IsInBounds(swipeControls.perfectPos) && !GetLineAt(swipeControls.perfectPos))
                    {
                        HorizontalGate(swipeControls.perfectPos, false);
                    }
                }
                if (swipeControls.SwipingUp || swipeControls.SwipingDown)
                {
                    //DottedLine.Instance.DestroyDottedLine();
                    if (IsInBounds(swipeControls.perfectPos) && !GetLineAt(swipeControls.perfectPos))
                    {
                        VerticalGate(swipeControls.perfectPos, false);
                    }
                }
                if (swipeControls.SwippedLeft || swipeControls.SwippedRight)
                {
                    if (IsInBounds(swipeControls.perfectPos) && !GetLineAt(swipeControls.perfectPos))
                    {
                        HorizontalGate(swipeControls.perfectPos, true);
                        StartCoroutine(FindBallFreePatch());
                    }
                }
                if (swipeControls.SwipedUp || swipeControls.SwipedDown)
                {
                    if (IsInBounds(swipeControls.perfectPos) && !GetLineAt(swipeControls.perfectPos))
                    {
                        VerticalGate(swipeControls.perfectPos, true);
                        StartCoroutine(FindBallFreePatch());
                    }
                }
#if UNITY_EDITOR
                if (Input.GetMouseButtonDown(0))
                {
                    Vector2 p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    startPos = new Vector2(Mathf.Round(p.x) + 0.5f,
                                           Mathf.Round(p.y) + 0.5f);
                    if (pressEffect)
                        _press = Instantiate(pressEffect, startPos, Quaternion.identity);
                }
                //if (Input.GetMouseButton(0))
                //{
                //    endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                //    endTime = Time.time - startTime;
                //    if (endPos == startPos && endTime >= 0.5f)
                //    {
                //        // indicator
                //        Debug.Log("Worked");
                //    }
                //}

                if (Input.GetMouseButtonUp(0))
                {
                    Vector2 p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    endPos = new Vector2(Mathf.Round(p.x) + 0.5f,
                                           Mathf.Round(p.y) + 0.5f);
                    if (_press)
                        Destroy(_press);
                    if (startPos == endPos)
                    {
                        hit = Physics2D.CircleCast(Camera.main.ScreenToWorldPoint(Input.mousePosition), 1.5f, Vector2.zero);
                    }
                }
#endif
#if UNITY_IOS || UNITY_ANDROID
                if (Input.touches.Length > 0)
                {
                    if (Input.touches[0].phase == TouchPhase.Began)
                    {
                        Vector2 p = Camera.main.ScreenToWorldPoint(Input.touches[0].position);
                        startPos = new Vector2(Mathf.Round(p.x) + 0.5f,
                                                Mathf.Round(p.y) + 0.5f);
                        if (pressEffect)
                        {
                            _press = Instantiate(pressEffect, startPos, Quaternion.identity);
                        }

                    }
                    //if (Input.touches[0].phase == TouchPhase.Stationary)
                    //{
                    //    endPos = Camera.main.ScreenToWorldPoint(Input.touches[0].position);
                    //    endTime = Time.time - startTime;
                    //    if (endPos == startPos && endTime >= 0.5f && !runonce)
                    //    {
                    //        // indicator
                    //        runonce = true;
                    //    }
                    //}
                    if (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled)
                    {
                        Vector2 p = Camera.main.ScreenToWorldPoint(Input.touches[0].position);
                        endPos = new Vector2(Mathf.Round(p.x) + 0.5f,
                                             Mathf.Round(p.y) + 0.5f);
                        if (_press)
                            Destroy(_press);
                        if (startPos == endPos)
                        {
                            hit = Physics2D.CircleCast(Camera.main.ScreenToWorldPoint(Input.touches[0].position), 1.5f, Vector2.zero);
                        }
                    }
                }
                if (hit.collider != null && hit.collider.tag == "Line")
                {
                    DestroyLine(hit.collider.gameObject);
                    DestroyTrashLine();
                    //StartCoroutine(DestroyTrashLine());
                    StartCoroutine(FindBallFreePatch());
                }
#endif
                break;
        }
    }
#endregion

#region private methods (Balls)
    private bool UpgradeableBallsLeft()
    {
        var sameValueBalls = from b in balls
                             group balls by b.GetComponent<Ball>().value into v
                             where v.Count() > 1
                             select v.Key;
        if (sameValueBalls.Any()) { return true; }
        else return false;
        //return (balls.Count != balls.Distinct().Count());
    }

    public void GenerateRandomBall()
    {
        int value;
        // find out if we are generaing a ball with the lowest or highest value
        float highOrLowChance = Random.Range(0f, 0.99f);
        if (highOrLowChance >= 0.85f)
        {
            float halfhalf = Random.Range(0f, 0.99f);
            if (halfhalf >= 0.5f)
            {
                value = loveBallValue;
            }
            else
            {
                value = sadBallValue;
            }
        }
        else if (highOrLowChance >= 0.6f && highOrLowChance < 0.85f)
        {
            value = highestNewBallValue;
        }
        else
        {
            value = lowestNewBallValue;
        }
        // attempt to get the starting position
        Vector2 p = new Vector2(Random.Range(1, boardWidth - 1), Random.Range(1, boardHeight - 1));
        GameObject obj;

        if (value == lowestNewBallValue)
        {
            obj = Instantiate(ballPrefabs[0], p, transform.rotation);
        }
        else if (value == highestNewBallValue)
        {
            obj = Instantiate(ballPrefabs[1], p, transform.rotation);
        }
        else if (value == loveBallValue) // love ball
        {
            obj = Instantiate(ballPrefabs[5], p, transform.rotation);
        }
        else // sad ball
        {
            obj = Instantiate(ballPrefabs[6], p, transform.rotation);

            Vector2 sadP = new Vector2(Random.Range(1, boardWidth - 1), Random.Range(1, boardHeight - 1));
            GameObject _obj = Instantiate(ballPrefabs[6], sadP, transform.rotation);
            _obj.transform.SetParent(this.transform);
            _obj.layer = 2;
            balls.Add(_obj); // Make sure we spawn 2 sad balls
        }

        obj.transform.SetParent(this.transform);
        obj.layer = 2; // Layer = ignore raycast
        balls.Add(obj);

        //AnimationHandler ballAnimManager = obj.GetComponent<AnimationHandler>();
        //ballAnimManager.AnimateEntry();
    }

    private bool CanUpgrade(Ball thisBall, Ball thatBall)
    {
        return (thisBall.value != maxValue && thisBall.power == thatBall.power
                || (thisBall.value == sadBallValue || thatBall.value == sadBallValue)
                || (thisBall.value == loveBallValue || thatBall.value == loveBallValue));
    }

    private IEnumerator UpgradeBall(GameObject toDestroy, GameObject toUpgrade)
    {
        Vector3 toUpgradePosition = (toDestroy.transform.position + toUpgrade.transform.position) / 2;                                                           
        timeControls.DoSlowmotion();
        toDestroy.GetComponent<Ball>().upgradeBall = toUpgrade;
        toUpgrade.GetComponent<Ball>().upgradeBall = toDestroy;
        toDestroy.GetComponent<Ball>().readyToUpgrade = true;
        toUpgrade.GetComponent<Ball>().readyToUpgrade = true;

        yield return new WaitUntil(() => toDestroy.GetComponent<Ball>().touched == true);

        balls.Remove(toDestroy);
        balls.Remove(toUpgrade);
        Destroy(toDestroy);
        Destroy(toUpgrade);

        if (toUpgrade.GetComponent<Ball>().value == 5 && toDestroy.GetComponent<Ball>().value == 5) // Final ball
        {
            // TODO: animation?

            // Floating Text
            if (floatingTextPrefab)
                ShowFloatingText(toUpgradePosition, "two 5 balls");
            bgColorControls.ChangeColor(toDestroy.GetComponent<Ball>().value);
        }
        else if (toUpgrade.GetComponent<Ball>().value == 6 && toDestroy.GetComponent<Ball>().value != 6 && toDestroy.GetComponent<Ball>().value != 7) // Love ball
        {
            if (floatingTextPrefab)
                ShowFloatingText(toUpgradePosition, "1 love ball");
            bgColorControls.ChangeColor(toDestroy.GetComponent<Ball>().value);

            GameObject newBall = Instantiate(ballPrefabs[toDestroy.GetComponent<Ball>().value], toUpgradePosition, transform.rotation);
            newBall.transform.SetParent(this.transform);
            newBall.layer = 2;
            balls.Add(newBall);
        }
        else if (toDestroy.GetComponent<Ball>().value == 6 && toUpgrade.GetComponent<Ball>().value != 6 && toUpgrade.GetComponent<Ball>().value != 7) // Love ball
        {
            if (floatingTextPrefab)
                ShowFloatingText(toUpgradePosition, "1 love ball");
            bgColorControls.ChangeColor(toUpgrade.GetComponent<Ball>().value);

            GameObject newBall = Instantiate(ballPrefabs[toUpgrade.GetComponent<Ball>().value], toUpgradePosition, transform.rotation);
            newBall.transform.SetParent(this.transform);
            newBall.layer = 2;
            balls.Add(newBall);
        }
        else if (toDestroy.GetComponent<Ball>().value == 6 && toUpgrade.GetComponent<Ball>().value == 6) //  2 Love balls? How lucky you are!
        {
            if (floatingTextPrefab)
                ShowFloatingText(toUpgradePosition, "2 love balls");
            bgColorControls.ChangeColor(toDestroy.GetComponent<Ball>().value);

            // easter egg? Destroy all balls?
        }
        else if (toUpgrade.GetComponent<Ball>().value == 7 || toDestroy.GetComponent<Ball>().value == 7) // Sad ball
        {
            // Just destroy the other ball
            if (toUpgrade.GetComponent<Ball>().value == 1 || toDestroy.GetComponent<Ball>().value == 1)
            {
                // But if the other ball is value 1 ball
                // Lose game 
                if (floatingTextPrefab)
                    ShowFloatingText(toUpgradePosition, "GameOver");
                bgColorControls.ChangeColor(7); // black

                state = State.GameOver;
            }
            else
            {
                if (floatingTextPrefab)
                    ShowFloatingText(toUpgradePosition, "Destroy");
                bgColorControls.ChangeColor(0); // white
            }
        }
        else // normal balls
        {
            if (floatingTextPrefab)
                ShowFloatingText(toUpgradePosition, "Regular");
            bgColorControls.ChangeColor(toDestroy.GetComponent<Ball>().value);

            GameObject newBall = Instantiate(ballPrefabs[toUpgrade.GetComponent<Ball>().value], toUpgradePosition, transform.rotation);
            newBall.transform.SetParent(this.transform);
            newBall.layer = 2;
            balls.Add(newBall);
            //AnimationHandler ballAnim = newBall.GetComponent<AnimationHandler>();
            //ballAnim.AnimateUpgrade();
        }
        //else if // other 2 balls
        points += toUpgrade.GetComponent<Ball>().value * 2;

    }
#endregion

#region private methods (Lines)
    private void VerticalGate(Vector2 pos, bool Released)
    {
        RaycastHit2D hit1 = Physics2D.Raycast(pos, Vector2.up, Mathf.Infinity);
        RaycastHit2D hit2 = Physics2D.Raycast(pos, Vector2.down, Mathf.Infinity);
        if (hit1.collider && hit2.collider != null && !Released && 
            (dottedLineStatus == DottedLineStatus.leftRight || dottedLineStatus == DottedLineStatus.none))
        {
            DottedLine.Instance.DestroyDottedLine();
            DottedLine.Instance.DrawDottedLine(hit1.point, hit2.point);
            dottedLineStatus = DottedLineStatus.upDown;
        }
        else if (hit1.collider && hit2.collider != null && Released)
        {
            DottedLine.Instance.DestroyDottedLine();
            GameObject lineGO = Instantiate(linePrefab);
            lines.Add(lineGO);
            activeLine = lineGO.GetComponent<Line>();
            if (activeLine != null)
            {
                activeLine.DrawLine(hit1.point, hit2.point);
            }
            int XCod = (int)hit1.point.x;
            int minYCod = Mathf.RoundToInt(hit2.point.y);
            int maxYCod = Mathf.RoundToInt(hit1.point.y) - 1;
            //Debug.Log(XCod);
            //Debug.Log(minYCod + ", " + maxYCod);
            for (int i = minYCod ; i <= maxYCod; i++)
            {
                //Debug.Log(new Vector2(XCod, i));
                SetLineAt(new Vector2(XCod, i), true);
            }
            dottedLineStatus = DottedLineStatus.none;
        }
    }

    private void HorizontalGate(Vector2 pos, bool Released)
    {
        RaycastHit2D hit1 = Physics2D.Raycast(pos, Vector2.left, Mathf.Infinity);
        RaycastHit2D hit2 = Physics2D.Raycast(pos, Vector2.right, Mathf.Infinity);
        if (hit1.collider && hit2.collider != null && !Released && 
            (dottedLineStatus == DottedLineStatus.upDown || dottedLineStatus == DottedLineStatus.none))
        {
            DottedLine.Instance.DestroyDottedLine();
            DottedLine.Instance.DrawDottedLine(hit1.point, hit2.point);
            dottedLineStatus = DottedLineStatus.leftRight;
        }
        else if (hit1.collider && hit2.collider != null && Released)
        {
            DottedLine.Instance.DestroyDottedLine();
            GameObject lineGO = Instantiate(linePrefab);
            lines.Add(lineGO);
            activeLine = lineGO.GetComponent<Line>();
            if (activeLine != null)
            {
                activeLine.DrawLine(hit1.point, hit2.point);
            }
            int YCod = (int)hit1.point.y;
            int minXCod = Mathf.RoundToInt(hit1.point.x);
            int maxXCod = Mathf.RoundToInt(hit2.point.x) - 1 ;
            //Debug.Log(YCod);
            //Debug.Log(minXCod + ", " + maxXCod);
            for (int i = minXCod; i <= maxXCod; i++)
            {
                //Debug.Log(new Vector2(i, YCod));
                SetLineAt(new Vector2(i, YCod), true);
            }
            dottedLineStatus = DottedLineStatus.none;
        }
    }

    private void DestroyLine(GameObject line)
    {
        int minX = Mathf.RoundToInt(line.GetComponent<BoxCollider2D>().bounds.min.x);
        int maxX = Mathf.RoundToInt(line.GetComponent<BoxCollider2D>().bounds.max.x);
        int minY = Mathf.RoundToInt(line.GetComponent<BoxCollider2D>().bounds.min.y);
        int maxY = Mathf.RoundToInt(line.GetComponent<BoxCollider2D>().bounds.max.y);
        //Debug.Log("minX " + minX + " maxX " + maxX + " minY " + minY + " maxY" + maxY);
        //Debug.Log(line.GetComponent<BoxCollider2D>().size);

        if (maxX - minX == 1) // vertical line
        {
            for (int i = minY; i < maxY; i++)
            {
                //Debug.Log(new Vector2(minX, i));
                SetLineAt(new Vector2(minX, i), false);
            }
        }
        if (maxY - minY == 1) // horizontal line
        {
            for (int i = minX; i < maxX; i++)
            {
                //Debug.Log(new Vector2(i, minY));
                SetLineAt(new Vector2(i, minY), false);
            }
        }
        lines.Remove(line);
        Destroy(line);
    }

    private void DestroyTrashLine()
    {
        GameObject[] allLines = GameObject.FindGameObjectsWithTag("Line");
        foreach (GameObject line in allLines)
        {
            if (line != null && !line.GetComponent<Line>()._isGoodLine)
            {
                int minX = Mathf.RoundToInt(line.GetComponent<BoxCollider2D>().bounds.min.x);
                int maxX = Mathf.RoundToInt(line.GetComponent<BoxCollider2D>().bounds.max.x);
                int minY = Mathf.RoundToInt(line.GetComponent<BoxCollider2D>().bounds.min.y);
                int maxY = Mathf.RoundToInt(line.GetComponent<BoxCollider2D>().bounds.max.y);
                if (maxX - minX == 1) // vertical line
                {
                    RaycastHit2D hit1 = Physics2D.Raycast(new Vector2((minX + maxX) / 2, minY - 0.8f), Vector2.down, 0.1f);
                    RaycastHit2D hit2 = Physics2D.Raycast(new Vector2((minX + maxX) / 2, maxY + 0.8f), Vector2.up, 0.1f);
                    if (hit1.collider == null || hit2.collider == null)
                    {
                        DestroyLine(line);
                    }
                }
                if (maxY - minY == 1)
                {
                    RaycastHit2D hit1 = Physics2D.Raycast(new Vector2(minX - 0.8f, (minY + maxY) / 2), Vector2.left, 0.1f);
                    RaycastHit2D hit2 = Physics2D.Raycast(new Vector2(maxX + 0.8f, (minY + maxY) / 2), Vector2.right, 0.1f);
                    if (hit1.collider == null || hit2.collider == null)
                    {
                        DestroyLine(line);
                    }
                }
            }
        }
    }

#endregion

#region private methods (Board)
    private bool IsInBounds(Vector3 v)
    {
        return v.x >= 0 && v.x < boardWidth && v.y >= 0 && v.y < boardHeight;
    }

    private bool GetLineAt(Vector3 v)
    {
        return grid[(int)v.x, (int)v.y];
    }

    private void SetLineAt(Vector3 v, bool b)
    {
        grid[(int)v.x, (int)v.y] = b;
    }

    private IEnumerator FindBallFreePatch()
    {
        yield return new WaitForSeconds(0.1f); // add some check delay
        bool[,] visited = new bool[boardWidth, boardHeight];
        int nRegions = 0;
        for (int i = 0; i < boardWidth; i++)
        {
            for (int j = 0; j < boardHeight; j++)
            {
                if (!grid[i, j] && !visited[i, j])
                {
                    Dictionary<Vector2, bool> region = new Dictionary<Vector2, bool>();
                    FloodFill(i, j, visited, region);
                    nRegions += 1;
                    bool ballDetected = false;
                    int ballNum = 0;
                    GameObject ball1 = null, ball2 = null;
                    foreach (GameObject b in balls)
                    {
                        Vector3 p = b.transform.position;
                        Vector2 v = new Vector2(Mathf.Round(p.x),
                                                Mathf.Round(p.y));
                        if (region.ContainsKey(v))
                        {
                            ballDetected = true;
                            ballNum++;
                            if (ball1 == null) { ball1 = b; }
                            else if (ball2 == null) { ball2 = b; }
                        }
                    }
                    if (ballNum == 2 && ball1 != null && ball2 != null)
                    {
                        Ball thisball = ball1.GetComponent<Ball>();
                        Ball thatball = ball2.GetComponent<Ball>();
                        if (CanUpgrade(thisball, thatball))
                        {
                            StartCoroutine(UpgradeBall(ball1, ball2));
                        }
                    }
                    if (!ballDetected)
                    {
                        // -1 life
                    }
                }
            }
        }
    }

    private void FloodFill(int i, int j, bool[,] visited, Dictionary<Vector2, bool> region)
    {
        visited[i, j] = true;
        region.Add(new Vector2(i, j), true);
        if (i > 0 && !visited[i - 1, j] && !grid[i - 1, j])
        {
            FloodFill(i - 1, j, visited, region);
        }
        if (i < (boardWidth - 1) && !visited[i + 1, j] && !grid[i + 1, j])
        {
            FloodFill(i + 1, j, visited, region);
        }
        if (j > 0 && !visited[i, j - 1] && !grid[i, j - 1])
        {
            FloodFill(i, j - 1, visited, region);
        }
        if (j < (boardHeight - 1) && !visited[i, j + 1] && !grid[i, j + 1])
        {
            FloodFill(i, j + 1, visited, region);
        }
    }

    private void ShowFloatingText(Vector2 pos, string desplayText)
    {
        var go = Instantiate(floatingTextPrefab, pos, Quaternion.identity);
        go.GetComponentInChildren<TextMesh>().text = desplayText;
    }
#endregion
}