﻿using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public GameObject gameBoard;
    public GameObject[] ballPrefabs;
    public GameObject linePrefab;
    Line activeLine;
    public bool[,] grid;
    public int boardWidth, boardHeight;

    private static int lowestNewBallValue = 1;
    private static int highestNewBallValue = 2;
    private int maxValue = 6;
    private int points;
    public List<GameObject> balls;
    public List<GameObject> lines;
    public SwipeManager swipeControls;
    public float spawnDelay = 0.5f;
    private bool generated;
    private Vector2 touchStartPosition = Vector2.zero;
    private RaycastHit2D hit;

    private enum State
    {
        Loaded,
        Playing,
        GameOver
    }

    private State state;

    #region monodevelop
    private void Awake()
    {
        state = State.Loaded;
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
                StartCoroutine(GenerateRandomBall());
                StartCoroutine(GenerateRandomBall());
                StartCoroutine(GenerateRandomBall());
                //GenerateRandomBall();
                break;
            case State.Playing:
                if (!UpgradeableBallsLeft() && generated == false) { StartCoroutine(GenerateRandomBall()); }
                FindBallFreePatch();
#if UNITY_EDITOR
                if (Input.GetMouseButtonDown(0))
                {
                    hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                    touchStartPosition = new Vector2(Mathf.Round(swipeControls.startTouch.x) + 0.5f,
                                                    Mathf.Round(swipeControls.startTouch.y) + 0.5f);
                }
#endif
#if UNITY_IOS || UNITY_ANDROID
                if (Input.touches.Length > 0)
                {
                    if (Input.touches[0].phase == TouchPhase.Began)
                    {
                        hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.touches[0].position), Vector2.zero);
                        touchStartPosition = new Vector2(Mathf.Round(swipeControls.startTouch.x) + 0.5f,
                                                    Mathf.Round(swipeControls.startTouch.y) + 0.5f);
                    }
                }
#endif
                if (swipeControls.SwipingLeft || swipeControls.SwipingRight)
                {
                    DottedLine.Instance.DestoryDottedLine();
                    if (hit.collider != null && hit.collider.tag == "Line") return;
                    if (IsInBounds(touchStartPosition))
                    LeftRightGate(touchStartPosition, false);
                }
                if (swipeControls.SwipingUp || swipeControls.SwipingDown)
                {
                    DottedLine.Instance.DestoryDottedLine();
                    if (hit.collider != null && hit.collider.tag == "Line") return;
                    if (IsInBounds(touchStartPosition))
                    UpDownGate(touchStartPosition, false);
                }
                if (swipeControls.SwippedLeft || swipeControls.SwippedRight)
                {
                    if (hit.collider != null && hit.collider.tag == "Line") return;
                    if (IsInBounds(touchStartPosition))
                    LeftRightGate(touchStartPosition, true);
                }
                if (swipeControls.SwipedUp || swipeControls.SwipedDown)
                {
                    if (hit.collider != null && hit.collider.tag == "Line") return;
                    if (IsInBounds(touchStartPosition))
                        UpDownGate(touchStartPosition, true);
                }
                if (swipeControls.Tap)
                {
                    if (hit.collider != null && hit.collider.tag == "Line")
                        DestoryLine(hit.collider.gameObject);
                }
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
        if (sameValueBalls.Count() > 0) { return true; }
        else return false;
        //return (balls.Count != balls.Distinct().Count());
    }

    public IEnumerator GenerateRandomBall()
    {
        generated = true;
        int value;
        // find out if we are generaing a ball with the lowest or highest value
        float highOrLowChance = Random.Range(0f, 0.99f);
        if (highOrLowChance >= 0.9f)
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
            //obj = SimplePool.Spawn(ballPrefabs[0], p, transform.rotation);
        }
        else
        {
            obj = Instantiate(ballPrefabs[1], p, transform.rotation);
            //obj = SimplePool.Spawn(ballPrefabs[1], p, transform.rotation);
        }
        obj.transform.SetParent(this.transform);
        obj.layer = 2; // Layer = ignore raycast
        balls.Add(obj);
        //AnimationHandler ballAnimManager = obj.GetComponent<AnimationHandler>();
        //ballAnimManager.AnimateEntry();
        yield return new WaitForSeconds(spawnDelay);
        generated = false;
    }

    private bool CanUpgrade(Ball thisBall, Ball thatBall)
    {
        return (thisBall.value != maxValue && thisBall.power == thatBall.power);
    }

    private void UpgradeBall(GameObject toDestory, GameObject toUpgrade)
    {
        Vector3 toUpgradePosition = (toDestory.transform.position + toUpgrade.transform.position) / 2;                                                           
            
        balls.Remove(toDestory);
        balls.Remove(toUpgrade);
        Destroy(toDestory);
        Destroy(toUpgrade);

        if (toUpgrade.GetComponent<Ball>().value < 5)
        {
            GameObject newBall = Instantiate(ballPrefabs[toUpgrade.GetComponent<Ball>().value], toUpgradePosition, transform.rotation);
            newBall.transform.SetParent(this.transform);
            newBall.layer = 2;
            balls.Add(newBall);
        }
        else if (toUpgrade.GetComponent<Ball>().value == 5)
        {
            // TODO: animation?
        }
        //else if // other 2 balls

        points += toUpgrade.GetComponent<Ball>().value * 2;
        //AnimationHandler ballAnim = newBall.GetComponent<AnimationHandler>();
        //ballAnim.AnimateUpgrade();
    }
#endregion

#region private methods (Lines)
    private void UpDownGate(Vector2 pos, bool Released)
    {
        RaycastHit2D hit1 = Physics2D.Raycast(pos, Vector2.up, Mathf.Infinity);
        RaycastHit2D hit2 = Physics2D.Raycast(pos, Vector2.down, Mathf.Infinity);
        if (hit1.collider && hit2.collider != null && !Released)
        {
            DottedLine.Instance.DrawDottedLine(hit1.point, hit2.point);
        }
        else if (hit1.collider && hit2.collider != null && Released)
        {
            DottedLine.Instance.DestoryDottedLine();
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
        }
    }

    private void LeftRightGate(Vector2 pos, bool Released)
    {
        RaycastHit2D hit1 = Physics2D.Raycast(pos, Vector2.left, Mathf.Infinity);
        RaycastHit2D hit2 = Physics2D.Raycast(pos, Vector2.right, Mathf.Infinity);
        if (hit1.collider && hit2.collider != null && !Released)
        {
            DottedLine.Instance.DrawDottedLine(hit1.point, hit2.point);
        }
        else if (hit1.collider && hit2.collider != null && Released)
        {
            DottedLine.Instance.DestoryDottedLine();
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
                SetLineAt(new Vector2(i, YCod), true);
            }
        }
    }

    private void DestoryLine(GameObject line)
    {
        Vector2 lineP = line.transform.position;
        int minX = Mathf.RoundToInt(line.GetComponent<BoxCollider2D>().bounds.min.x);
        int maxX = Mathf.RoundToInt(line.GetComponent<BoxCollider2D>().bounds.max.x);
        int minY = Mathf.RoundToInt(line.GetComponent<BoxCollider2D>().bounds.min.y);
        int maxY = Mathf.RoundToInt(line.GetComponent<BoxCollider2D>().bounds.max.y);
        //Debug.Log("minX " + minX + " maxX " + maxX + " minY " + minY + " maxY" + maxY);

        if (maxX - minX == 1)
        {
            for (int i = minY; i < maxY; i++)
            {
                //Debug.Log(new Vector2(minX, i));
                SetLineAt(new Vector2(minX, i), false);
            }
        }
        else if (maxY - minY == 1)
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

    private void FindBallFreePatch()
    {
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
                            UpgradeBall(ball1, ball2);
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
#endregion
}