using UnityEngine;
using System.Collections;

public class AdaptativeMeshSea : MonoBehaviour {

	public PlusieurVagues vagues;
	public Camera pointDeVue;
	public float distance;
	public int precision;
	public float textureSize;
	public float forwardDistance;

	Vector3[] vertices;
	//Vector3[] verticesImages;
	Vector3[] normals;
	Vector4[] tangents;
	Vector2[] uv;
	int[] triangles;

	// Use this for initialization
	void Start () {
		//TODO precision differente en x et z, ou fontion de aspect ratio ?
		int xCount = precision;
		int zCount = precision;
		//TODO size pas utile ?, plutot calculer directement la position des vertices ?
		float xSize =1.0f;
		float zSize = 1.0f;
		Mesh mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
		vertices = new Vector3[(xCount+1)*(zCount+1)];
		//verticesImages = new Vector3[vertices.Length];
		normals = new Vector3[vertices.Length];
		tangents = new Vector4[vertices.Length];
		uv = new Vector2[vertices.Length];
		triangles = new int[xCount*zCount*6];
		float xStart = 0.0f;
		float zStart = 0.0f;
		float deltaX = xSize/xCount;
		float deltaZ = zSize/zCount;
		int i = 0;
		Vector4 tangent = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
		for (int x = 0; x<(xCount+1); ++x)
		{
			for (int z = 0; z <(zCount+1); ++z)
			{
				vertices[i] = new Vector3(xStart + x*deltaX,0.0f,zStart + z*deltaZ);
				normals[i] = Vector3.up;
				tangents[i] = tangent;
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
		mesh.normals = normals;
		mesh.tangents = tangents;
		mesh.uv = uv;
		mesh.triangles = triangles;

        m_renderer = GetComponent<Renderer>();
    }
	
	// Update is called once per frame
	void Update () {
		Vector3 basGauche;
		Vector3 hautGauche;
		Vector3 hautDroite;
		Vector3 basDroite;
		if (voitLaMer_calculePointsSurLeFrustumDroit(pointDeVue.transform,pointDeVue.fieldOfView,pointDeVue.aspect,distance,
		                                             out hautGauche, out hautDroite, out basGauche, out basDroite))
		{
			Debug.DrawLine (basGauche, hautGauche, Color.magenta);
			Debug.DrawLine (hautGauche, hautDroite, Color.magenta);
			Debug.DrawLine (hautDroite, basDroite, Color.magenta);
			Debug.DrawLine (basDroite, basGauche, Color.magenta);

			if (m_renderer.material.HasProperty("_LeftBottom"))
			{
				m_renderer.material.SetVector("_LeftBottom",new Vector4(basGauche.x,basGauche.y,basGauche.z,1.0f));
			}
			if (m_renderer.material.HasProperty("_LeftRight"))
			{
				Vector3 gaucheDroite = basDroite - basGauche;
				m_renderer.material.SetVector("_LeftRight",new Vector4(gaucheDroite.x,gaucheDroite.y,gaucheDroite.z,0.0f));
			}
			if (m_renderer.material.HasProperty("_BottomTop"))
			{
				Vector3 basHaut = hautGauche - basGauche;
				m_renderer.material.SetVector("_BottomTop",new Vector4(basHaut.x,basHaut.y,basHaut.z,1.0f));
			}
			/* Tout ceci se retrouve dans seaSurface2Shader
			 * 
			Vector3 horizontalStep = (basDroite - basGauche) / precision;
			Vector3 verticalStep = (hautGauche - basGauche ) / precision;
			Vector3 horizontalOffset = Vector3.zero;
			Gizmos.color = Color.magenta;
			Vector3 forwardCorrection = Vector3.Scale(pointDeVue.transform.forward, new Vector3(-forwardDistance, 0.0f, -forwardDistance));
			for (int hi=0;hi<=precision; ++hi)
			{
				Vector3 verticalOffset = Vector3.zero;
				for (int vi=0; vi<=precision; ++vi)
				{
					Vector3 current = basGauche + horizontalOffset + verticalOffset;
					Vector3 v = current - pointDeVue.transform.position;
					vertices[hi * (precision+1)+ vi] = pointDeVue.transform.position + v * ( -pointDeVue.transform.position.y / v.y) + forwardCorrection;
					verticalOffset += verticalStep;
				}
				horizontalOffset += horizontalStep;
			}


			Mesh mesh = GetComponent<MeshFilter>().mesh;
			for (int i=0; i<vertices.Length; ++i)
			{
				Vector3 normal = new Vector3();
				Vector4 tangentX = new Vector4();
				Vector3 absoluteVertex = vertices[i]+transform.position;
				vagues.CalculeImage(absoluteVertex, out verticesImages[i], out normal, out tangentX);
				verticesImages[i] = transform.InverseTransformPoint(verticesImages[i]);
				normals[i] = normal;
				tangents[i] = tangentX;
			    
				Debug.DrawRay(transform.position + verticesImages[i],normal,Color.red);
				Debug.DrawRay(transform.position + verticesImages[i],tangentX,Color.green);
				uv[i] = new Vector2( absoluteVertex.x/textureSize ,absoluteVertex.z/textureSize );
				
			}
			mesh.vertices = verticesImages;
			mesh.vertices = vertices;
			mesh.normals = normals;
			mesh.tangents = tangents;
			mesh.uv = uv;
			*/

			//mesh.RecalculateBounds();  may not work because of the vertex shader
			MeshFilter meshFilter = m_renderer.GetComponent<MeshFilter>();
			meshFilter.sharedMesh.bounds = new Bounds (
				Camera.main.transform.position, 
				Vector3.one * Camera.main.farClipPlane);
		}		

	}

	//4 "coins" du frustum a une distance donnee, par rapport au repere global
	void pointsSurLeFrustum(Transform origine, float fieldOfView, float aspect, float distance,
	                     out Vector3 hautGauche, out Vector3 hautDroite, out Vector3 basGauche, out Vector3 basDroite)
	{
		float demiHauteur = distance * Mathf.Tan(fieldOfView * 0.5f * Mathf.Deg2Rad );
		float demiLargeur = demiHauteur * aspect;
		hautDroite = origine.TransformPoint(demiLargeur, demiHauteur, distance);
		hautGauche = origine.TransformPoint(-demiLargeur, demiHauteur, distance);
		basDroite = origine.transform.TransformPoint(demiLargeur,-demiHauteur, distance);
		basGauche = origine.transform.TransformPoint(-demiLargeur,-demiHauteur, distance);
	}

	//3 vecteurs de la nouvelle base, dans le sens de forward mais up vers le haut, sauf si -90 ou 90, dans ce cas meme base
	//le tout par rapport au repere global
	void baseRedressee(Transform origine, out Vector3 forward, out Vector3 up, out Vector3 right)
	{
		//
		forward = origine.forward;
		right = origine.right;
		up = origine.up;
		float ax = origine.rotation.eulerAngles.x;
		if ( (!Mathf.Approximately(ax,90.0f)) && (!Mathf.Approximately(ax,270.0f)) )
		{ 
			right = Vector3.Cross(Vector3.up,forward).normalized;
			up = Vector3.Cross(forward,right);
		}
	}

	//4 points de nouveau frustum, oriente selon nouvelle base, qui contient l'ancien frustum, distance near
	//par rapport au repere global
	void pointsSurLeNouveauFrustum(Vector3 position, Vector3 forward, Vector3 up, Vector3 right, float distance,
	                                  Vector3 hautGauche, Vector3 hautDroite, Vector3 basGauche, Vector3 basDroite,
	                                  out Vector3 hautGauche2, out Vector3 hautDroite2, out Vector3 basGauche2, out Vector3 basDroite2)
	{
		float d1 = Vector3.Dot(hautDroite-position,right);
		float d2 = Vector3.Dot(hautGauche-position,right);
		float d3 = Vector3.Dot(basDroite-position,right);
		float d4 = Vector4.Dot(basGauche-position,right);
		Vector3 leftSquare = right * Mathf.Min(d1,d2,d3,d4);
		Vector3 rightSquare = right * Mathf.Max(d1,d2,d3,d4);
		d1 = Vector3.Dot(hautDroite-position,up);
		d2 = Vector3.Dot(hautGauche-position,up);
		d3 = Vector3.Dot(basDroite-position,up);
		d4 = Vector4.Dot(basGauche-position,up);
		Vector3 downSquare = up * Mathf.Min(d1,d2,d3,d4);
		Vector3 upSquare = up * Mathf.Max(d1,d2,d3,d4);
		Vector3 start = position + distance * forward;
		basGauche2 = start+downSquare+leftSquare;
		hautGauche2 = start+upSquare+leftSquare;
		hautDroite2 = start+upSquare+rightSquare;
		basDroite2 = start+downSquare+rightSquare;
	}

	//les deux points hauts peuvent etre baisses s'ils sont au dessus de l'horizon (aka y 0)
	//TODO prendre en compte la hauteur maximum des vagues : implique pb de parfois pas projetable ?
	//retourn vrai si le rectangle "voit" la mer
	bool voitLaMer_reduirePointSurFrustumDroit(ref Vector3 hautGauche, ref Vector3 hautDroite, ref Vector3 basGauche, ref Vector3 basDroite)
	{
		if (hautGauche.y >0.0f)
		{
			Vector3 v = hautGauche - basGauche;
			Vector3 p = basGauche + v * ( -basGauche.y / v.y );
			if (p.y > basGauche.y)
			{
				hautGauche = p;
				v = hautDroite - basDroite;
				hautDroite = basDroite + v * ( -basDroite.y / v.y);
				
			}
		}
		return (basDroite.y<=0.0f);
	}

	bool voitLaMer_calculePointsSurLeFrustumDroit(Transform origine, float fieldOfView, float aspect, float distance,
	                                         out Vector3 hautGauche, out Vector3 hautDroite, out Vector3 basGauche, out Vector3 basDroite)
	{
	    //4 points du frustum, distance 
		Vector3 hautDroite0;
		Vector3 hautGauche0;
		Vector3 basDroite0;
		Vector3 basGauche0;
		pointsSurLeFrustum(origine,fieldOfView,aspect,distance,
		                   out hautGauche0,out hautDroite0,out basGauche0,out basDroite0);
/*		Debug.DrawLine(hautDroite0,hautGauche0,Color.gray);
		Debug.DrawLine(hautDroite0,basDroite0,Color.gray);
		Debug.DrawLine(basDroite0,basGauche0,Color.gray);
		Debug.DrawLine(hautGauche0,basGauche0,Color.gray);*/
		
		//3 vecteurs de la nouvelle base, dans le sens de forward mais up vers le haut, sauf si -90 et 90, dans ce cas meme base
		Vector3 forward;
		Vector3 up;
		Vector3 right;
		baseRedressee(origine.transform,out forward,out up,out right);
		Vector3 p0 = origine.transform.position;
/*	Debug.DrawRay(p0,forward,Color.blue);
		Debug.DrawRay(p0,right,Color.red);
		Debug.DrawRay(p0,up,Color.green);*/

		pointsSurLeNouveauFrustum(p0,forward,up,right,distance,
		                          hautGauche0,hautDroite0,basGauche0,basDroite0,
		                          out hautGauche,out hautDroite,out basGauche,out basDroite);
		
/*	Debug.DrawLine (basGauche, hautGauche, Color.cyan);
		Debug.DrawLine (hautGauche, hautDroite, Color.cyan);
		Debug.DrawLine (hautDroite, basDroite, Color.cyan);
		Debug.DrawLine (basDroite, basGauche, Color.cyan);*/
		
		return (voitLaMer_reduirePointSurFrustumDroit(ref hautGauche,ref hautDroite,ref basGauche,ref basDroite));
	}

	void OnDrawGizmos() {
		//4 points du frustum, distance 
		Vector3 basGauche;
		Vector3 hautGauche;
		Vector3 hautDroite;
		Vector3 basDroite;
		if (voitLaMer_calculePointsSurLeFrustumDroit(pointDeVue.transform,pointDeVue.fieldOfView,pointDeVue.aspect,distance,
		                                        out hautGauche, out hautDroite, out basGauche, out basDroite))
		{
			Debug.DrawLine (basGauche, hautGauche, Color.magenta);
			Debug.DrawLine (hautGauche, hautDroite, Color.magenta);
			Debug.DrawLine (hautDroite, basDroite, Color.magenta);
			Debug.DrawLine (basDroite, basGauche, Color.magenta);
		}		
	}

    private Renderer m_renderer;
}
