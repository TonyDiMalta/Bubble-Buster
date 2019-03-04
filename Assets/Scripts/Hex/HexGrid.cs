using Assets.Scripts;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour {

    [SerializeField, Range(3, 10)]
    private int width = 7;
    [SerializeField, Range(6, 20)]
    private int height = 13;

    public int Width
    {
        get
        {
            return width;
        }
    }

    public int Height
    {
        get
        {
            return height;
        }
    }

    [SerializeField, Range(1, 20)]
    private int numberLinesToFill = 5;

    [SerializeField, Range(1, 10)]
    private uint turnsCountBeforeEvent = 5;
    private uint numberOfTurns = 0;

    public Color defaultColor = Color.white;

	public HexCell cellPrefab;
	public Text cellLabelPrefab;
    public GameObject bubblePrefab;

    HexCell[,] cells;

	Canvas gridCanvas;
	HexMesh hexMesh;

    private static HexCoordinates[] evenRowNeighborsCoordinates =
    {
        new HexCoordinates(-1, -1),
        new HexCoordinates(-1, 0),
        new HexCoordinates(-1, 1),
        new HexCoordinates(0, 1),
        new HexCoordinates(1, 0),
        new HexCoordinates(0, -1)
    };

    private static HexCoordinates[] oddRowNeighborsCoordinates =
    {
        new HexCoordinates(0, -1),
        new HexCoordinates(-1, 0),
        new HexCoordinates(0, 1),
        new HexCoordinates(1, 1),
        new HexCoordinates(1, 0),
        new HexCoordinates(1, -1)
    };

    void Awake () {
		gridCanvas = GetComponentInChildren<Canvas>();
		hexMesh = GetComponentInChildren<HexMesh>();

        //Calculate grid position and scale according to the aspect ratio.
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(-Camera.main.transform.localPosition);
        Vector3 outerRadius = new Vector3(HexMetrics.outerRadius, HexMetrics.outerRadius, 0f);
        transform.localScale = transform.localScale * Camera.main.aspect / (200f / 300f);
        transform.position = worldPos + Vector3.Scale(outerRadius, transform.lossyScale);

        //Adapt the grid size to fit the screen.
        Vector3 maxWorldPos = Camera.main.ViewportToWorldPoint(new Vector3(Camera.main.rect.width - Camera.main.rect.x, Camera.main.rect.height - Camera.main.rect.y, 0f));
        Vector3 maxLocalPos = transform.InverseTransformPoint(maxWorldPos);
        HexCoordinates maxCoordinates = HexCoordinates.FromPosition(maxLocalPos);
        width = maxCoordinates.X;
        height = maxCoordinates.Y - 1; //Remove 1 to keep one extra line for the score display.

        cells = new HexCell[width, height];

		for (int y = 0; y < height; ++y) {
			for (int x = 0; x < width; ++x) {
				CreateCell(x, y);
			}
		}
	}

	void Start ()
    {
        if (Debug.isDebugBuild)
        {
            hexMesh.Triangulate(cells);
        }
	}

	public void ColorCell (Vector3 position, Color color)
    {
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
        if (AreCoordinatesInRange(coordinates.X, coordinates.Y) == true)
        {
            HexCell cell = cells[coordinates.X, coordinates.Y];
            cell.color = color;

            if (Debug.isDebugBuild)
            {
                hexMesh.Triangulate(cells);
            }
        }
	}

	void CreateCell (int x, int y)
    {
		Vector3 position = HexCoordinates.PositionFromOffsetCoordinates(x, y);

        HexCell cell = cells[x, y] = Instantiate<HexCell>(cellPrefab);
        cell.name = "Cell[" + x.ToString() + ", " + y.ToString() + "]";
        cell.transform.SetParent(transform, false);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, y);
        cell.color = defaultColor;

        if (y >= height - numberLinesToFill)
        {
            CreateBubble(cell);
        }
        else
        {
            cell.bubble = null;
        }

        if (Debug.isDebugBuild)
        {
            Text label = Instantiate<Text>(cellLabelPrefab);
            label.rectTransform.SetParent(gridCanvas.transform, false);
            label.rectTransform.anchoredPosition =
                new Vector2(position.x, position.y);
            label.text = cell.coordinates.ToStringOnSeparateLines();
        }
    }

    void CreateBubble(HexCell cell)
    {
        GameObject go = Instantiate(bubblePrefab, new Vector3(0f, 0f, 1f), Quaternion.identity); //create a bubble
        go.transform.SetParent(cell.transform, false); //attach the bubble to the cell

        MyMaterial material = MyMaterial.GetRandomMaterial(); //get a random color
        var renderer = go.transform.GetComponent<Renderer>();
        renderer.material = material; //set the color
        go.name = go.tag + "(" + material.colorName + ")";

        var bubble = go.GetComponent<Bubble>();
        bubble.material = material;
        bubble.isOnBoard = true;
        cell.bubble = bubble; //add the new bubble to the cell
    }

    public Bubble CreateBubble(Transform parent, MyMaterial material)
    {
        GameObject go = Instantiate(bubblePrefab, new Vector3(0f, 0f, 1f), Quaternion.identity); //create a bubble
        go.transform.SetParent(parent, false); //attach the bubble to the parent
        
        var renderer = go.transform.GetComponent<Renderer>();
        renderer.material = material; //set the color
        go.name = go.tag + "(" + material.colorName + ")";

        var bubble = go.GetComponent<Bubble>();
        bubble.material = material;
        bubble.isOnBoard = false;
        return bubble; //return the new bubble
    }

    public void AddBubbleToCoordinates(int x, int y, Bubble bubble)
    {
        if (AreCoordinatesInRange(x, y) == true &&
            cells[x, y].bubble == null)
        {
            cells[x, y].bubble = bubble;
            bubble.isOnBoard = true;
            Transform bubbleTransform = bubble.transform;
            bubbleTransform.SetParent(cells[x, y].transform, false);
            Rigidbody rb = bubble.GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeAll;
            bubbleTransform.localPosition = new Vector3(0f, 0f, 1f);
        }
    }

    public void RemoveBubbleFromCoordinates(int x, int y)
    {
        if (AreCoordinatesInRange(x, y) == true)
        {
            Destroy(cells[x, y].bubble.gameObject, 0f);
            cells[x, y].bubble = null;
        }
    }

    private bool AreCoordinatesInRange(int x, int y, bool shouldRaiseExceptions = true)
    {
        bool isInRange = 
            x >= 0 && x < width &&
            y >= 0 && y < height;
        Assert.IsFalse(shouldRaiseExceptions == true && isInRange == false,
            GetType() + "::" + MethodBase.GetCurrentMethod().Name + "(" + 
            x.ToString() + ", " + y.ToString() + 
            ") is outside of the range (" + 
            width.ToString() + ", " + height.ToString() + ")");
        return isInRange;
    }

    public bool HasCellBubble(int x, int y, bool shouldRaiseExceptions = true)
    {
        if (AreCoordinatesInRange(x, y, shouldRaiseExceptions) == true &&
            cells[x, y].bubble != null)
        {
            return true;
        }

        return false;
    }

    private List<HexCell> GetBubbleNeighbors(int x, int y)
    {
        if (AreCoordinatesInRange(x, y) == false)
        {
            return null;
        }

        var neighbors = new List<HexCell>();

        if (y % 2 == 0)
        {
            foreach (var coordinates in evenRowNeighborsCoordinates)
            {
                int x2 = x + coordinates.X;
                int y2 = y + coordinates.Y;
                if (HasCellBubble(x2, y2, false) == true)
                {
                    neighbors.Add(cells[x2, y2]);
                }
            }
        }
        else
        {
            foreach (var coordinates in oddRowNeighborsCoordinates)
            {
                int x2 = x + coordinates.X;
                int y2 = y + coordinates.Y;
                if (HasCellBubble(x2, y2, false) == true)
                {
                    neighbors.Add(cells[x2, y2]);
                }
            }
        }

        return neighbors;
    }

    public List<HexCoordinates> GetClusterFromCoordinates(int x, int y, bool shouldIgnoreColor = false)
    {
        if (AreCoordinatesInRange(x, y) == false)
        {
            return null;
        }

        HexCell originalCell = cells[x, y];

        if (originalCell.bubble == null)
        {
            return null;
        }

        string originalColor = originalCell.bubble.material.colorName;
        
        var cellsToProcess = new Stack<HexCell>();
        cellsToProcess.Push(originalCell);
        var clusterCells = new List<HexCoordinates>();

        while (cellsToProcess.Count > 0)
        {
            var currentCell = cellsToProcess.Pop();
            var currentCoordinates = currentCell.coordinates;
            
            if (currentCell.bubble == null ||
                clusterCells.Contains(currentCoordinates) == true)
            {
                continue;
            }
            
            if (shouldIgnoreColor == true ||
                currentCell.bubble.material.colorName == originalColor)
            {
                clusterCells.Add(currentCoordinates);
                var neighbors = GetBubbleNeighbors(currentCoordinates.X, currentCoordinates.Y);

                foreach (var cell in neighbors)
                {
                    if (clusterCells.Contains(cell.coordinates) == false)
                    {
                        cellsToProcess.Push(cell);
                    }
                }
            }
        }
        
        return clusterCells;
    }

    public List<HexCoordinates> GetFloatingClusters()
    {
        var floatingClusters = new List<HexCoordinates>();
        var processedCells = new List<HexCoordinates>();

        foreach (var cell in cells)
        {
            HexCoordinates cellCoordinates = cell.coordinates;
            if (cell.bubble != null &&
                processedCells.Contains(cellCoordinates) == false)
            {
                var clusterRange = GetClusterFromCoordinates(cellCoordinates.X, cellCoordinates.Y, true);

                if (clusterRange == null ||
                    clusterRange.Count == 0)
                {
                    continue;
                }

                processedCells.AddRange(clusterRange);

                bool isFloatingCluster = true;
                foreach (var clusterCellCoordinate in clusterRange)
                {
                    if (clusterCellCoordinate.Y == height - 1)
                    {
                        isFloatingCluster = false;
                        break;
                    }
                }

                if (isFloatingCluster == true)
                {
                    floatingClusters.AddRange(clusterRange);
                }
            }
        }

        return floatingClusters;
    }

    public void AddNewBubblesRow()
    {
        for (int y = 0; y < height - 1; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                var bubbleToMove = cells[x, y + 1].bubble;

                if (bubbleToMove != null)
                {
                    var bubbleToMoveTransform = bubbleToMove.gameObject.transform;
                    var currentCellTransform = cells[x, y].transform;
                    bubbleToMoveTransform.SetParent(currentCellTransform, false);
                    cells[x, y].bubble = bubbleToMove;
                    cells[x, y + 1].bubble = null;
                }
            }
        }

        for (int x = 0; x < width; ++x)
        {
            CreateBubble(cells[x, height - 1]);
        }
    }

    public void SetTurnOver()
    {
        ++numberOfTurns;
    }

    public bool IsTurnEventCountReached()
    {
        return numberOfTurns % turnsCountBeforeEvent == 0;
    }
}