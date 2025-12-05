using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using UnityEngine;

using Random = UnityEngine.Random;

#pragma warning disable IDE0051 // Remove unused private members

public class robotProgramming : MonoBehaviour
{
    public KMBombModule module;
    public KMBombInfo bomb;
    public KMAudio bombAudio;
    static int ModuleIdCounter = 1;
    int ModuleId;
    bool moduleSolved;
    public KMColorblindMode colorblind;
    bool colorblindActive;
    public TextMesh[] colorblindRobotTexts;
    public TextMesh colorblindLEDText;
    public GameObject colorblindOtherText;

    //Buttons
    public KMSelectable startButton;
    public KMSelectable resetButton;
    public KMSelectable[] arrowButtons;
    public KMSelectable[] blockButtons;

    //Maze
    private readonly string[] maze = new string[9]; //to make my life easier, the coordinates for reading off the maze are in reverse order (y, x)
    private static readonly string[] mazeTops = new string[16]
    {
        "XrXyXgXbX|X...X...X|XXX.X.X.X|X...X.X.X|X.XXXXX.X|X.......X",
        "XrXyXgXbX|X.......X|X.XX.XX.X|X.X...X.X|XXX.X.XXX|X...X...X",
        "XrXyXgXbX|X.......X|XXX.XXX.X|X.X...X.X|X.XXX.XXX|X.......X",
        "XrXyXgXbX|X.......X|X.X.XXX.X|X.X.....X|XXX.X.XXX|X...X...X",
        "XrXyXgXbX|X...X...X|X.XXX.XXX|X.X.....X|X.X.XXX.X|X.....X.X",
        "XrXyXgXbX|X.....X.X|X.XXX.X.X|X.X...X.X|XXX.XXX.X|X.......X",
        "XrXyXgXbX|X.X.X.X.X|X.X.X.X.X|X.......X|X.XXXXX.X|X.....X.X",
        "XrXyXgXbX|X.X...X.X|X.X.XXX.X|X.......X|X.X.X.X.X|X.X.X.X.X",
        "XrXyXgXbX|X.X...X.X|X.X.X.X.X|X...X...X|X.XXXXX.X|X.......X",
        "XrXyXgXbX|X.X...X.X|X.XXX.X.X|X.......X|XXX.XXXXX|X.......X",
        "XrXyXgXbX|X...X...X|X.X.X.X.X|X.X...X.X|X.X.XXX.X|X...X...X",
        "XrXyXgXbX|X.X...X.X|X.X.X.X.X|X...X...X|X.XXXXX.X|X...X...X",
        "XrXyXgXbX|X...X.X.X|X.XXX.X.X|X.......X|XXX.X.XXX|X...X...X",
        "XrXyXgXbX|X.......X|XX.XXX.XX|X...X...X|X.X.X.X.X|X.X...X.X",
        "XrXyXgXbX|X.X.....X|X.X.XXX.X|X...X...X|X.XXXXX.X|X.X.....X",
        "XrXyXgXbX|X.....X.X|X.XXX.X.X|X.X...X.X|X.X.XXX.X|X.X.....X"
    };
    private static readonly string[] mazeBottoms = new string[16]
    {
        "X.X.XXX.X|X.X.X...X|XXXXXXXXX",
        "X.XXXXX.X|X.......X|XXXXXXXXX",
        "X.X.XXX.X|X.X.....X|XXXXXXXXX",
        "X.X.XXXXX|X.......X|XXXXXXXXX",
        "X.X.XXXXX|X.......X|XXXXXXXXX",
        "X.X.X.XXX|X.......X|XXXXXXXXX",
        "X.X.X.XXX|X.......X|XXXXXXXXX",
        "X.X.X.X.X|X...X...X|XXXXXXXXX",
        "X.X.XXX.X|X.X.....X|XXXXXXXXX",
        "XXX.XXX.X|X.......X|XXXXXXXXX",
        "X.XXX.XXX|X.X.....X|XXXXXXXXX",
        "X.X.X.X.X|X...X...X|XXXXXXXXX",
        "X.XXX.X.X|X.......X|XXXXXXXXX",
        "X.XXX.XXX|X.......X|XXXXXXXXX",
        "X.X.XXX.X|X.X...x.X|XXXXXXXXX",
        "X.X.X.X.X|X...X...X|XXXXXXXXX"
    };
    public Sprite[] topHalfSprites;
    public Sprite[] bottomHalfSprites;
    public SpriteRenderer topHalfRenderer;
    public SpriteRenderer bottomHalfRenderer;
    int topIndex;
    int bottomIndex;
    private static readonly Vector2Int[] goalPositions = new Vector2Int[4] { new Vector2Int(7, 0), new Vector2Int(5, 0), new Vector2Int(1, 0), new Vector2Int(3, 0) }; //sorted by color (blue, green, red, yellow)

    //Robot Characteristics
    public enum RobotColor
    {
        Blue, Green, Red, Yellow
    }
    public enum Shape
    {
        Triangle, Square, Hexagon, Circle
    }
    public enum Type
    {
        ROB, HAL, R2D2, Fender
    }
    private readonly RobotColor[] robotColors = new RobotColor[4] { RobotColor.Blue, RobotColor.Green, RobotColor.Red, RobotColor.Yellow };
    private readonly Shape[] robotShapes = new Shape[4] { Shape.Triangle, Shape.Square, Shape.Hexagon, Shape.Circle };
    private readonly Robot[] robots = new Robot[4];
    private readonly Robot[] sortedRobots = new Robot[4];
    private Type[] robotTypes;

    //Robot Visuals
    public GameObject[] robotObjects;
    readonly GameObject[] sortedRobotObjects = new GameObject[4]; //makes moving the visuals WAY easier if sorted by color
    public Material[] robotMaterials;
    public Mesh[] robotMeshes;

    //Moving/Handling Input
    public enum Direction
    {
        Up, Right, Down, Left
    }
    readonly List<string> inputNames = new List<string>();
    //Striking
    bool willStrike;
    bool lastR2D2Behavior;
    int lastSerialCharacterIndex;
    //R2D2
    bool R2D2actsLikeHAL;
    bool initialR2D2Behavior; //Reset
    //Fender
    int serialCharacterIndex;
    int initialCharacterIndex; //Reset
    readonly string[] placeNames = new string[6] { "1st", "2nd", "3rd", "4th", "5th", "6th" }; //used for logging
    //Animation
    readonly Queue<AnimationRequest> animationQueue = new Queue<AnimationRequest>();
    bool currentlyAnimating;

    //Showing inputs
    //LED
    List<RobotColor> notBlockedColors = new List<RobotColor> { RobotColor.Blue, RobotColor.Green, RobotColor.Red, RobotColor.Yellow };
    int currentColorIndex;
    int initialColorIndex;
    public Renderer LedRenderer;
    public Material[] unlitColorMaterials;
    //Small Display
    public TextMesh displayText;
    public Sprite[] shapeSprites;
    private static readonly Color[] displayColors = new Color[4] { new Color(58 / 255f, 88 / 255f, 1), new Color(0, 1, 0), new Color(1, 0, 0), new Color(1, 1, 0) };
    public SpriteRenderer[] displayShapeRenderers;
    public GameObject displayShapesObject;

    void Awake()
    {
        ModuleId = ModuleIdCounter++;

        for (int i = 0; i < 4; i++)
        {
            int dummy = i; //i need this because "i" is changing constantly and i think that makes it not work
            arrowButtons[dummy].OnInteract += delegate () { arrowButtonPressed((Direction) dummy); return false; };
            blockButtons[dummy].OnInteract += delegate () { blockButtonPressed((RobotColor) dummy); return false; };
        }
        startButton.OnInteract += delegate () { handleStart(); return false; };
        resetButton.OnInteract += delegate () { willStrike = false; handleReset(); return false; };

        colorblindActive = colorblind.ColorblindModeActive;
    }

    void Start()
    {
        topIndex = Random.Range(0, 16);
        bottomIndex = Random.Range(0, 16);
        string[] topHalf = mazeTops[topIndex].Split('|');
        string[] bottomHalf = mazeBottoms[bottomIndex].Split('|');
        string logMaze = "";

        topHalfRenderer.sprite = topHalfSprites[topIndex];
        bottomHalfRenderer.sprite = bottomHalfSprites[bottomIndex];
        displayText.text = "[AWAITING INPUT]\n" + (topIndex + 1) + " " + (bottomIndex + 1);

        for (int i = 0; i < 6; i++) //assign top half of the maze
        {
            maze[i] = topHalf[i];
            logMaze += topHalf[i] + "\n";
        }
        for (int i = 6; i < 9; i++) //assign bottom half
        {
            maze[i] = bottomHalf[i - 6];
            logMaze += bottomHalf[i - 6] + (i == 8 ? "" : "\n");
        }
        LogMsg("The maze numbers are " + (topIndex + 1) + " and " + (bottomIndex + 1) + ".");
        LogMsg("The resulting maze:\n" + logMaze);

        robotColors.Shuffle();
        robotShapes.Shuffle();

        int caseNumber = 0; //this number increments by 4 if the first rule in the chart applies, by 2 if the second rule applies, and by 1 if the 3rd rule applies

        for (int i = 0; i < 4; i++)
        {
            robotObjects[i].GetComponent<Renderer>().material = robotMaterials[(int) robotColors[i]];
            robotObjects[i].GetComponent<MeshFilter>().mesh = robotMeshes[(int) robotShapes[i]];

            displayShapeRenderers[i].color = displayColors[(int) robotColors[i]];
            displayShapeRenderers[i].sprite = shapeSprites[(int) robotShapes[i]];

            if (robotColors[i] == RobotColor.Yellow && robotShapes[i] == Shape.Hexagon) //second rule
                caseNumber += 2;
        }

        if (robotColors[0] == RobotColor.Red || robotColors[0] == RobotColor.Green) //first rule
            caseNumber += 4;
        if (robotShapes[2] == Shape.Triangle) //third rule
            caseNumber += 1;
        switch (caseNumber)
        {
            default:
            case 0:
                robotTypes = new Type[4] { Type.ROB, Type.HAL, Type.R2D2, Type.Fender };
                break;
            case 1:
                robotTypes = new Type[4] { Type.HAL, Type.Fender, Type.ROB, Type.R2D2 };
                break;
            case 2:
                robotTypes = new Type[4] { Type.HAL, Type.ROB, Type.Fender, Type.R2D2 };
                break;
            case 3:
                robotTypes = new Type[4] { Type.R2D2, Type.Fender, Type.HAL, Type.ROB };
                break;
            case 4:
                robotTypes = new Type[4] { Type.Fender, Type.ROB, Type.HAL, Type.R2D2 };
                break;
            case 5:
                robotTypes = new Type[4] { Type.HAL, Type.R2D2, Type.Fender, Type.ROB };
                break;
            case 6:
                robotTypes = new Type[4] { Type.Fender, Type.HAL, Type.R2D2, Type.ROB };
                break;
            case 7:
                robotTypes = new Type[4] { Type.R2D2, Type.ROB, Type.Fender, Type.HAL };
                break;
        }

        //Sorting the robots based on color to make moving them easier
        int[] initialXPositions = new int[4] { 1, 3, 5, 7 };
        for (int i = 0; i < 4; i++)
        {
            robots[i] = new Robot(robotColors[i], robotShapes[i], robotTypes[i], initialXPositions[i], 7);
            sortedRobots[(int) robotColors[i]] = robots[i];
            sortedRobotObjects[(int) robotColors[i]] = robotObjects[i];
        }

        LogMsg("The robots in the initial order are: " + LogRobots(robots[0]) + ", " + LogRobots(robots[1]) + ", " + LogRobots(robots[2]) + ", " + LogRobots(robots[3]) + ".");
        LogMsg("The robot types are: " + robotTypes[0].ToString() + ", " + robotTypes[1].ToString() + ", " + robotTypes[2].ToString() + ", " + robotTypes[3].ToString() + ".");

        if (colorblindActive)
            setupColorblind();

        LogMsgSilent(maze.Join("\n"));
    }

    //button presses
    void arrowButtonPressed(Direction direction)
    {
        if (notBlockedColors.Count == 0 || currentlyAnimating || moduleSolved)
            return;

        inputNames.Add(direction.ToString());

        int index = (int) notBlockedColors[currentColorIndex];
        string colorName = notBlockedColors[currentColorIndex].ToString().ToLower();
        string directionName = direction.ToString().ToLower();
        Robot currentRobot = sortedRobots[index];
        Debug.LogFormat("[Robot Programming #{0}] The {1} robot, {2}, has received a{3} {4} press.", ModuleId, colorName, currentRobot.Type.ToString(), directionName[0].EqualsAny("aeiou") ? "n" : "", directionName);

        //modifying direction according to robot's type
        Direction outputDirection = new Direction();
        switch (currentRobot.Type)
        {
            case Type.ROB:
                outputDirection = direction;
                break;
            case Type.HAL:
                outputDirection = (Direction) (((int) direction + 2) % 4);
                break;
            case Type.R2D2:
                if (R2D2actsLikeHAL)
                {
                    outputDirection = (Direction) (((int) direction + 2) % 4);
                }
                else
                {
                    outputDirection = direction;
                }

                Debug.LogFormat("[Robot Programming #{0}] R2D2 will act like {1} for this turn.", ModuleId, R2D2actsLikeHAL ? "HAL" : "ROB");
                R2D2actsLikeHAL = !R2D2actsLikeHAL;
                break;
            case Type.Fender:
                string serialNumber = bomb.GetSerialNumber();

                string serialBorderedCharacter = "";
                for (int i = 0; i < 6; i++)
                {
                    if (i == serialCharacterIndex)
                    {
                        serialBorderedCharacter += "[" + serialNumber[i] + "]";
                        continue;
                    }
                    serialBorderedCharacter += serialNumber[i];
                }

                bool isADigit = char.IsDigit(serialNumber[serialCharacterIndex]);
                if (isADigit)
                {
                    outputDirection = direction;
                }
                else
                {
                    outputDirection = (Direction) (((int) direction + 2) % 4);
                }

                Debug.LogFormat("[Robot Programming #{0}] The {1} character in the serial number ({2}) is a {3}, so Fender will act like {4} for this turn.", ModuleId, placeNames[serialCharacterIndex], serialBorderedCharacter, isADigit ? "digit" : "letter", isADigit ? "ROB" : "HAL");
                serialCharacterIndex = (serialCharacterIndex + 1) % 6;
                break;
        }

        //moving the robot
        switch (outputDirection)
        {
            case Direction.Up:
                currentRobot.Position.y--;
                break;
            case Direction.Right:
                currentRobot.Position.x++;
                break;
            case Direction.Down:
                currentRobot.Position.y++;
                break;
            case Direction.Left:
                currentRobot.Position.x--;
                break;
        }
        LogMsg("The " + colorName + " robot will move " + outputDirection.ToString().ToLower() + ". Its new coordinates are " + (currentRobot.Position.x + 1) + ", " + (9 - currentRobot.Position.y) + ".");

        bool crashes = isRobotColliding(currentRobot, !willStrike); //!willStrike makes it only log the first crash
        if (!willStrike) //prevents extraneous moves from being animated/counted for the most recent behavior, because it'll strike before it gets to the move anyway
        {
            bool oob = currentRobot.Position.x < 0 || currentRobot.Position.x > 8 || currentRobot.Position.y < 0 || currentRobot.Position.y > 8;
            animationQueue.Enqueue(new AnimationRequest(index, outputDirection, crashes, oob));
            if (!crashes)
            {
                currentRobot.LastPosition = currentRobot.Position;
                lastR2D2Behavior = R2D2actsLikeHAL;
                lastSerialCharacterIndex = serialCharacterIndex;
            }
        }
        if (crashes)
        {
            willStrike = true;
        }

        //shift forward color by one and keep shifting if robot is stuck, completely stopping if all robots are stuck
        bool decidedRobot = false;
        for (int i = currentColorIndex + 1; i < currentColorIndex + 1 + notBlockedColors.Count; i++)
        {
            if (!isRobotStuck(sortedRobots[(int) notBlockedColors[i % notBlockedColors.Count]]))
            {
                decidedRobot = true;
                currentColorIndex = i % notBlockedColors.Count;
                break;
            }
        }
        if (!decidedRobot)
        {
            LogMsg("All robots are blocked or stuck, so no more moves can be made.");
            notBlockedColors.Clear(); //this is ok to clear because at that point no moves can be made anyway
        }

        updateLed();
        updateDisplay();
    }

    void blockButtonPressed(RobotColor color)
    {
        if (!notBlockedColors.Contains(color) || currentlyAnimating || moduleSolved)
            return;

        if (currentColorIndex > notBlockedColors.IndexOf(color))
        {
            //LogMsgSilent("Current color index greater than index of blocked color. Shifting down.");
            currentColorIndex--;
        }
        notBlockedColors.Remove(color);
        if (currentColorIndex > notBlockedColors.Count - 1)
        {
            //LogMsgSilent("Current color index greater than length of list. Setting to 0.");
            currentColorIndex = 0;
        }

        inputNames.Add("Block " + color.ToString());
        LogMsg(sortedRobots[(int) color].Type.ToString() + " (" + color.ToString() + ") has been blocked.");

        updateLed();
        updateDisplay();
    }

    //handle visuals
    void updateLed()
    {
        if (notBlockedColors.Count <= 0 || currentColorIndex == 4)
        {
            LedRenderer.material = unlitColorMaterials[4];

            if (colorblindActive)
                colorblindLEDText.text = "";

            return;
        }

        LedRenderer.material = unlitColorMaterials[(int) notBlockedColors[currentColorIndex]];

        if (colorblindActive)
            setColorblindText(colorblindLEDText, notBlockedColors[currentColorIndex]);
    }

    void updateDisplay()
    {
        displayShapesObject.SetActive(false);

        displayText.text = "";
        for (int i = Mathf.Max(0, inputNames.Count - 3); i < inputNames.Count; i++)
        {
            if (inputNames[i][0] == 'B') //If it's a block action
            {
                string colorHex = "";
                switch (inputNames[i].Split(' ')[1])
                {
                    case "Blue":
                        colorHex = "#3A58FF";
                        break;
                    case "Green":
                        colorHex = "#00FF00";
                        break;
                    case "Red":
                        colorHex = "#FF0000";
                        break;
                    case "Yellow":
                        colorHex = "#FFFF00";
                        break;
                }

                displayText.text += "<color=" + colorHex + ">Block</color>";
            }
            else
                displayText.text += inputNames[i];

            if (i < inputNames.Count - 1)
                displayText.text += "\n";
        }
    }

    //handle other things
    void handleStart()
    {
        if (inputNames.Count <= 0 || currentlyAnimating || moduleSolved)
            return;
        LogMsg("Running the program.");
        StartCoroutine(AnimateInstructions());
    }

    void handleReset()
    {
        if (currentlyAnimating || moduleSolved)
            return;

        resetVariables(-1);
    }

    void resetVariables(int strikeType)
    {
        if (strikeType == -1)
            LogMsg("Reset.");
        Debug.LogFormat("<Robot Programming #{0}> R2D2 now acts like {1}.", ModuleId, initialR2D2Behavior ? "HAL" : "ROB");
        Debug.LogFormat("<Robot Programming #{0}> Fender now starts at the {1} character.", ModuleId, placeNames[initialCharacterIndex]);
        Debug.LogFormat("<Robot Programming #{0}> The LED is now {1}.", ModuleId, ((RobotColor) initialColorIndex).ToString());
        inputNames.Clear();

        //reset everything to what it initially was
        animationQueue.Clear();

        R2D2actsLikeHAL = initialR2D2Behavior;
        serialCharacterIndex = initialCharacterIndex;

        notBlockedColors = new List<RobotColor> { RobotColor.Blue, RobotColor.Green, RobotColor.Red, RobotColor.Yellow };
        currentColorIndex = initialColorIndex;
        updateLed();

        for (int i = 0; i < 4; i++)
        {
            sortedRobots[i].Position = sortedRobots[i].InitialPosition;
            Debug.LogFormat("<Robot Programming #{0}> {1} ({2}) is now at {3}, {4}.", ModuleId, sortedRobots[i].Type.ToString(), ((RobotColor) i).ToString(), sortedRobots[i].Position.x + 1, sortedRobots[i].Position.y + 1);
        }

        displayText.text = (strikeType == 0 ? "[ERROR]" : strikeType == 1 ? "[OOC]" : strikeType == 2 ? "[OOB]" : "[RESET]") + "\n R2D2: " + (R2D2actsLikeHAL ? "HAL" : "ROB") + "\nFender: " + placeNames[serialCharacterIndex] + "\n" + (topIndex + 1) + " " + (bottomIndex + 1) + "\n";
        displayShapesObject.SetActive(true);
        willStrike = false;
    }

    void handleStrike(RobotColor ledColor, int strikeType)
    {
        willStrike = true; //ensures willStrike is true (wouldn't be the case if the reason the mod is striking is because the robots aren't in goal positions)
        module.HandleStrike();
        bombAudio.PlaySoundAtTransform("strike", transform);

        //set everything to how it would be when the robot strikes
        initialColorIndex = (int) ledColor;
        initialR2D2Behavior = lastR2D2Behavior;
        initialCharacterIndex = lastSerialCharacterIndex;
        for (int i = 0; i < 4; i++)
        {
            sortedRobots[i].InitialPosition = sortedRobots[i].LastPosition;
        }

        resetVariables(strikeType);
    }

    void handleSolve()
    {
        LogMsg("Solved.");
        module.HandlePass();
        moduleSolved = true;
        bombAudio.PlaySoundAtTransform("solve", transform);

        LedRenderer.material = unlitColorMaterials[4];
        colorblindLEDText.text = "";
        displayText.text = "[SOLVED!]";
    }

    //checks
    bool isRobotColliding(Robot robot, bool logCrashes)
    {
        if (robot.Position.x < 0 || robot.Position.x > 8 || robot.Position.y < 0 || robot.Position.y > 8) //prevents out of bounds (which causes wall checks to error out)
        {
            if (logCrashes)
                LogMsg(robot.Type.ToString() + " (" + robot.Color.ToString() + ") is moving out of bounds. This move will cause the program to crash!");
            return true;
        }

        for (int i = 0; i < 4; i++)
        {
            if (sortedRobots[i].Position == robot.Position && sortedRobots[i] != robot) //if robot is on top of another robot
            {
                if (logCrashes)
                    LogMsg(robot.Type.ToString() + " (" + robot.Color.ToString() + ") is colliding with the " + ((RobotColor) i).ToString() + " robot. This move will cause the program to crash!");
                return true;
            }
        }

        if (maze[robot.Position.y][robot.Position.x] == 'X') //checks if robot is in a wall
        {
            if (logCrashes)
                LogMsg(robot.Type.ToString() + " (" + robot.Color.ToString() + ") is colliding with a wall. This move will cause the program to crash!");
            return true;
        }
        return false;
    }

    bool isRobotStuck(Robot robot)
    {
        Vector2Int[] directions = new Vector2Int[4] { new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1) };
        for (int i = 0; i < 4; i++)
        {
            if (!isRobotColliding(new Robot(robot.Position + directions[i]), false))
                return false;
        }
        LogMsg(robot.Type.ToString() + " (" + robot.Color.ToString() + ") cannot move! Skipping its turn.");
        return true;
    }

    bool isModuleSolved()
    {
        for (int i = 0; i < 4; i++)
        {
            if (sortedRobots[i].Position != goalPositions[i])
                return false;
        }
        return true;
    }

    //animation
    IEnumerator AnimateInstructions()
    {
        currentlyAnimating = true;
        int animationsDone = 0;
        int lastMovedRobot = -1;

        while (animationQueue.Count > 0)
        {
            var request = animationQueue.Dequeue();
            lastMovedRobot = request.RobotIndex;
            GameObject robotObject = sortedRobotObjects[request.RobotIndex];
            Vector3 startPosition = robotObject.transform.localPosition;
            Vector3 endPosition = new Vector3(Mathf.Lerp(-0.07345f, 0.00635f, sortedRobots[request.RobotIndex].Position.x / 8f), 0f, Mathf.Lerp(0.04f, -0.04f, sortedRobots[request.RobotIndex].Position.y / 8f));
            switch (request.Direction)
            {
                case Direction.Up:
                    endPosition = new Vector3(startPosition.x, startPosition.y, startPosition.z + .01f);
                    break;
                case Direction.Right:
                    endPosition = new Vector3(startPosition.x + 0.009975f, startPosition.y, startPosition.z);
                    break;
                case Direction.Down:
                    endPosition = new Vector3(startPosition.x, startPosition.y, startPosition.z - .01f);
                    break;
                case Direction.Left:
                    endPosition = new Vector3(startPosition.x - 0.009975f, startPosition.y, startPosition.z);
                    break;
            }

            float t = 0;

            if (request.Crashed) //if the move made the robot crash. WILL strike
            {
                while (t < .5f)
                {
                    t += .05f;
                    robotObject.transform.localPosition = Vector3.Lerp(startPosition, endPosition, t);
                    yield return new WaitForSeconds(.01f);
                }
                handleStrike((RobotColor) request.RobotIndex, request.OOB ? 2 : 0);
                while (t > 0)
                {
                    t -= .05f;
                    robotObject.transform.localPosition = Vector3.Lerp(startPosition, endPosition, t);
                    yield return new WaitForSeconds(.01f);
                }
                currentlyAnimating = false;
                yield break;
            }

            float speed = Mathf.Lerp(.05f, 1f, animationsDone / 40f); //gradually speed up movement
            while (t < 1)
            {
                t += speed;
                robotObject.transform.localPosition = Vector3.Lerp(startPosition, endPosition, t);
                yield return new WaitForSeconds(.01f);
            }
            animationsDone++;
        }

        if (isModuleSolved())
        {
            handleSolve();
        }
        else
        {
            LogMsg("The program didn't bring all robots to the goal. Strike!");
            int nextRobotTurn = lastMovedRobot + 1;
            if (lastMovedRobot == -1) nextRobotTurn = initialColorIndex;
            for (int i = nextRobotTurn; i < nextRobotTurn + 4; i++)
            {
                if (!isRobotStuck(sortedRobots[i % 4]))
                {
                    handleStrike(sortedRobots[i % 4].Color, 1);
                    break;
                }
            }
        }

        currentlyAnimating = false;
        yield break;
    }

    //colorblind
    void setupColorblind()
    {
        for (int i = 0; i < 4; i++)
        {
            setColorblindText(colorblindRobotTexts[i], robotColors[i]);
            if (robotShapes[i] == Shape.Triangle)
                colorblindRobotTexts[i].transform.localPosition = new Vector3(0f, -0.00192f, -0.0043f);
        }
        setColorblindText(colorblindLEDText, notBlockedColors[currentColorIndex]);
        colorblindOtherText.SetActive(true);
    }

    void setColorblindText(TextMesh text, RobotColor color)
    {
        text.text = "" + color.ToString()[0];
        if (color.EqualsAny(RobotColor.Green, RobotColor.Yellow))
            text.color = Color.black;
        else
            text.color = Color.white;
    }

    void toggleColorblind()
    {
        colorblindActive = !colorblindActive;
        if (colorblindActive)
        {
            for (int i = 0; i < 4; i++)
            {
                setColorblindText(colorblindRobotTexts[i], robotColors[i]);
                if (robotShapes[i] == Shape.Triangle)
                    colorblindRobotTexts[i].transform.localPosition = new Vector3(0f, -0.00192f, -0.0043f);
            }
            updateLed();
            colorblindOtherText.SetActive(true);
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                colorblindRobotTexts[i].text = "";
            }
            colorblindLEDText.text = "";
            colorblindOtherText.SetActive(false);
        }
    }

    //twitch plays
    enum Input
    {
        Up, Right, Down, Left, Block, Reset, Start, Colorblind
    }
    private readonly string TwitchHelpMessage = @"""!{0} left/right/up/down/l/r/u/d"" to press that button. ""!{0} block red/yellow/green/blue/r/y/g/b"" to block that color. ""!{0} reset"" to reset the program. ""!{0} start"" to start the program. ""!{0} colorblind"" to toggle colorblind.";
    IEnumerator ProcessTwitchCommand(string cmd)
    {
        var commands = cmd.ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        Queue<Input> inputs = new Queue<Input>();
        Queue<RobotColor> blocks = new Queue<RobotColor>();
        for (int i = 0; i < commands.Length; i++)
        {
            switch (commands[i])
            {
                case "up":
                case "u":
                    inputs.Enqueue(Input.Up);
                    break;
                case "right":
                case "r":
                    inputs.Enqueue(Input.Right);
                    break;
                case "down":
                case "d":
                    inputs.Enqueue(Input.Down);
                    break;
                case "left":
                case "l":
                    inputs.Enqueue(Input.Left);
                    break;
                case "reset":
                    //inputs.Clear(); //since they will all be ignored by the reset anyway
                    inputs.Enqueue(Input.Reset);
                    break;
                case "start":
                    inputs.Enqueue(Input.Start);
                    i = commands.Length; //exit out of for loop, because further commands will be ignored by the start anyway
                    break;
                case "block":
                    inputs.Enqueue(Input.Block);

                    if (i + 1 >= commands.Length)
                    {
                        yield return @"sendtochaterror ""block"" command not followed by a color.";
                        yield break;
                    }
                    switch (commands[i + 1])
                    {
                        case "blue":
                        case "b":
                            blocks.Enqueue(RobotColor.Blue);
                            break;
                        case "green":
                        case "g":
                            blocks.Enqueue(RobotColor.Green);
                            break;
                        case "red":
                        case "r":
                            blocks.Enqueue(RobotColor.Red);
                            break;
                        case "yellow":
                        case "y":
                            blocks.Enqueue(RobotColor.Yellow);
                            break;
                        default:
                            yield return @"sendtochaterror ""block"" command not followed by a color.";
                            yield break;
                    }

                    i++;
                    break;
                case "colorblind":
                    yield return null;
                    toggleColorblind();
                    break;
                default:
                    yield return $@"sendtochaterror ""{commands[i]}"" is an improper command.";
                    yield break;
            }
        }

        if (inputs.Count > 0)
        {
            yield return null;

            while (currentlyAnimating)
                yield return "trycancel";

            if (!moduleSolved)
                yield return executeCommands(inputs, blocks);
        }
    }

    IEnumerator executeCommands(Queue<Input> inputs, Queue<RobotColor> blockedColors)
    {
        while (inputs.Count > 0)
        {
            Input input = inputs.Dequeue();
            switch (input)
            {
                case Input.Up:
                    arrowButtons[0].OnInteract();
                    break;
                case Input.Right:
                    arrowButtons[1].OnInteract();
                    break;
                case Input.Down:
                    arrowButtons[2].OnInteract();
                    break;
                case Input.Left:
                    arrowButtons[3].OnInteract();
                    break;
                case Input.Reset:
                    resetButton.OnInteract();
                    break;
                case Input.Start:
                    startButton.OnInteract();
                    break;
                case Input.Block:
                    RobotColor blockedColor = blockedColors.Dequeue();
                    blockButtons[(int) blockedColor].OnInteract();
                    break;
            }

            yield return new WaitForSeconds(.1f);
        }
    }

    //logging and other functions
    void LogMsg(string msg)
    {
        Debug.LogFormat("[Robot Programming #{0}] {1}", ModuleId, msg);
    }
    void LogMsgSilent(string msg)
    {
        Debug.LogFormat("<Robot Programming #{0}> {1}", ModuleId, msg);
    }
    string LogRobots(Robot robot)
    {
        return $"{robot.Color} {robot.Shape}";
    }

    // Twitch Plays auto-solver
    IEnumerator TwitchHandleForcedSolve()
    {
        while (currentlyAnimating)
            yield return true;

        if (moduleSolved)
            yield break;

        resetButton.OnInteract();
        yield return new WaitForSeconds(.1f);

        // Generate the solution. See FindPath for full explanation.
        var solution = FindPath(sortedRobots.Select(robot => robot.InitialPosition.y * 9 + robot.InitialPosition.x).ToArray());

        // Variables to keep track of R2D2 and Fender
        var r2d2IsRob = true;
        var sn = bomb.GetSerialNumber();
        var fenderMovement = 0;

        // In the majority of cases, we will just execute the commands from start to finish.
        // However, in some cases, a robot’s movement is restricted by the presence of other robots.
        // In such a case, the module will skip that robot’s color, so we have to skip it too.
        // Therefore, at each iteration, we find the earliest command for the robot that is currently waiting for input.
        while (solution.Count > 0)
        {
            // FindPath returns values that consist of the robot color (lowest two bits) and the command (rest of the bits).
            // Find index of earliest command of the desired robot color (lowest two bits), then extract the command (by shifting the robot color out).
            var commandIx = solution.FindIndex(v => (v & 0x3) == (int) notBlockedColors[currentColorIndex]);
            var command = solution[commandIx] >> 2;

            KMSelectable btn;
            if (command == 4)
                btn = blockButtons[(int) notBlockedColors[currentColorIndex]];
            else
            {
                // Determine whether we need to press the correct or the opposite button
                var robotPersonality = sortedRobots[(int) notBlockedColors[currentColorIndex]].Type;
                var correctButton = robotPersonality == Type.ROB;
                switch (robotPersonality)
                {
                    case Type.R2D2:
                        correctButton = r2d2IsRob;
                        r2d2IsRob = !r2d2IsRob;
                        break;
                    case Type.Fender:
                        correctButton = char.IsDigit(sn[fenderMovement]);
                        fenderMovement = (fenderMovement + 1) % 6;
                        break;
                }

                // Press the appropriate button
                btn = arrowButtons[correctButton ? command : (command + 2) % 4];
            }
            btn.OnInteract();
            yield return new WaitForSeconds(.1f);
            solution.RemoveAt(commandIx);
        }

        startButton.OnInteract();
        yield return new WaitForSeconds(.1f);

        while (!moduleSolved)
            yield return true;
    }

    // Takes a list of robot starting positions (in the order B G R Y) and returns a possible solution that will navigate them all to their goal.
    // The returned integers consist of a robot color (lowest two bits) and a command (rest). Commands are Up, Right, Down, Left, or Block.
    private List<int> FindPath(int[] startPositions)
    {
        // Throughout this algorithm:
        //  • The robots are always considered to be in the order B G R Y. Robot #0 is always the blue one, etc.
        //  • The module variable ‘maze’ is re-used to determine the maze geometry and the positions of the goals.
        //  • Coordinates (positions in the 9×9 maze) are stored as values 0–80, or equivalently, as x + 9*y.
        //    Thus, X and Y coordinates are extracted from a coordinate C as X = C % 9 and Y = C / 9.
        //  • A single integer, called a “code”, is used to represent the entire game state:
        //      YRGBNNYYYYYYYRRRRRRRGGGGGGGBBBBBBB
        //      └─┬┘└┤└──┬──┘└──┬──┘└──┬──┘└──┬──┘
        //        │  │ Y pos  R pos  G pos  B pos
        //        │  │ (7 b)  (7 b)  (7 b)  (7 b)
        //        │  └─▶ which robot’s turn it is (2 bits)
        //        └─▶ which robots have already been blocked (4 bits)
        //    Since such a code uses more than 32 bits, we use a ulong to store it.

        // For each of the five possible actions (up, right, down, left, block), this is the amount by which a robot’s coordinate changes.
        // Ordinarily this would be incorrect when at the edge of the maze, but in our case, the maze is entirely cordoned off with walls
        // and the top row contains the goals, so no robot will move off the edge at any point.
        var ds = new[] { -9, 1, 9, -1, 0 };

        // Calculates the starting code. Since only the start positions are placed, everything else is 0: it will be robot #0’s turn and no robots are blocked.
        ulong startCode = (ulong) (startPositions[0] | (startPositions[1] << 7) | (startPositions[2] << 14) | (startPositions[3] << 21));

        // START OF BREADTH-FIRST SEARCH ALGORITHM

        // Queue to contain all the game states (“codes”) yet to be examined. Start with only the starting code;
        // then each iteration will add all codes that are reachable by one move, etc. until a code is found that solves the module.
        var q = new Queue<ulong>();
        q.Enqueue(startCode);

        // For each code we discovered, remember which code it was generated from and which command was executed to get there.
        var cameFrom = new Dictionary<ulong, ulong>();
        var commandsFrom = new Dictionary<ulong, int>();

        // This variable remembers the code of the goal state when we find it.
        // The value assigned here is never used unless there’s a bug, in which case this value guarantees an exception (as opposed to erratic behaviour)
        ulong endCode = ulong.MaxValue;

        while (q.Count > 0)
        {
            // LOOP ITERATION:
            // Look at a code from the queue, generate all codes reachable by one command and put them in the queue

            // Step 1: take a code from the queue and calculate which robot’s turn it is and where it is
            var code = q.Dequeue();
            var curRobot = (int) ((code >> 28) & 0x3);
            var curCoordinate = (int) ((code >> (7 * curRobot)) & 0x7F);

            // For each possible command...
            foreach (var d in ds)
            {
                // Do a “block” command (d = 0) IF AND ONLY IF the current robot is on its goal position
                if ((d == 0) != (maze[curCoordinate / 9][curCoordinate % 9] == "bgry"[curRobot]))
                    continue;

                var newCoordinate = curCoordinate + d;
                // Only consider this command if it takes the robot into an empty space or its own goal (i.e., not a wall and not another robot’s goal)
                if (maze[newCoordinate / 9][newCoordinate % 9] != '.' && maze[newCoordinate / 9][newCoordinate % 9] != "bgry"[curRobot])
                    continue;
                // Only consider this command if it doesn’t cause the robot to crash into another robot
                if (Enumerable.Range(0, 4).Any(otherRobot => otherRobot != curRobot && (int) ((code >> (7 * otherRobot)) & 0x7F) == newCoordinate))
                    continue;

                // Calculate whose turn it is next (skip robots that are already blocked)
                var newRobot = (curRobot + 1) % 4;
                while ((code & (1ul << (30 + newRobot))) != 0)
                    newRobot = (newRobot + 1) % 4;

                // Calculate the new code by:
                var newCode =
                    // — changing the current robot’s position
                    (((code & ~(0x7Ful << (7 * curRobot))) | ((ulong) newCoordinate << (7 * curRobot))) & 0xFFFFFFFul) |
                    // — updating whose turn it is
                    ((ulong) newRobot << 28) |
                    // — keeping the information on which robots are blocked
                    ((code >> 30) << 30);
                if (d == 0)
                    // — blocking the current robot if that’s what we’re doing
                    newCode |= 1ul << (30 + curRobot);

                // Only consider this command if it takes us to a new code we haven’t already seen
                if (!cameFrom.ContainsKey(newCode))
                {
                    cameFrom[newCode] = code;
                    commandsFrom[newCode] = Array.IndexOf(ds, d);

                    // If every robot is in the top row (Y coordinate = 0), consider this the solved state
                    if (Enumerable.Range(0, 4).All(robotIx => ((newCode >> (7 * robotIx)) & 0x7F) / 9 == 0))
                    {
                        endCode = newCode;
                        goto found;
                    }

                    // Otherwise, remember this new code in the queue and come back to it later to explore further
                    q.Enqueue(newCode);
                }
            }
        }

        found:
        // We found a solution! At this point, we know:
        //  • what the final code is (‘endCode’)
        //  • what earlier code each code was generated from and which command was executed to get there
        // So we start with the final code and reconstruct how we got there by iteratively tracing it back to the starting position (‘startCode’).

        var commands = new List<int>();
        var curCode = endCode;
        while (curCode != startCode)
        {
            var cmd = commandsFrom[curCode];
            curCode = cameFrom[curCode];

            // Add an integer consisting of the robot (bottom two bits) and the command (rest)
            commands.Add((cmd << 2) | ((int) (curCode >> 28) & 0x3));
        }

        // Since we went from the final position to the starting position, the commands are in reverse order, so reverse them back.
        commands.Reverse();
        return commands;
    }
}