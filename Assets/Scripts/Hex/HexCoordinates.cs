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

	public static HexCoordinates FromPosition (Vector3 position) {
		float x = position.x / (HexMetrics.innerRadius * 2f);
		float z = -x;

		float offset = position.y / (HexMetrics.outerRadius * 1.5f);
		z -= offset;

		int iX = Mathf.RoundToInt(x);
		int iY = Mathf.RoundToInt(-x -z);
		int iZ = Mathf.RoundToInt(z);

		if (iX + iY + iZ != 0) {
			float dX = Mathf.Abs(x - iX);
			float dY = Mathf.Abs(-x -z - iY);
			float dZ = Mathf.Abs(z - iZ);

			if (dX > dY && dX > dZ) {
				iX = -iY - iZ;
			}
			else if (dY > dZ) {
				iY = -iX - iZ;
			}
		}

		return new HexCoordinates(iX, iY);
	}

	public override string ToString () {
		return "(" + X.ToString() + ", " + Y.ToString() + ")";
	}

	public string ToStringOnSeparateLines () {
		return X.ToString() + "\n" + Y.ToString();
	}
}