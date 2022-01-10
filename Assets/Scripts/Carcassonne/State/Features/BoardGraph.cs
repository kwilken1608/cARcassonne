using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using QuikGraph;
using UnityEngine;

namespace Carcassonne.State.Features
{
    // Type Aliases
    using CarcassonneEdge = TaggedUndirectedEdge<SubTile, ConnectionType>;

    public enum ConnectionType
    {
        Tile,
        Board,
        Feature
    }

    /// <summary>
    /// Event arguments for a BoardGraph.Changed event.
    ///
    /// graph: A reference to the graph in question
    /// vertices: New vertices added to the graph
    /// edges: New edges added to the graph
    /// </summary>
    public class BoardChangedEventArgs : EventArgs
    {
        public BoardGraph graph;
        public IEnumerable<SubTile> vertices;
        public IEnumerable<CarcassonneEdge> edges;
    }
    
    public class SubTile : IComparable<SubTile>
        {
            public TileScript tile;
            public Vector2Int location;
            public Geography geography;

            /// <summary>
            /// Create a new SubTile
            /// </summary>
            /// <param name="tile">Reference to the TileScript</param>
            /// <param name="tilePosition">Position of the tile in board matrix space</param>
            /// <param name="direction">Direction of the SubTile space. Valid arguments are Vector2Int.[up|down|left|right|zero]</param>
            /// <param name="meeple">Reference to MeepleScript, if Meeple is present.</param>
            public SubTile(TileScript tile, Vector2Int tilePosition, Vector2Int direction, [CanBeNull] MeepleScript meeple = null)
            {
                this.tile = tile;
                this.location = Coordinates.TileToSubTile(tilePosition, direction); // The centre of the tile is at the tile's position + [1,1] to leave room for the -1 movement.
                this.geography = tile.getGeographyAt(direction);
                // Debug.Log($"GEOGRAPHY 44: {tile} @ {direction} = {geography}");
            }

            public int CompareTo(SubTile other)
            {
                // Check to see if these are the same vertices
                if (location == other.location)
                {
                    if(tile == other.tile && geography == other.geography) return 0;
                
                    throw new ArgumentException(
                        $"Vertices have the same location ({location}), but have different tiles, geographies, or meeples." +
                        "Only one SubTile may occupy a given location.");
                }
            
                // If they are different, sort by hash code
                //TODO Test this to make sure it works consistently.
                return (tile.GetHashCode() - other.tile.GetHashCode()) > 0 ? 1 : -1;
            }

        }

    public class BoardGraph : CarcassonneGraph
    {
        public void Add(BoardGraph b)
        {
            IEnumerable<CarcassonneEdge> edges = b.Edges;
            IEnumerable<SubTile> vertices = b.Vertices;
            
            AddVerticesAndEdgeRange(b.Edges);
            
            // Search for adjacent vertices and add links
            foreach (var va in Vertices)
            {
                // Find all Subtiles that are adjacent to the vertex in graph a
                var toLink = b.Vertices.Where(vb => (va.location - vb.location).sqrMagnitude == 1 &&
                                                    va.tile != vb.tile);

                foreach (var subtile in toLink)
                {
                    Debug.Assert(va.geography == subtile.geography, $"Geographies ({va.geography}, {subtile.geography}) do not match.");

                    CarcassonneEdge e = EdgeBetween(va, subtile, ConnectionType.Board); 
                    AddEdge(e);
                    edges.Append(e);

                    if (va.geography == Geography.City || va.geography == Geography.Road)
                    {
                        CarcassonneEdge f = EdgeBetween(va, subtile, ConnectionType.Feature);
                        AddEdge(f);
                        edges.Append(f);
                    }
                }
            }

            Debug.Log("Firing a Board.Changed event.");
            // Fire board changed event.
            BoardChangedEventArgs args = new BoardChangedEventArgs();
            args.graph = this;
            args.edges = edges;
            args.vertices = vertices;
            OnChanged(args);
        }

        protected virtual void OnChanged(BoardChangedEventArgs args)
        {
            Changed?.Invoke(this, args);
        }

        /// <summary>
        /// Get a graph representation of the tile itself.
        /// </summary>
        /// <param name="tile"></param>
        /// <returns></returns>
        public static BoardGraph FromTile(TileScript tile, Vector2Int location)
        {
            BoardGraph g = new BoardGraph();
    
            foreach (var side in tile.Sides)
            {
                // Add connection to neighbouring subtile
                var direction = side.Key;
                var geography = side.Value;
                // Debug.Log($"GEOGRAPHY 140: {tile} @ {direction} = {geography}");
                g = AddAndConnectSubTile(tile, location, direction, g, geography);
            }
            
            // Add a centre vertex IF it is a cloister
            if (tile.Center == Geography.Cloister)
            {
                var direction = Vector2Int.zero;
                g = AddAndConnectSubTile(tile, location, direction, g, null);
                
                // Remove connections between NORTH/SOUTH and EAST/WEST. They are redundant now that the centre is connected.
                var edgesToRemove =
                    g.Edges.Where(e => (e.Source.location - e.Target.location).sqrMagnitude == 4);
                g.RemoveEdges(edgesToRemove);
            }

            return g;
        }

        private static BoardGraph AddAndConnectSubTile(TileScript tile, Vector2Int location, Vector2Int direction, BoardGraph g, Geography? geography)
        {
            SubTile st = new SubTile(tile, location, direction);

            g.AddVertex(st);
            foreach (var v in g.Vertices.Where(v => v != st))
            {
                // Connect to adjacent sides and centre
                if((v.location - st.location).sqrMagnitude < 4){
                    g.AddEdge(EdgeBetween(v, st, ConnectionType.Tile));
                }

                // If the vertices are of the same type (City or Road) AND they are connected by the centre, add a connection
                if (geography!= null && (geography == Geography.City || geography == Geography.Road)
                                     && geography == v.geography && (geography & tile.Center) == geography)
                {
                    g.AddEdge(EdgeBetween(v, st, ConnectionType.Feature));
                }
            }

            return g;
        }

        /// <summary>
        /// Automates the creation of undirected edges between two verticies.
        /// Undirected edges need to go from a lesser vertex to a greater one (via CompareTo), so this sorts the
        /// vertices and returns a valid edge. 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static CarcassonneEdge EdgeBetween(SubTile a, SubTile b, ConnectionType t)
        {
            if (a.CompareTo(b) > 0)
            {
                return new CarcassonneEdge(b, a, t);
            }

            return new CarcassonneEdge(a, b, t);
        }

        public event EventHandler<BoardChangedEventArgs> Changed;
    }
}