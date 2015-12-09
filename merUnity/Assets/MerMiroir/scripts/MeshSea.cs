using UnityEngine;
using System.Collections;

public class MeshSea : MonoBehaviour {

	public PlusieurVagues vagues;
	public float xSize;
	public float zSize;
	public int xCount;
	public int zCount;
	public float textureSize;

	Vector3[] vertices;
	Color[] colors;
	Vector3[] verticesImages;
	Vector3[] normals;
	Vector4[] tangents;
	Vector2[] uv;
	int[] triangles;
	// Use this for initialization
	void Start () {
		Mesh mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
		vertices = new Vector3[(xCount+1)*(zCount+1)];
		colors = new Color[vertices.Length];
		verticesImages = new Vector3[vertices.Length];
		normals = new Vector3[vertices.Length];
		tangents = new Vector4[vertices.Length];
		uv = new Vector2[vertices.Length];
		triangles = new int[xCount*zCount*6];
		float xStart = -xSize/2.0f;
		float zStart = -zSize/2.0f;
		float deltaX = xSize/xCount;
		float deltaZ = zSize/zCount;
		int i = 0;
		for (int x = 0; x<(xCount+1); ++x)
		{
			for (int z = 0; z <(zCount+1); ++z)
			{
				vertices[i] = new Vector3(xStart + x*deltaX,0.0f,zStart + z*deltaZ);
				colors[i] = Color.gray;
				normals[i] = Vector3.up;
				tangents[i] = Vector3.right;
				uv[i] = new Vector2( (float)x/xCount ,(float)z/zCount );
				++i;
			}
		}
		int ti = 0;
		int xOffset = zCount+1;
		for (int x=0; x<xCount; ++x)
		{
			for (int z=0; z<zCount; ++z)
			{
				int i0 = x*xOffset + z;
				int i1 = i0+ 1;
				int i2 = i0 + xOffset;
				int i3 = i0 + xOffset + 1;
				triangles[ti] = i1;
				++ti;
				triangles[ti] = i2;
				++ti;
				triangles[ti] = i0;
				++ti;
				triangles[ti] = i1;
				++ti;
				triangles[ti] = i3;
				++ti;
				triangles[ti] = i2;
				++ti;
			}
		}
		mesh.vertices = vertices;
		mesh.colors = colors;
		mesh.normals = normals;
		mesh.tangents = tangents;
		mesh.uv = uv;
		mesh.triangles = triangles;
	}
	
	// Update is called once per frame
	void Update () {
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		for (int i=0; i<(xCount+1)*(zCount+1); ++i)
		{
			Vector3 normal = new Vector3();
			Vector4 tangentX = new Vector4();
			Vector3 absoluteVertex = vertices[i]+transform.position;
            vagues.SeaManager.CalculeImage(absoluteVertex, out verticesImages[i], out normal, out tangentX);
			verticesImages[i] = transform.InverseTransformPoint(verticesImages[i]);
			Color color = Color.gray;
		    Vector3 cross = Vector3.Cross(normal,Vector3.up);
			if ( (cross.magnitude >0.5) && (normal.x <0.0))
			  { 
				color = Color.gray + 4f*(cross.magnitude-0.5f) * (Color.white - Color.gray); 
			  }
			colors[i]= color;
			colors[i]= Color.red;
			Debug.DrawLine(transform.position + verticesImages[i],transform.position + verticesImages[i]+normal,color);
			normals[i] = normal;
			tangents[i] = tangentX;
			uv[i] = new Vector2( absoluteVertex.x/textureSize ,absoluteVertex.z/textureSize );

		}
		mesh.vertices = verticesImages;
		mesh.colors = colors;
		mesh.normals = normals;
		mesh.tangents = tangents;
		mesh.uv = uv;

	}
}
