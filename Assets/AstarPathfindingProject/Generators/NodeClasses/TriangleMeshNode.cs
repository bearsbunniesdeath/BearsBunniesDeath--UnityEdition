using UnityEngine;
using Pathfinding.Serialization;

namespace Pathfinding {
	public interface INavmeshHolder {
		Int3 GetVertex (int i);
		int GetVertexArrayIndex (int index);
		void GetTileCoordinates (int tileIndex, out int x, out int z);
	}

	/** Node represented by a triangle */
	public class TriangleMeshNode : MeshNode {
		public TriangleMeshNode (AstarPath astar) : base(astar) {}

		/** Internal vertex index for the first vertex */
		public int v0;

		/** Internal vertex index for the second vertex */
		public int v1;

		/** Internal vertex index for the third vertex */
		public int v2;

		protected static INavmeshHolder[] _navmeshHolders = new INavmeshHolder[0];
		public static INavmeshHolder GetNavmeshHolder (uint graphIndex) {
			return _navmeshHolders[(int)graphIndex];
		}

		/** Sets the internal navmesh holder for a given graph index.
		 * \warning Internal method
		 */
		public static void SetNavmeshHolder (int graphIndex, INavmeshHolder graph) {
			if (_navmeshHolders.Length <= graphIndex) {
				var gg = new INavmeshHolder[graphIndex+1];
				for (int i = 0; i < _navmeshHolders.Length; i++) gg[i] = _navmeshHolders[i];
				_navmeshHolders = gg;
			}
			_navmeshHolders[graphIndex] = graph;
		}

		/** Set the position of this node to the average of its 3 vertices */
		public void UpdatePositionFromVertices () {
			INavmeshHolder g = GetNavmeshHolder(GraphIndex);

			position = (g.GetVertex(v0) + g.GetVertex(v1) + g.GetVertex(v2)) * 0.333333f;
		}

		/** Return a number identifying a vertex.
		 * This number does not necessarily need to be a index in an array but two different vertices (in the same graph) should
		 * not have the same vertex numbers.
		 */
		public int GetVertexIndex (int i) {
			return i == 0 ? v0 : (i == 1 ? v1 : v2);
		}

		/** Return a number specifying an index in the source vertex array.
		 * The vertex array can for example be contained in a recast tile, or be a navmesh graph, that is graph dependant.
		 * This is slower than GetVertexIndex, if you only need to compare vertices, use GetVertexIndex.
		 */
		public int GetVertexArrayIndex (int i) {
			return GetNavmeshHolder(GraphIndex).GetVertexArrayIndex(i == 0 ? v0 : (i == 1 ? v1 : v2));
		}

		public override Int3 GetVertex (int i) {
			return GetNavmeshHolder(GraphIndex).GetVertex(GetVertexIndex(i));
		}

		public override int GetVertexCount () {
			// A triangle has 3 vertices
			return 3;
		}

		public override Vector3 ClosestPointOnNode (Vector3 p) {
			INavmeshHolder g = GetNavmeshHolder(GraphIndex);

			return Pathfinding.Polygon.ClosestPointOnTriangle((Vector3)g.GetVertex(v0), (Vector3)g.GetVertex(v1), (Vector3)g.GetVertex(v2), p);
		}

		public override Vector3 ClosestPointOnNodeXZ (Vector3 p) {
			// Get the object holding the vertex data for this node
			// This is usually a graph or a recast graph tile
			INavmeshHolder g = GetNavmeshHolder(GraphIndex);

			// Get all 3 vertices for this node
			Int3 tp1 = g.GetVertex(v0);
			Int3 tp2 = g.GetVertex(v1);
			Int3 tp3 = g.GetVertex(v2);

			Vector2 closest = Polygon.ClosestPointOnTriangle(
				new Vector2(tp1.x*Int3.PrecisionFactor, tp1.z*Int3.PrecisionFactor),
				new Vector2(tp2.x*Int3.PrecisionFactor, tp2.z*Int3.PrecisionFactor),
				new Vector2(tp3.x*Int3.PrecisionFactor, tp3.z*Int3.PrecisionFactor),
				new Vector2(p.x, p.z)
				);

			return new Vector3(closest.x, p.y, closest.y);
		}

		public override bool ContainsPoint (Int3 p) {
			// Get the object holding the vertex data for this node
			// This is usually a graph or a recast graph tile
			INavmeshHolder navmeshHolder = GetNavmeshHolder(GraphIndex);

			// Get all 3 vertices for this node
			Int3 a = navmeshHolder.GetVertex(v0);
			Int3 b = navmeshHolder.GetVertex(v1);
			Int3 c = navmeshHolder.GetVertex(v2);

			if ((long)(b.x - a.x) * (long)(p.z - a.z) - (long)(p.x - a.x) * (long)(b.z - a.z) > 0) return false;

			if ((long)(c.x - b.x) * (long)(p.z - b.z) - (long)(p.x - b.x) * (long)(c.z - b.z) > 0) return false;

			if ((long)(a.x - c.x) * (long)(p.z - c.z) - (long)(p.x - c.x) * (long)(a.z - c.z) > 0) return false;

			return true;
			// Equivalent code, but the above code is faster
			//return Polygon.IsClockwiseMargin (a,b, p) && Polygon.IsClockwiseMargin (b,c, p) && Polygon.IsClockwiseMargin (c,a, p);

			//return Polygon.ContainsPoint(g.GetVertex(v0),g.GetVertex(v1),g.GetVertex(v2),p);
		}

		public override void UpdateRecursiveG (Path path, PathNode pathNode, PathHandler handler) {
			UpdateG(path, pathNode);

			handler.PushNode(pathNode);

			if (connections == null) return;

			for (int i = 0; i < connections.Length; i++) {
				GraphNode other = connections[i];
				PathNode otherPN = handler.GetPathNode(other);
				if (otherPN.parent == pathNode && otherPN.pathID == handler.PathID) other.UpdateRecursiveG(path, otherPN, handler);
			}
		}

		public override void Open (Path path, PathNode pathNode, PathHandler handler) {
			if (connections == null) return;

			// Flag2 indicates if this node needs special treatment
			// with regard to connection costs
			bool flag2 = pathNode.flag2;

			// Loop through all connections
			for (int i = connections.Length-1; i >= 0; i--) {
				GraphNode other = connections[i];

				// Make sure we can traverse the neighbour
				if (path.CanTraverse(other)) {
					PathNode pathOther = handler.GetPathNode(other);

					// Fast path out, worth it for triangle mesh nodes since they usually have degree 2 or 3
					if (pathOther == pathNode.parent) {
						continue;
					}

					uint cost = connectionCosts[i];

					if (flag2 || pathOther.flag2) {
						// Get special connection cost from the path
						// This is used by the start and end nodes
						cost = path.GetConnectionSpecialCost(this, other, cost);
					}

					// Test if we have seen the other node before
					if (pathOther.pathID != handler.PathID) {
						// We have not seen the other node before
						// So the path from the start through this node to the other node
						// must be the shortest one so far

						// Might not be assigned
						pathOther.node = other;

						pathOther.parent = pathNode;
						pathOther.pathID = handler.PathID;

						pathOther.cost = cost;

						pathOther.H = path.CalculateHScore(other);
						other.UpdateG(path, pathOther);

						handler.PushNode(pathOther);
					} else {
						// If not we can test if the path from this node to the other one is a better one than the one already used
						if (pathNode.G + cost + path.GetTraversalCost(other) < pathOther.G) {
							pathOther.cost = cost;
							pathOther.parent = pathNode;

							other.UpdateRecursiveG(path, pathOther, handler);
						} else if (pathOther.G+cost+path.GetTraversalCost(this) < pathNode.G && other.ContainsConnection(this)) {
							// Or if the path from the other node to this one is better

							pathNode.parent = pathOther;
							pathNode.cost = cost;

							UpdateRecursiveG(path, pathNode, handler);
						}
					}
				}
			}
		}

		/** Returns the edge which is shared with \a other.
		 * If no edge is shared, -1 is returned.
		 * The edge is GetVertex(result) - GetVertex((result+1) % GetVertexCount()).
		 * See GetPortal for the exact segment shared.
		 * \note Might return that an edge is shared when the two nodes are in different tiles and adjacent on the XZ plane, but on the Y-axis.
		 * Therefore it is recommended that you only test for neighbours of this node or do additional checking afterwards.
		 */
		public int SharedEdge (GraphNode other) {
			int a, b;

			GetPortal(other, null, null, false, out a, out b);
			return a;
		}

		public override bool GetPortal (GraphNode _other, System.Collections.Generic.List<Vector3> left, System.Collections.Generic.List<Vector3> right, bool backwards) {
			int aIndex, bIndex;

			return GetPortal(_other, left, right, backwards, out aIndex, out bIndex);
		}

		public bool GetPortal (GraphNode _other, System.Collections.Generic.List<Vector3> left, System.Collections.Generic.List<Vector3> right, bool backwards, out int aIndex, out int bIndex) {
			aIndex = -1;
			bIndex = -1;

			//If the nodes are in different graphs, this function has no idea on how to find a shared edge.
			if (_other.GraphIndex != GraphIndex) return false;

			// Since the nodes are in the same graph, they are both TriangleMeshNodes
			// So we don't need to care about other types of nodes
			var other = _other as TriangleMeshNode;

			if (!backwards) {
				int first = -1;
				int second = -1;

				int av = GetVertexCount();
				int bv = other.GetVertexCount();

				/** \todo Maybe optimize with pa=av-1 instead of modulus... */
				for (int a = 0; a < av; a++) {
					int va = GetVertexIndex(a);
					for (int b = 0; b < bv; b++) {
						if (va == other.GetVertexIndex((b+1)%bv) && GetVertexIndex((a+1) % av) == other.GetVertexIndex(b)) {
							first = a;
							second = b;
							a = av;
							break;
						}
					}
				}

				aIndex = first;
				bIndex = second;

				if (first != -1) {
					if (left != null) {
						//All triangles should be clockwise so second is the rightmost vertex (seen from this node)
						left.Add((Vector3)GetVertex(first));
						right.Add((Vector3)GetVertex((first+1)%av));
					}
				} else {
					for (int i = 0; i < connections.Length; i++) {
						if (connections[i].GraphIndex != GraphIndex) {
							var mid = connections[i] as NodeLink3Node;
							if (mid != null && mid.GetOther(this) == other) {
								// We have found a node which is connected through a NodeLink3Node

								if (left != null) {
									mid.GetPortal(other, left, right, false);
									return true;
								}
							}
						}
					}
					return false;
				}
			}

			return true;
		}

		public override void SerializeNode (GraphSerializationContext ctx) {
			base.SerializeNode(ctx);
			ctx.writer.Write(v0);
			ctx.writer.Write(v1);
			ctx.writer.Write(v2);
		}

		public override void DeserializeNode (GraphSerializationContext ctx) {
			base.DeserializeNode(ctx);
			v0 = ctx.reader.ReadInt32();
			v1 = ctx.reader.ReadInt32();
			v2 = ctx.reader.ReadInt32();
		}
	}
}
