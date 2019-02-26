using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour {

    [SerializeField]
    private int width = 6, height = 6;

	public Color defaultColor = Color.white;

	public HexCell cellPrefab;
	public Text cellLabelPrefab;

	HexCell[] cells;

	Canvas gridCanvas;
	HexMesh hexMesh;

	void Awake () {
		gridCanvas = GetComponentInChildren<Canvas>();
		hexMesh = GetComponentInChildren<HexMesh>();

		cells = new HexCell[height * width];

		for (int y = 0, i = 0; y < height; ++y) {
			for (int x = 0; x < width; ++x) {
				CreateCell(x, y, i++);
			}
		}
	}

	void Start () {
		hexMesh.Triangulate(cells);
	}

	public void ColorCell (Vector3 position, Color color) {
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
        if (coordinates.X >= 0 && coordinates.X < width &&
            coordinates.Y >= 0 && coordinates.Y < height)
        {
            int index = coordinates.X + coordinates.Y * width;
            HexCell cell = cells[index];
            cell.color = color;
            hexMesh.Triangulate(cells);
        }
	}

	void CreateCell (int x, int y, int i) {
		Vector3 position;
		position.x = (x + y * 0.5f - y / 2) * (HexMetrics.innerRadius * 2f);
		position.y = y * (HexMetrics.outerRadius * 1.5f);
		position.z = 0f;

        HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
		cell.transform.SetParent(transform, false);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, y);
		cell.color = defaultColor;

		Text label = Instantiate<Text>(cellLabelPrefab);
		label.rectTransform.SetParent(gridCanvas.transform, false);
		label.rectTransform.anchoredPosition =
			new Vector2(position.x, position.y);
		label.text = cell.coordinates.ToStringOnSeparateLines();
	}
}