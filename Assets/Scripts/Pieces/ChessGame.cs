using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pieces
{
    public readonly struct PieceType
    {
        public static readonly PieceType Queen = new PieceType("Queen", new Rect(75, 0, 44, 52),
            new PieceLegalMovements(8, EDirections.Down, EDirections.Left, EDirections.Right, EDirections.Up,
                EDirections.LeftBtmCorner, EDirections.LeftTopCorner, EDirections.RightBtmCorner,
                EDirections.RightTopCorner));

        public static readonly PieceType King = new PieceType("King", new Rect(0, 0, 55, 59),
            new PieceLegalMovements(1, EDirections.Down, EDirections.Left, EDirections.Right, EDirections.Up,
                EDirections.LeftBtmCorner, EDirections.LeftTopCorner, EDirections.RightBtmCorner,
                EDirections.RightTopCorner));

        public static readonly PieceType Bishop = new PieceType("Bishop", new Rect(222, 0, 47, 51),
            new PieceLegalMovements(8, EDirections.LeftBtmCorner, EDirections.LeftTopCorner, EDirections.RightBtmCorner,
                EDirections.RightTopCorner));

        public static readonly PieceType Knight =
            new PieceType("Knight", new Rect(298, 0, 42, 52), new PieceLegalMovements(4));

        public static readonly PieceType Rook = new PieceType("Rook", new Rect(151, 0, 40, 52),
            new PieceLegalMovements(8, EDirections.Up, EDirections.Down, EDirections.Left, EDirections.Right));

        public static readonly PieceType Pawn = new PieceType("Pawn", new Rect(374, 0, 38, 51), new PieceLegalMovements(
            1,
            (movement, chess) => true,
            (movement, chess) =>
            {
                List<IMovement> list = new List<IMovement>();

                if (chess.MovesAmount == 0)
                {
                    var currentCell = movement.GetCurrentCell();
                    if (currentCell.Item2 == 1)
                    {
                        list.Add(new PieceMovement(Pawn, (currentCell.Item1, 3), currentCell));
                    }
                    else if (currentCell.Item2 == 6)
                    {
                        list.Add(new PieceMovement(Pawn, (currentCell.Item1, 4), currentCell));
                    }

                    return Check();
                }

                return Check();

                List<IMovement> Check()
                {
                    foreach (var foodCord in PossibleEats())
                    {
                        if (chess.CanEat(foodCord))
                        {
                            list.Add(new Movement(foodCord, movement.GetCurrentCell()));
                        }
                    }

                    if (chess.White)
                    {
                        list.Add(Movement.CreateDirectionalMovement(EDirections.Up,
                            movement.GetCurrentCell()));
                    }
                    else
                    {
                        list.Add(Movement.CreateDirectionalMovement(EDirections.Down,
                            movement.GetCurrentCell()));
                    }

                    return list;
                }

                List<(int, int)> PossibleEats()
                {
                    var foods = new List<(int, int)>();
                    if (chess.White)
                    {
                        foods.Add((chess.Row+1, chess.Column-1));
                        foods.Add((chess.Row-1, chess.Column-1));
                    }
                    else
                    {
                        foods.Add((chess.Row+1, chess.Column+1));
                        foods.Add((chess.Row-1, chess.Column+1));
                    }
                    return foods;
                }
            }));

        public readonly Rect TextureRect;
        public readonly string Name;
        public readonly PieceLegalMovements PieceLegalMovements;

        private PieceType(string name, Rect textureRect, PieceLegalMovements pieceLegalMovements)
        {
            TextureRect = textureRect;
            Name = name;
            PieceLegalMovements = pieceLegalMovements;
        }
    }

    public readonly struct PieceLegalMovements
    {
        public readonly EDirections[] Directions;
        public readonly ushort Range;
        public readonly Func<IMovement, ChessPieceScript, bool> CustomMoveCondition;
        public readonly Func<IMovement, ChessPieceScript, List<IMovement>> CustomMovement;

        public PieceLegalMovements(ushort range, Func<IMovement, ChessPieceScript, bool> customMoveCondition,
            Func<IMovement, ChessPieceScript, List<IMovement>> customMovement, params EDirections[] directions)
        {
            Range = range;
            CustomMoveCondition = customMoveCondition;
            CustomMovement = customMovement;
            Directions = directions;
        }

        public PieceLegalMovements(ushort range, params EDirections[] directions)
        {
            Range = range;
            CustomMoveCondition = (var, var2) => false;
            CustomMovement = (var, var2) => null;
            Directions = directions;
        }
    }


    public enum EDirections
    {
        Up,
        Down,
        Left,
        Right,
        LeftTopCorner,
        RightTopCorner,
        LeftBtmCorner,
        RightBtmCorner,
        Invalid
    }
}