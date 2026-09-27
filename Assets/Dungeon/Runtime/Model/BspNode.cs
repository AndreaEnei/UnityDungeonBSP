using System;
using UnityEngine;

namespace Tesi.Dungeon
{
    /// <summary>Rappresenta una regione nell'albero BSP.</summary>
    public sealed class BspNode
    {
        /// <summary>Regione rettangolare occupata dal nodo.</summary>
        public RectInt Bounds { get; }

        /// <summary>Livello del nodo nell'albero; la radice ha profondita 0.</summary>
        public int Depth { get; }

        /// <summary>Figlio sinistro o inferiore, in base al tipo di taglio.</summary>
        public BspNode Left { get; private set; }

        /// <summary>Figlio destro o superiore, in base al tipo di taglio.</summary>
        public BspNode Right { get; private set; }

        /// <summary>Orientamento del taglio; null se il nodo non è stato diviso.</summary>
        public SplitOrientation? Split { get; private set; }

        /// <summary>Stanza assegnata alla foglia. Null finchè non è stata creata.</summary>
        public RectInt? Room { get; private set; }

        /// <summary>Indica se il nodo non possiede figli.</summary>
        public bool IsLeaf => Left == null && Right == null;

        public BspNode(RectInt bounds, int depth)
        {
            Bounds = bounds;
            Depth = depth;
        }

        public void SetChildren(
            BspNode left,
            BspNode right,
            SplitOrientation split)
        {
            if (left == null)
            {
                throw new ArgumentNullException(nameof(left));
            }

            if (right == null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            if (!IsLeaf)
            {
                throw new InvalidOperationException(
                    "A BSP node can only be split once."
                );
            }

            Left = left;
            Right = right;
            Split = split;
        }

        public void SetRoom(RectInt room)
        {
            if (!IsLeaf)
            {
                throw new InvalidOperationException(
                    "A room can only be assigned to a BSP leaf."
                );
            }

            if (Room.HasValue)
            {
                throw new InvalidOperationException(
                    "A BSP leaf can only receive one room."
                );
            }

            if (room.width <= 0 || room.height <= 0)
            {
                throw new ArgumentException(
                    "A room must have positive dimensions.",
                    nameof(room)
                );
            }

            if (room.xMin < Bounds.xMin ||
                room.yMin < Bounds.yMin ||
                room.xMax > Bounds.xMax ||
                room.yMax > Bounds.yMax)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(room),
                    "A room must be contained inside its BSP leaf."
                );
            }

            Room = room;
        }
    }
}