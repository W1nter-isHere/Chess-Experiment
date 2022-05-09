using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities;

namespace Utilities
{
    public readonly struct PieceTypes
    {
        public static readonly PieceTypes Queen = new PieceTypes("Queen", new Rect(200, 0, 200, 200),
            new PieceLegalMovements(8, EDirections.Down, EDirections.Left, EDirections.Right, EDirections.Up,
                EDirections.LeftBtmCorner, EDirections.LeftTopCorner, EDirections.RightBtmCorner,
                EDirections.RightTopCorner));

        public static readonly PieceTypes King = new PieceTypes("King", new Rect(0, 0, 200, 200),
            new PieceLegalMovements(1, EDirections.Down, EDirections.Left, EDirections.Right, EDirections.Up,
                EDirections.LeftBtmCorner, EDirections.LeftTopCorner, EDirections.RightBtmCorner,
                EDirections.RightTopCorner));

        public static readonly PieceTypes Bishop = new PieceTypes("Bishop", new Rect(400, 0, 200, 200),
            new PieceLegalMovements(8, EDirections.LeftBtmCorner, EDirections.LeftTopCorner, EDirections.RightBtmCorner,
                EDirections.RightTopCorner));

        public static readonly PieceTypes Knight =
            new PieceTypes("Knight", new Rect(600, 0, 200, 200), new PieceLegalMovements(4,
                    (movement, chess) => true,
                    (movement, chess) =>
                    {
                        var list = new List<IMovement>
                        {
                            new Movement(PositionHelper.MoveLeft(PositionHelper.MoveUp(PositionHelper.MoveUp(movement.GetCurrentCell()))), movement.GetCurrentCell()),
                            new Movement(PositionHelper.MoveRight(PositionHelper.MoveUp(PositionHelper.MoveUp(movement.GetCurrentCell()))), movement.GetCurrentCell()),
                            new Movement(PositionHelper.MoveUp(PositionHelper.MoveLeft(PositionHelper.MoveLeft(movement.GetCurrentCell()))), movement.GetCurrentCell()),
                            new Movement(PositionHelper.MoveDown(PositionHelper.MoveLeft(PositionHelper.MoveLeft(movement.GetCurrentCell()))), movement.GetCurrentCell()),
                            new Movement(PositionHelper.MoveDown(PositionHelper.MoveRight(PositionHelper.MoveRight(movement.GetCurrentCell()))), movement.GetCurrentCell()),
                            new Movement(PositionHelper.MoveUp(PositionHelper.MoveRight(PositionHelper.MoveRight(movement.GetCurrentCell()))), movement.GetCurrentCell()),
                            new Movement(PositionHelper.MoveLeft(PositionHelper.MoveDown(PositionHelper.MoveDown(movement.GetCurrentCell()))), movement.GetCurrentCell()),
                            new Movement(PositionHelper.MoveRight(PositionHelper.MoveDown(PositionHelper.MoveDown(movement.GetCurrentCell()))), movement.GetCurrentCell())
                        };

                        var movementsFiltered = new List<IMovement>();

                        foreach (var move in list)
                        {
                            try
                            {
                                var p = GameManagerScript.PiecesOnBoard[move.GetDestCell()];
                                if (chess.CanEat(p))
                                {
                                    movementsFiltered.Add(move);
                                }
                            }
                            catch (KeyNotFoundException e)
                            {
                                movementsFiltered.Add(move);
                            }
                        }
                        
                        return movementsFiltered;
                    }));

        public static readonly PieceTypes Rook = new PieceTypes("Rook", new Rect(800, 0, 200, 200),
            new PieceLegalMovements(8, EDirections.Up, EDirections.Down, EDirections.Left, EDirections.Right));

        public static readonly PieceTypes Pawn = new PieceTypes("Pawn", new Rect(1000, 0, 200, 200), new PieceLegalMovements(
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
                        try
                        {
                            var a = GameManagerScript.PiecesOnBoard[PositionHelper.MoveUp(movement.GetCurrentCell())];
                        }
                        catch (KeyNotFoundException e)
                        {
                            list.Add(Movement.CreateDirectionalMovement(EDirections.Up, movement.GetCurrentCell()));
                        }
                    }
                    else
                    {
                        try
                        {
                            var a = GameManagerScript.PiecesOnBoard[PositionHelper.MoveDown(movement.GetCurrentCell())];
                        }
                        catch (KeyNotFoundException e)
                        {
                            list.Add(Movement.CreateDirectionalMovement(EDirections.Down, movement.GetCurrentCell()));
                        }
                    }

                    return list;
                }

                List<(int, int)> PossibleEats()
                {
                    var foods = new List<(int, int)>();
                    if (chess.White)
                    {
                        foods.Add((chess.Row + 1, chess.Column - 1));
                        foods.Add((chess.Row - 1, chess.Column - 1));
                    }
                    else
                    {
                        foods.Add((chess.Row + 1, chess.Column + 1));
                        foods.Add((chess.Row - 1, chess.Column + 1));
                    }

                    return foods;
                }
            }));

        public readonly Rect TextureRect;
        public readonly string Name;
        public readonly PieceLegalMovements PieceLegalMovements;
        
        private static readonly PieceTypes[] Types = { Queen, King, Bishop, Knight, Rook, Pawn };

        private PieceTypes(string name, Rect textureRect, PieceLegalMovements pieceLegalMovements)
        {
            TextureRect = textureRect;
            Name = name;
            PieceLegalMovements = pieceLegalMovements;
        }
        
        public static PieceTypes? SerializeType(string type)
        {
            foreach (var typee in Types)
            {
                if (typee.Name.Equals(type))
                {
                    return typee;
                }
            }

            return null;
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

    public class PositionHelper
    {
        public static (int, int) MoveUp((int, int) cord)
        {
            return (cord.Item1, cord.Item2 - 1);
        }

        public static (int, int) MoveDown((int, int) cord)
        {
            return (cord.Item1, cord.Item2 + 1);
        }

        public static (int, int) MoveLeft((int, int) cord)
        {
            return (cord.Item1 - 1, cord.Item2);
        }

        public static (int, int) MoveRight((int, int) cord)
        {
            return (cord.Item1 + 1, cord.Item2);
        }

        public static (int, int) MoveLeftTop((int, int) cord)
        {
            return MoveUp(MoveLeft(cord));
        }

        public static (int, int) MoveRightTop((int, int) cord)
        {
            return MoveUp(MoveRight(cord));
        }

        public static (int, int) MoveLeftBottom((int, int) cord)
        {
            return MoveDown(MoveLeft(cord));
        }

        public static (int, int) MoveRightBottom((int, int) cord)
        {
            return MoveDown(MoveRight(cord));
        }
    }
}