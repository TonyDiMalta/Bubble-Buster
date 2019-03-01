using UnityEngine;

[System.Serializable]
public struct HexCoordinates {

	[SerializeField]
	private int x, y;

	public int X {
		get {
			return x;
		}
	}

	public int Y {
		get {
			return y;
		}
	}

	public HexCoordinates (int x, int y) {
		this.x = x;
		this.y = y;
	}

	public static HexCoordinates FromOffsetCoordinates (int x, int y) {
		return new HexCoordinates(x, y);
	}

    public static Vector3 PositionFromOffsetCoordinates(int x, int y) {
        Vector3 position;

        position.x = (x + y * 0.5f - y / 2) * (HexMetrics.innerRadius * 2f);
        position.y = y * (HexMetrics.outerRadius * 1.5f);
        position.z = 0f;

        return position;
    }

    public static HexCoordinates FromPosition (Vector3 position) {
        HexCoordinates coordinates = new HexCoordinates();

        float fy = position.y / (HexMetrics.outerRadius * 1.5f);
        coordinates.y = Mathf.RoundToInt(fy);

        float fx = position.x / (HexMetrics.innerRadius * 2f) - coordinates.y * 0.5f + coordinates.y / 2;
        coordinates.x = Mathf.RoundToInt(fx);

        return coordinates;
    }

	public override string ToString () {
		return "(" + X.ToString() + ", " + Y.ToString() + ")";
	}

	public string ToStringOnSeparateLines () {
		return X.ToString() + "\n" + Y.ToString();
	}
}