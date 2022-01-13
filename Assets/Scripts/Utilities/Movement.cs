using System.Collections.Generic;

namespace Utilities
{
    public class Movement : IMovement
    {
        protected (int, int) DestCell;
        protected readonly (int, int) CurrentCell;

        public Movement((int, int) destCell, (int, int) currentCell)
        {
            DestCell = destCell;
            CurrentCell = currentCell;
        }

        public (int, int) GetDestCell()
        {
            return DestCell;
        }

        public (int, int) GetCurrentCell()
        {
            return CurrentCell;
        }

        public virtual List<IMovement> GetPossibleMovements(ChessPieceScript chess)
        {
            throw new System.NotImplementedException();
        }

        public static IMovement CreateDirectionalMovement(EDirections direction, (int, int) currentCell, int range = 1)
        {
            int row = currentCell.Item1;
            int column = currentCell.Item2;

            switch (direction)
            {
                case EDirections.Up:
                    column -= range;
                    break;
                case EDirections.Down:
                    column += range;
                    break;
                case EDirections.Left:
                    row -= range;
                    break;
                case EDirections.Right:
                    row += range;
                    break;
                case EDirections.LeftBtmCorner:
                    row -= range;
                    column += range;
                    break;
                case EDirections.LeftTopCorner:
                    row -= range;
                    column -= range;
                    break;
                case EDirections.RightBtmCorner:
                    row += range;
                    column += range;
                    break;
                case EDirections.RightTopCorner:
                    row += range;
                    column -= range;
                    break;
                default:
                    break;
            }

            return new Movement((row, column), currentCell);
        }

        public static bool IsInRange(PieceType type, (int, int) currentCell, (int, int) destCell)
        {
            var row = destCell.Item1 - currentCell.Item1;
            var column = destCell.Item2 - currentCell.Item2;
            var range = type.PieceLegalMovements.Range;

            var r = row < 0 ? -row : row;
            var c = column < 0 ? -column : column;

            return range >= r && range >= c;
        }
        
        public static bool IsInRange(string type, (int, int) currentCell, (int, int) destCell)
        {
            return IsInRange(PieceType.SerializeType(type).GetValueOrDefault(PieceType.Pawn), currentCell, destCell);
        }

        public static EDirections GetDirectionOfMovement(IMovement movement)
        {
            (var currentRow, var currentColumn) = movement.GetCurrentCell();
            (var destRow, var destColumn) = movement.GetDestCell();

            // current row is larger than destination row, moved left
            if (currentRow > destRow)
            {
                // current column is larger than destination column, moved up
                if (currentColumn > destColumn)
                    return EDirections.LeftTopCorner;

                // current column is smaller than destination column, moved down
                if (currentColumn < destColumn)
                    return EDirections.LeftBtmCorner;

                return EDirections.Left;
            }

            // current row is smaller than destination row, moved right
            if (currentRow < destRow)
            {
                // current column is larger than destination column, moved up
                if (currentColumn > destColumn)
                    return EDirections.RightTopCorner;

                // current column is smaller than destination column, moved down
                return currentColumn < destColumn ? EDirections.LeftBtmCorner : EDirections.Right;
            }

            if (currentColumn > destColumn)
                return EDirections.Up;

            return currentColumn < destColumn ? EDirections.Down : EDirections.Invalid;
        }

        protected static List<IMovement> RemoveObstacleMoves(List<IMovement> possibleMoves)
        {
            var list = new List<IMovement>(possibleMoves);

            foreach (var movement in possibleMoves)
            {
                switch (GetDirectionOfMovement(movement))
                {
                    case EDirections.Up:
                        for (var column = movement.GetDestCell().Item2 + 1;
                            column < movement.GetCurrentCell().Item2;
                            column++)
                        {
                            try
                            {
                                var piece = GameManagerScript.PiecesOnBoard[(movement.GetCurrentCell().Item1, column)];
                                // if code was able to get here means there is a piece in the way, so we remove this movement as a possible movement.
                                list.Remove(movement);
                            }
                            catch (KeyNotFoundException e)
                            {
                                // if there is no piece at the position, good. So we don't do anything.
                            }
                        }

                        break;
                    case EDirections.Down:
                        for (var column = movement.GetDestCell().Item2 - 1;
                            column > movement.GetCurrentCell().Item2;
                            column--)
                        {
                            try
                            {
                                var piece = GameManagerScript.PiecesOnBoard[(movement.GetCurrentCell().Item1, column)];
                                // if code was able to get here means there is a piece in the way, so we remove this movement as a possible movement.
                                list.Remove(movement);
                            }
                            catch (KeyNotFoundException e)
                            {
                                // if there is no piece at the position, good. So we don't do anything.
                            }
                        }

                        break;
                    case EDirections.Left:
                        for (var row = movement.GetDestCell().Item1 + 1; row < movement.GetCurrentCell().Item1; row++)
                        {
                            try
                            {
                                var piece = GameManagerScript.PiecesOnBoard[(row, movement.GetCurrentCell().Item2)];
                                // if code was able to get here means there is a piece in the way, so we remove this movement as a possible movement.
                                list.Remove(movement);
                            }
                            catch (KeyNotFoundException e)
                            {
                                // if there is no piece at the position, good. So we don't do anything.
                            }
                        }

                        break;
                    case EDirections.Right:
                        for (var row = movement.GetDestCell().Item1 - 1; row > movement.GetCurrentCell().Item1; row--)
                        {
                            try
                            {
                                var piece = GameManagerScript.PiecesOnBoard[(row, movement.GetCurrentCell().Item2)];
                                // if code was able to get here means there is a piece in the way, so we remove this movement as a possible movement.
                                list.Remove(movement);
                            }
                            catch (KeyNotFoundException e)
                            {
                                // if there is no piece at the position, good. So we don't do anything.
                            }
                        }

                        break;
                    case EDirections.LeftTopCorner:
                        int rowLTC = movement.GetDestCell().Item1 + 1;
                        for (var column = movement.GetDestCell().Item2 + 1;
                            column < movement.GetCurrentCell().Item2;
                            column++)
                        {
                            try
                            {
                                var piece = GameManagerScript.PiecesOnBoard[(rowLTC, column)];
                                // if code was able to get here means there is a piece in the way, so we remove this movement as a possible movement.
                                list.Remove(movement);
                            }
                            catch (KeyNotFoundException e)
                            {
                                // if there is no piece at the position, good. So we don't do anything.
                            }

                            rowLTC++;
                        }

                        break;
                    case EDirections.LeftBtmCorner:
                        int rowLBC = movement.GetDestCell().Item1 + 1;
                        for (var column = movement.GetDestCell().Item2 - 1;
                            column > movement.GetCurrentCell().Item2;
                            column--)
                        {
                            try
                            {
                                var piece = GameManagerScript.PiecesOnBoard[(rowLBC, column)];
                                // if code was able to get here means there is a piece in the way, so we remove this movement as a possible movement.
                                list.Remove(movement);
                            }
                            catch (KeyNotFoundException e)
                            {
                                // if there is no piece at the position, good. So we don't do anything.
                            }

                            rowLBC++;
                        }

                        break;
                    case EDirections.RightTopCorner:
                        int rowRTC = movement.GetDestCell().Item1 - 1;
                        for (var column = movement.GetDestCell().Item2 + 1;
                            column < movement.GetCurrentCell().Item2;
                            column++)
                        {
                            try
                            {
                                var piece = GameManagerScript.PiecesOnBoard[(rowRTC, column)];
                                // if code was able to get here means there is a piece in the way, so we remove this movement as a possible movement.
                                list.Remove(movement);
                            }
                            catch (KeyNotFoundException e)
                            {
                                // if there is no piece at the position, good. So we don't do anything.
                            }

                            rowRTC--;
                        }

                        break;
                    case EDirections.RightBtmCorner:
                        int rowRBC = movement.GetDestCell().Item1 - 1;
                        for (var column = movement.GetDestCell().Item2 - 1;
                            column > movement.GetCurrentCell().Item2;
                            column--)
                        {
                            try
                            {
                                var piece = GameManagerScript.PiecesOnBoard[(rowRBC, column)];
                                // if code was able to get here means there is a piece in the way, so we remove this movement as a possible movement.
                                list.Remove(movement);
                            }
                            catch (KeyNotFoundException e)
                            {
                                // if there is no piece at the position, good. So we don't do anything.
                            }

                            rowRBC--;
                        }

                        break;
                }

                try
                {
                    var destPiece = GameManagerScript.PiecesOnBoard[movement.GetDestCell()];
                    var currentPiece = GameManagerScript.PiecesOnBoard[movement.GetCurrentCell()];
                    if ((currentPiece.White && destPiece.White) || (!currentPiece.White && !destPiece.White))
                    {
                        list.Remove(movement);
                    }
                }
                catch (KeyNotFoundException e)
                {
                }
            }

            return list;
        }
    }

    public class PieceMovement : Movement
    {
        protected readonly PieceType Type;

        public PieceMovement(string pieceType, (int, int) destCell, (int, int) currentCell) : base(destCell,
            currentCell)
        {
            Type = PieceType.SerializeType(pieceType).GetValueOrDefault(PieceType.Pawn);
        }
        
        public PieceMovement(PieceType pieceType, (int, int) destCell, (int, int) currentCell) : base(destCell,
            currentCell)
        {
            Type = pieceType;
        }

        public override List<IMovement> GetPossibleMovements(ChessPieceScript chess)
        {
            if (Type.PieceLegalMovements.CustomMoveCondition.Invoke(this, chess))
            {
                var movement = Type.PieceLegalMovements.CustomMovement.Invoke(this, chess);
                if (movement != null)
                {
                    var list1 = new List<IMovement>();
                    
                    // remove moves with cells out of the board
                    foreach (var move1 in movement)
                    {
                        try
                        {
                            var c = GameManagerScript.CellsOnBoard[move1.GetDestCell()];
                            list1.Add(move1);
                        }
                        catch (KeyNotFoundException e)
                        {
                        }
                    }
                    
                    return Type.Name.Equals("Knight") ? list1 : RemoveObstacleMoves(list1);
                }
            }

            var list = new List<IMovement>();

            if (!IsInRange(Type, CurrentCell, DestCell)) return list;

            foreach (var direction in Type.PieceLegalMovements.Directions)
            {
                for (ushort i = 0; i < Type.PieceLegalMovements.Range; i++)
                {
                    var move = CreateDirectionalMovement(direction, CurrentCell, i + 1);
                    try
                    {
                        var c = GameManagerScript.CellsOnBoard[move.GetDestCell()];
                        list.Add(move);
                    }
                    catch (KeyNotFoundException e)
                    {
                    }
                }
            }

            return Type.Name.Equals("Knight") ? list : RemoveObstacleMoves(list);
        }
    }

    public interface IMovement
    {
        (int, int) GetDestCell();

        (int, int) GetCurrentCell();

        List<IMovement> GetPossibleMovements(ChessPieceScript chess);
    }
}