using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine;
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

		cells = new HexCell[width, height];

		for (int y = 0; y < height; ++y) {
			for (int x = 0; x < width; ++x) {
				CreateCell(x, y);
			}
		}
	}

	void Start () {
		hexMesh.Triangulate(cells);
	}

	public void ColorCell (Vector3 position, Color color) {
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
        if (AreCoordinatesInRange(coordinates.X, coordinates.Y) == true)
        {
            HexCell cell = cells[coordinates.X, coordinates.Y];
            cell.color = color;
            hexMesh.Triangulate(cells);
        }
	}

	void CreateCell (int x, int y) {
		Vector3 position;
		position.x = (x + y * 0.5f - y / 2) * (HexMetrics.innerRadius * 2f);
		position.y = y * (HexMetrics.outerRadius * 1.5f);
		position.z = 0f;

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

		Text label = Instantiate<Text>(cellLabelPrefab);
		label.rectTransform.SetParent(gridCanvas.transform, false);
		label.rectTransform.anchoredPosition =
			new Vector2(position.x, position.y);
		label.text = cell.coordinates.ToStringOnSeparateLines();
    }

    void CreateBubble(HexCell cell)
    {
        MyMaterial material = MyMaterial.GetRandomMaterial(); //get a random color
                                                              //create a new bubble
        var go = (GameObject)Instantiate(bubblePrefab, new Vector3(0f, 0f, 1f), Quaternion.identity);
        go.transform.SetParent(cell.transform, false);
        go.tag = material.ColorName;
        cell.bubble = new Bubble(go, material);

        var renderer = go.transform.GetComponent<Renderer>();
        renderer.material = material; //set the color
    }

    public void AddBubbleToCoordinates(int x, int y)
    {
        if (AreCoordinatesInRange(x, y) == true &&
            cells[x, y].bubble == null)
        {
            CreateBubble(cells[x, y]);
        }
    }

    public void RemoveBubbleFromCoordinates(int x, int y)
    {
        if (AreCoordinatesInRange(x, y) == true)
        {
            Destroy(cells[x, y].bubble.GameObject, 0f);
            cells[x, y].bubble = null;
        }
    }

    private bool AreCoordinatesInRange(int x, int y)
    {
        if (x >= 0 && x < width &&
            y >= 0 && y < height)
        {
            return true;
        }

        return false;
    }

    public bool HasCellBubble(int x, int y)
    {
        if (AreCoordinatesInRange(x, y) == true &&
            cells[x, y].bubble != null)
        {
            return true;
        }

        return false;
    }

    private List<HexCell> GetBubbleNeighbors(int x, int y)
    {
        var neighbors = new List<HexCell>();

        if (y % 2 == 0)
        {
            foreach (var coordinates in evenRowNeighborsCoordinates)
            {
                int x2 = x + coordinates.X;
                int y2 = y + coordinates.Y;
                if (HasCellBubble(x2, y2) == true)
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
                if (HasCellBubble(x2, y2) == true)
                {
                    neighbors.Add(cells[x2, y2]);
                }
            }
        }

        return neighbors;
    }

    public List<HexCoordinates> GetClusterFromCoordinates(int x, int y)
    {
        HexCell originalCell = cells[x, y];

        if (originalCell.bubble == null)
        {
            return null;
        }

        string originalTag = originalCell.bubble.GameObject.tag;
        
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
            
            if (currentCell.bubble.GameObject.CompareTag(originalTag) == true)
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
}