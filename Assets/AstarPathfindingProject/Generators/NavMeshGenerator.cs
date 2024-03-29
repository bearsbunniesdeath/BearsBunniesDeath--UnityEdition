using UnityEngine;
using System.Collections.Generic;
using Pathfinding.Serialization;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace Pathfinding {
	public interface INavmesh {
		void GetNodes (GraphNodeDelegateCancelable del);
	}

	[System.Serializable]
	[JsonOptIn]
	/** Generates graphs based on navmeshes.
	 * \ingroup graphs
	 * Navmeshes are meshes where each polygon define a walkable area.
	 * These are great because the AI can get so much more information on how it can walk.
	 * Polygons instead of points mean that the funnel smoother can produce really nice looking paths and the graphs are also really fast to search
	 * and have a low memory footprint because of their smaller size to describe the same area (compared to grid graphs).
	 * \see Pathfinding.RecastGraph
	 *
	 * \shadowimage{navmeshgraph_graph.png}
	 * \shadowimage{navmeshgraph_inspector.png}
	 *
	 */
	public class NavMeshGraph : NavGraph, INavmesh, IUpdatableGraph, INavmeshHolder {
		/** Mesh to construct navmesh from */
		[JsonMember]
		public Mesh sourceMesh;

		/** Offset in world space */
		[JsonMember]
		public Vector3 offset;

		/** Rotation in degrees */
		[JsonMember]
		public Vector3 rotation;

		/** Scale of the graph */
		[JsonMember]
		public float scale = 1;

		/** More accurate nearest node queries.
		 * When on, looks for the closest point on every triangle instead of if point is inside the node triangle in XZ space.
		 * This is slower, but a lot better if your mesh contains overlaps (e.g bridges over other areas of the mesh).
		 * Note that for maximum effect the Full Get Nearest Node Search setting should be toggled in A* Inspector Settings.
		 */
		[JsonMember]
		public bool accurateNearestNode = true;

		public TriangleMeshNode[] nodes;

		public TriangleMeshNode[] TriNodes {
			get { return nodes; }
		}

		public override void GetNodes (GraphNodeDelegateCancelable del) {
			if (nodes == null) return;
			for (int i = 0; i < nodes.Length && del(nodes[i]); i++) {}
		}

		public override void OnDestroy () {
			base.OnDestroy();

			// Cleanup
			TriangleMeshNode.SetNavmeshHolder(active.astarData.GetGraphIndex(this), null);
		}

		public Int3 GetVertex (int index) {
			return vertices[index];
		}

		public int GetVertexArrayIndex (int index) {
			return index;
		}

		public void GetTileCoordinates (int tileIndex, out int x, out int z) {
			//Tiles not supported
			x = z = 0;
		}

		/** Bounding Box Tree. Enables really fast lookups of nodes. \astarpro */
		BBTree _bbTree;
		public BBTree bbTree {
			get { return _bbTree; }
			set { _bbTree = value; }
		}

		[System.NonSerialized]
		Int3[] _vertices;

		public Int3[] vertices {
			get {
				return _vertices;
			}
			set {
				_vertices = value;
			}
		}

		[System.NonSerialized]
		Vector3[] originalVertices;

		[System.NonSerialized]
		public int[] triangles;

		public void GenerateMatrix () {
			SetMatrix(Matrix4x4.TRS(offset, Quaternion.Euler(rotation), new Vector3(scale, scale, scale)));
		}

		/** Transforms the nodes using newMatrix from their initial positions.
		 * The "oldMatrix" variable can be left out in this function call (only for this graph generator)
		 * since the information can be taken from other saved data, which gives better precision.
		 */
		public override void RelocateNodes (Matrix4x4 oldMatrix, Matrix4x4 newMatrix) {
			if (vertices == null || vertices.Length == 0 || originalVertices == null || originalVertices.Length != vertices.Length) {
				return;
			}

			for (int i = 0; i < _vertices.Length; i++) {
				_vertices[i] = (Int3)newMatrix.MultiplyPoint3x4(originalVertices[i]);
			}

			for (int i = 0; i < nodes.Length; i++) {
				var node = nodes[i];
				node.UpdatePositionFromVertices();

				if (node.connections != null) {
					for (int q = 0; q < node.connections.Length; q++) {
						node.connectionCosts[q] = (uint)(node.position-node.connections[q].position).costMagnitude;
					}
				}
			}

			SetMatrix(newMatrix);

			RebuildBBTree(this);
		}

		public static NNInfo GetNearest (NavMeshGraph graph, GraphNode[] nodes, Vector3 position, NNConstraint constraint, bool accurateNearestNode) {
			if (nodes == null || nodes.Length == 0) {
				Debug.LogError("NavGraph hasn't been generated yet or does not contain any nodes");
				return new NNInfo();
			}

			if (constraint == null) constraint = NNConstraint.None;


			return GetNearestForceBoth(graph, graph, position, NNConstraint.None, accurateNearestNode);
		}

		public override NNInfo GetNearest (Vector3 position, NNConstraint constraint, GraphNode hint) {
			return GetNearest(this, nodes, position, constraint, accurateNearestNode);
		}

		/** This performs a linear search through all polygons returning the closest one.
		 * This is usually only called in the Free version of the A* Pathfinding Project since the Pro one supports BBTrees and will do another query
		 */
		public override NNInfo GetNearestForce (Vector3 position, NNConstraint constraint) {
			return GetNearestForce(this, this, position, constraint, accurateNearestNode);
			//Debug.LogWarning ("This function shouldn't be called since constrained nodes are sent back in the GetNearest call");

			//return new NNInfo ();
		}

		/** This performs a linear search through all polygons returning the closest one */
		public static NNInfo GetNearestForce (NavGraph graph, INavmeshHolder navmesh, Vector3 position, NNConstraint constraint, bool accurateNearestNode) {
			NNInfo nn = GetNearestForceBoth(graph, navmesh, position, constraint, accurateNearestNode);

			nn.node = nn.constrainedNode;
			nn.clampedPosition = nn.constClampedPosition;
			return nn;
		}

		/** This performs a linear search through all polygons returning the closest one.
		 * This will fill the NNInfo with .node for the closest node not necessarily complying with the NNConstraint, and .constrainedNode with the closest node
		 * complying with the NNConstraint.
		 * \see GetNearestForce(Node[],Int3[],Vector3,NNConstraint,bool)
		 */
		public static NNInfo GetNearestForceBoth (NavGraph graph, INavmeshHolder navmesh, Vector3 position, NNConstraint constraint, bool accurateNearestNode) {
			var pos = (Int3)position;

			float minDist = -1;
			GraphNode minNode = null;

			float minConstDist = -1;
			GraphNode minConstNode = null;

			float maxDistSqr = constraint.constrainDistance ? AstarPath.active.maxNearestNodeDistanceSqr : float.PositiveInfinity;

			GraphNodeDelegateCancelable del = delegate(GraphNode _node) {
				var node = _node as TriangleMeshNode;

				if (accurateNearestNode) {
					Vector3 closest = node.ClosestPointOnNode(position);
					float dist = ((Vector3)pos-closest).sqrMagnitude;

					if (minNode == null || dist < minDist) {
						minDist = dist;
						minNode = node;
					}

					if (dist < maxDistSqr && constraint.Suitable(node)) {
						if (minConstNode == null || dist < minConstDist) {
							minConstDist = dist;
							minConstNode = node;
						}
					}
				} else {
					if (!node.ContainsPoint((Int3)position)) {
						float dist = (node.position-pos).sqrMagnitude;
						if (minNode == null || dist < minDist) {
							minDist = dist;
							minNode = node;
						}

						if (dist < maxDistSqr && constraint.Suitable(node)) {
							if (minConstNode == null || dist < minConstDist) {
								minConstDist = dist;
								minConstNode = node;
							}
						}
					} else {
						int dist = System.Math.Abs(node.position.y-pos.y);

						if (minNode == null || dist < minDist) {
							minDist = dist;
							minNode = node;
						}

						if (dist < maxDistSqr && constraint.Suitable(node)) {
							if (minConstNode == null || dist < minConstDist) {
								minConstDist = dist;
								minConstNode = node;
							}
						}
					}
				}
				return true;
			};

			graph.GetNodes(del);

			var nninfo = new NNInfo(minNode);

			//Find the point closest to the nearest triangle

			if (nninfo.node != null) {
				var node = nninfo.node as TriangleMeshNode;//minNode2 as MeshNode;

				Vector3 clP = node.ClosestPointOnNode(position);

				nninfo.clampedPosition = clP;
			}

			nninfo.constrainedNode = minConstNode;
			if (nninfo.constrainedNode != null) {
				var node = nninfo.constrainedNode as TriangleMeshNode;//minNode2 as MeshNode;

				Vector3 clP = node.ClosestPointOnNode(position);

				nninfo.constClampedPosition = clP;
			}

			return nninfo;
		}


		public GraphUpdateThreading CanUpdateAsync (GraphUpdateObject o) {
			return GraphUpdateThreading.UnityThread;
		}

		public void UpdateAreaInit (GraphUpdateObject o) {}

		public void UpdateArea (GraphUpdateObject o) {
			UpdateArea(o, this);
		}

		public static void UpdateArea (GraphUpdateObject o, INavmesh graph) {
			Bounds bounds = o.bounds;

			// Bounding rectangle with floating point coordinates
			Rect r = Rect.MinMaxRect(bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z);

			// Bounding rectangle with int coordinates
			var r2 = new IntRect(
				Mathf.FloorToInt(bounds.min.x*Int3.Precision),
				Mathf.FloorToInt(bounds.min.z*Int3.Precision),
				Mathf.FloorToInt(bounds.max.x*Int3.Precision),
				Mathf.FloorToInt(bounds.max.z*Int3.Precision)
				);

			// Corners of the bounding rectangle
			var a = new Int3(r2.xmin, 0, r2.ymin);
			var b = new Int3(r2.xmin, 0, r2.ymax);
			var c = new Int3(r2.xmax, 0, r2.ymin);
			var d = new Int3(r2.xmax, 0, r2.ymax);

			var ymin = ((Int3)bounds.min).y;
			var ymax = ((Int3)bounds.max).y;

			// Loop through all nodes
			graph.GetNodes(_node => {
				var node = _node as TriangleMeshNode;

				bool inside = false;

				int allLeft = 0;
				int allRight = 0;
				int allTop = 0;
				int allBottom = 0;

				// Check bounding box rect in XZ plane
				for (int v = 0; v < 3; v++) {
					Int3 p = node.GetVertex(v);
					var vert = (Vector3)p;

					if (r2.Contains(p.x, p.z)) {
						inside = true;
						break;
					}

					if (vert.x < r.xMin) allLeft++;
					if (vert.x > r.xMax) allRight++;
					if (vert.z < r.yMin) allTop++;
					if (vert.z > r.yMax) allBottom++;
				}

				if (!inside) {
					if (allLeft == 3 || allRight == 3 || allTop == 3 || allBottom == 3) {
						return true;
					}
				}

				// Check if the polygon edges intersect the bounding rect
				for (int v = 0; v < 3; v++) {
					int v2 = v > 1 ? 0 : v+1;

					Int3 vert1 = node.GetVertex(v);
					Int3 vert2 = node.GetVertex(v2);

					if (VectorMath.SegmentsIntersectXZ(a, b, vert1, vert2)) { inside = true; break; }
					if (VectorMath.SegmentsIntersectXZ(a, c, vert1, vert2)) { inside = true; break; }
					if (VectorMath.SegmentsIntersectXZ(c, d, vert1, vert2)) { inside = true; break; }
					if (VectorMath.SegmentsIntersectXZ(d, b, vert1, vert2)) { inside = true; break; }
				}

				// Check if the node contains any corner of the bounding rect
				if (inside || node.ContainsPoint(a) || node.ContainsPoint(b) || node.ContainsPoint(c) || node.ContainsPoint(d)) {
					inside = true;
				}

				if (!inside) {
					return true;
				}

				int allAbove = 0;
				int allBelow = 0;

				// Check y coordinate
				for (int v = 0; v < 3; v++) {
					Int3 p = node.GetVertex(v);
					if (p.y < ymin) allBelow++;
					if (p.y > ymax) allAbove++;
				}

				// Polygon is either completely above the bounding box or completely below it
				if (allBelow == 3 || allAbove == 3) return true;

				// Triangle is inside the bounding box!
				// Update it!
				o.WillUpdateNode(node);
				o.Apply(node);
				return true;
			});
		}

		/** Returns the closest point of the node.
		 * The only reason this is here is because it is slightly faster compared to TriangleMeshNode.ClosestPointOnNode
		 * since it doesn't involve so many indirections.
		 *
		 * Use TriangleMeshNode.ClosestPointOnNode in most other cases.
		 */
		static Vector3 ClosestPointOnNode (TriangleMeshNode node, Int3[] vertices, Vector3 pos) {
			return Polygon.ClosestPointOnTriangle((Vector3)vertices[node.v0], (Vector3)vertices[node.v1], (Vector3)vertices[node.v2], pos);
		}

		/** Returns if the point is inside the node in XZ space */
		[System.Obsolete("Use TriangleMeshNode.ContainsPoint instead")]
		public bool ContainsPoint (TriangleMeshNode node, Vector3 pos) {
			if (VectorMath.IsClockwiseXZ((Vector3)vertices[node.v0], (Vector3)vertices[node.v1], pos)
				&& VectorMath.IsClockwiseXZ((Vector3)vertices[node.v1], (Vector3)vertices[node.v2], pos)
				&& VectorMath.IsClockwiseXZ((Vector3)vertices[node.v2], (Vector3)vertices[node.v0], pos)) {
				return true;
			}
			return false;
		}

		/** Returns if the point is inside the node in XZ space */
		[System.Obsolete("Use TriangleMeshNode.ContainsPoint instead")]
		public static bool ContainsPoint (TriangleMeshNode node, Vector3 pos, Int3[] vertices) {
			if (!VectorMath.IsClockwiseMarginXZ((Vector3)vertices[node.v0], (Vector3)vertices[node.v1], (Vector3)vertices[node.v2])) {
				Debug.LogError("Noes!");
			}

			if (VectorMath.IsClockwiseMarginXZ((Vector3)vertices[node.v0], (Vector3)vertices[node.v1], pos)
				&& VectorMath.IsClockwiseMarginXZ((Vector3)vertices[node.v1], (Vector3)vertices[node.v2], pos)
				&& VectorMath.IsClockwiseMarginXZ((Vector3)vertices[node.v2], (Vector3)vertices[node.v0], pos)) {
				return true;
			}
			return false;
		}

		/** Scans the graph using the path to an .obj mesh */
		public void ScanInternal (string objMeshPath) {
			Mesh mesh = ObjImporter.ImportFile(objMeshPath);

			if (mesh == null) {
				Debug.LogError("Couldn't read .obj file at '"+objMeshPath+"'");
				return;
			}

			sourceMesh = mesh;
			ScanInternal();
		}

		public override void ScanInternal (OnScanStatus statusCallback) {
			if (sourceMesh == null) {
				return;
			}

			GenerateMatrix();

			Vector3[] vectorVertices = sourceMesh.vertices;

			triangles = sourceMesh.triangles;

			TriangleMeshNode.SetNavmeshHolder(active.astarData.GetGraphIndex(this), this);
			GenerateNodes(vectorVertices, triangles, out originalVertices, out _vertices);
		}

		/** Generates a navmesh. Based on the supplied vertices and triangles */
		void GenerateNodes (Vector3[] vectorVertices, int[] triangles, out Vector3[] originalVertices, out Int3[] vertices) {
			Profiler.BeginSample("Init");

			if (vectorVertices.Length == 0 || triangles.Length == 0) {
				originalVertices = vectorVertices;
				vertices = new Int3[0];
				nodes = new TriangleMeshNode[0];
				return;
			}

			vertices = new Int3[vectorVertices.Length];

			int c = 0;

			for (int i = 0; i < vertices.Length; i++) {
				vertices[i] = (Int3)matrix.MultiplyPoint3x4(vectorVertices[i]);
			}

			var hashedVerts = new Dictionary<Int3, int>();

			var newVertices = new int[vertices.Length];

			Profiler.EndSample();
			Profiler.BeginSample("Hashing");

			for (int i = 0; i < vertices.Length; i++) {
				if (!hashedVerts.ContainsKey(vertices[i])) {
					newVertices[c] = i;
					hashedVerts.Add(vertices[i], c);
					c++;
				}
			}

			for (int x = 0; x < triangles.Length; x++) {
				Int3 vertex = vertices[triangles[x]];

				triangles[x] = hashedVerts[vertex];
			}

			Int3[] totalIntVertices = vertices;
			vertices = new Int3[c];
			originalVertices = new Vector3[c];
			for (int i = 0; i < c; i++) {
				vertices[i] = totalIntVertices[newVertices[i]];
				originalVertices[i] = vectorVertices[newVertices[i]];
			}

			Profiler.EndSample();
			Profiler.BeginSample("Constructing Nodes");

			nodes = new TriangleMeshNode[triangles.Length/3];

			int graphIndex = active.astarData.GetGraphIndex(this);

			// Does not have to set this, it is set in ScanInternal
			//TriangleMeshNode.SetNavmeshHolder ((int)graphIndex,this);

			for (int i = 0; i < nodes.Length; i++) {
				nodes[i] = new TriangleMeshNode(active);
				TriangleMeshNode node = nodes[i];//new MeshNode ();

				node.GraphIndex = (uint)graphIndex;
				node.Penalty = initialPenalty;
				node.Walkable = true;


				node.v0 = triangles[i*3];
				node.v1 = triangles[i*3+1];
				node.v2 = triangles[i*3+2];

				if (!VectorMath.IsClockwiseXZ(vertices[node.v0], vertices[node.v1], vertices[node.v2])) {
					//Debug.DrawLine (vertices[node.v0],vertices[node.v1],Color.red);
					//Debug.DrawLine (vertices[node.v1],vertices[node.v2],Color.red);
					//Debug.DrawLine (vertices[node.v2],vertices[node.v0],Color.red);

					int tmp = node.v0;
					node.v0 = node.v2;
					node.v2 = tmp;
				}

				if (VectorMath.IsColinearXZ(vertices[node.v0], vertices[node.v1], vertices[node.v2])) {
					Debug.DrawLine((Vector3)vertices[node.v0], (Vector3)vertices[node.v1], Color.red);
					Debug.DrawLine((Vector3)vertices[node.v1], (Vector3)vertices[node.v2], Color.red);
					Debug.DrawLine((Vector3)vertices[node.v2], (Vector3)vertices[node.v0], Color.red);
				}

				// Make sure position is correctly set
				node.UpdatePositionFromVertices();
			}

			Profiler.EndSample();

			var sides = new Dictionary<Int2, TriangleMeshNode>();

			for (int i = 0, j = 0; i < triangles.Length; j += 1, i += 3) {
				sides[new Int2(triangles[i+0], triangles[i+1])] = nodes[j];
				sides[new Int2(triangles[i+1], triangles[i+2])] = nodes[j];
				sides[new Int2(triangles[i+2], triangles[i+0])] = nodes[j];
			}

			Profiler.BeginSample("Connecting Nodes");

			var connections = new List<MeshNode>();
			var connectionCosts = new List<uint>();

			for (int i = 0, j = 0; i < triangles.Length; j += 1, i += 3) {
				connections.Clear();
				connectionCosts.Clear();

				TriangleMeshNode node = nodes[j];

				for (int q = 0; q < 3; q++) {
					TriangleMeshNode other;
					if (sides.TryGetValue(new Int2(triangles[i+((q+1)%3)], triangles[i+q]), out other)) {
						connections.Add(other);
						connectionCosts.Add((uint)(node.position-other.position).costMagnitude);
					}
				}

				node.connections = connections.ToArray();
				node.connectionCosts = connectionCosts.ToArray();
			}

			Profiler.EndSample();
			Profiler.BeginSample("Rebuilding BBTree");

			RebuildBBTree(this);

			Profiler.EndSample();
		}

		/** Rebuilds the BBTree on a NavGraph.
		 * \astarpro
		 * \see NavMeshGraph.bbTree */
		public static void RebuildBBTree (NavMeshGraph graph) {
			// BBTree is an A* Pathfinding Project Pro only feature - The Pro version can be bought in the Unity Asset Store or on arongranberg.com
		}

		public void PostProcess () {
		}

		public override void OnDrawGizmos (bool drawNodes) {
			if (!drawNodes) {
				return;
			}

			Matrix4x4 preMatrix = matrix;

			GenerateMatrix();

			if (nodes == null) {
				//Scan ();
			}

			if (nodes == null) {
				return;
			}

			if (preMatrix != matrix) {
				//Debug.Log ("Relocating Nodes");
				RelocateNodes(preMatrix, matrix);
			}

			PathHandler debugData = AstarPath.active.debugPathData;
			for (int i = 0; i < nodes.Length; i++) {
				var node = nodes[i];

				Gizmos.color = NodeColor(node, AstarPath.active.debugPathData);

				if (node.Walkable) {
					if (AstarPath.active.showSearchTree && debugData != null && debugData.GetPathNode(node).parent != null) {
						Gizmos.DrawLine((Vector3)node.position, (Vector3)debugData.GetPathNode(node).parent.node.position);
					} else {
						for (int q = 0; q < node.connections.Length; q++) {
							Gizmos.DrawLine((Vector3)node.position, Vector3.Lerp((Vector3)node.position, (Vector3)node.connections[q].position, 0.45f));
						}
					}

					Gizmos.color = AstarColor.MeshEdgeColor;
				} else {
					Gizmos.color = AstarColor.UnwalkableNode;
				}
				Gizmos.DrawLine((Vector3)vertices[node.v0], (Vector3)vertices[node.v1]);
				Gizmos.DrawLine((Vector3)vertices[node.v1], (Vector3)vertices[node.v2]);
				Gizmos.DrawLine((Vector3)vertices[node.v2], (Vector3)vertices[node.v0]);
			}
		}

		public override void DeserializeExtraInfo (GraphSerializationContext ctx) {
			uint graphIndex = ctx.graphIndex;

			TriangleMeshNode.SetNavmeshHolder((int)graphIndex, this);

			int nodeCount = ctx.reader.ReadInt32();
			int vertexCount = ctx.reader.ReadInt32();

			if (nodeCount == -1) {
				nodes = new TriangleMeshNode[0];
				_vertices = new Int3[0];
				originalVertices = new Vector3[0];
				return;
			}

			nodes = new TriangleMeshNode[nodeCount];
			_vertices = new Int3[vertexCount];
			originalVertices = new Vector3[vertexCount];

			for (int i = 0; i < vertexCount; i++) {
				_vertices[i] = ctx.DeserializeInt3();
				originalVertices[i] = ctx.DeserializeVector3();
			}


			for (int i = 0; i < nodeCount; i++) {
				nodes[i] = new TriangleMeshNode(active);
				TriangleMeshNode node = nodes[i];
				node.DeserializeNode(ctx);
				node.UpdatePositionFromVertices();
			}
		}

		public override void SerializeExtraInfo (GraphSerializationContext ctx) {
			if (nodes == null || originalVertices == null || _vertices == null || originalVertices.Length != _vertices.Length) {
				ctx.writer.Write(-1);
				ctx.writer.Write(-1);
				return;
			}
			ctx.writer.Write(nodes.Length);
			ctx.writer.Write(_vertices.Length);

			for (int i = 0; i < _vertices.Length; i++) {
				ctx.SerializeInt3(_vertices[i]);
				ctx.SerializeVector3(originalVertices[i]);
			}

			for (int i = 0; i < nodes.Length; i++) {
				nodes[i].SerializeNode(ctx);
			}
		}

		public override void DeserializeSettingsCompatibility (GraphSerializationContext ctx) {
			base.DeserializeSettingsCompatibility(ctx);

			sourceMesh = ctx.DeserializeUnityObject() as Mesh;

			offset = ctx.DeserializeVector3();
			rotation = ctx.DeserializeVector3();
			scale = ctx.reader.ReadSingle();
			accurateNearestNode = ctx.reader.ReadBoolean();
		}
	}
}
