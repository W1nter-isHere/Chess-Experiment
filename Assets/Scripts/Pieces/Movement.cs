using System.Collections.Generic;

namespace Pieces
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

        public static IMovement CreateDirectionalMovement(EDirections direction, (int, int) currentCell)
        {
            int row = currentCell.Item1;
            int column = currentCell.Item2;

            switch (direction)
            {
                case EDirections.Up:
                    column--;
                    break;
                case EDirections.Down:
                    column++;
                    break;
                case EDirections.Left:
                    row--;
                    break;
                case EDirections.Right:
                    row++;
                    break;
                case EDirections.LeftBtmCorner:
                    row--;
                    column++;
                    break;
                case EDirections.LeftTopCorner:
                    row--;
                    column--;
                    break;
                case EDirections.RightBtmCorner:
                    row++;
                    column++;
                    break;
                case EDirections.RightTopCorner:
                    row++;
                    column--;
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
                    return EDirections.LeftTopCorner;

                // current column is smaller than destination column, moved down
                return currentColumn < destColumn ? EDirections.LeftBtmCorner : EDirections.Right;
            }

            if (currentColumn > destColumn)
                return EDirections.Up;
            
            return currentColumn < destColumn ? EDirections.Down : EDirections.Invalid;
        }
    }

    public class PieceMovement : Movement
    {
        protected readonly PieceType Type;

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
                    return movement;
                }
            }

            if (!IsInRange(Type, CurrentCell, DestCell)) return null;
            
            var list = new List<IMovement> {this};
            foreach (var direction in Type.PieceLegalMovements.Directions)
            {
                list.Add(CreateDirectionalMovement(direction, CurrentCell));
            }

            return list;
        }
    }

    public interface IMovement
    {
        (int, int) GetDestCell();

        (int, int) GetCurrentCell();

        List<IMovement> GetPossibleMovements(ChessPieceScript chess);
    }
}