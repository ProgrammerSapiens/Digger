using System.Threading;
using System.Windows.Forms;

namespace Digger
{
    internal class Player : ICreature
    {
        public CreatureCommand Act(int x, int y)
        {
            var creatureCommand = new CreatureCommand();
            switch (Game.KeyPressed)
            {
                case Keys.Up: if (y - 1 >= 0 && !(Game.Map[x, y - 1] is Sack)) creatureCommand.DeltaY = -1; break;
                case Keys.Right: if (x + 1 < Game.MapWidth && !(Game.Map[x + 1, y] is Sack)) creatureCommand.DeltaX = 1; break;
                case Keys.Down: if (y + 1 < Game.MapHeight && !(Game.Map[x, y + 1] is Sack)) creatureCommand.DeltaY = 1; break;
                case Keys.Left: if (x - 1 >= 0 && !(Game.Map[x - 1, y] is Sack)) creatureCommand.DeltaX = -1; break;
            }
            return creatureCommand;
        }
        public bool DeadInConflict(ICreature conflictedObject)
        {
            if (conflictedObject is Terrain || conflictedObject is Gold)
            {
                return false;
            }
            Game.IsOver = true;
            Application.Exit();
            return true;
        }
        public int GetDrawingPriority() => 1;
        public string GetImageFileName() => "Digger.png";
    }
    internal class Monster : ICreature
    {
        public CreatureCommand Act(int x, int y)
        {
            if (Game.IsOver == true) return new CreatureCommand();
            var creatureCommand = new CreatureCommand();
            (int playerX, int playerY) = WhereIsThePlayer();
            if (playerY < y && WalkAbility(x, y - 1)) { creatureCommand.DeltaY = -1; }
            if (playerY > y && WalkAbility(x, y + 1)) { creatureCommand.DeltaY = +1; }
            if (playerX < x && WalkAbility(x - 1, y)) { creatureCommand.DeltaX = -1; }
            if (playerX > x && WalkAbility(x + 1, y)) { creatureCommand.DeltaX = +1; }
            return creatureCommand;
        }
        private (int, int) WhereIsThePlayer()                           //Обнаружение координат игрока
        {
            int playerX = 0;
            int playerY = 0;
            bool playerIsFound = false;
            for (int i = 0; i < Game.MapHeight; i++)
            {
                for (int j = 0; j < Game.MapWidth; j++)
                {
                    if (Game.Map[i, j] is Player)
                    {
                        playerX = i;
                        playerY = j;
                        playerIsFound = true;
                        break;
                    }
                }
                if (playerIsFound == true) break;
            }
            return (playerX, playerY);
        }

        private bool WalkAbility(int x, int y)
        {
            var whatIsNearby = Game.Map[x, y];
            if (whatIsNearby is Sack || whatIsNearby is Terrain || whatIsNearby is Monster) return false;
            return true;
        }
        public bool DeadInConflict(ICreature conflictedObject)
        {
            if (conflictedObject is Sack || conflictedObject is Monster) return true;                       //При контакте с мешком или монстром уничтожается
            return false;
        }
        public int GetDrawingPriority() => 0;
        public string GetImageFileName() => "Monster.png";
    }
    internal class Terrain : ICreature
    {
        public CreatureCommand Act(int x, int y) => new CreatureCommand();
        public bool DeadInConflict(ICreature conflictedObject) => true;
        public int GetDrawingPriority() => 2;
        public string GetImageFileName() => "Terrain.png";
    }
    internal class Sack : ICreature
    {
        int count = 0;
        public CreatureCommand Act(int x, int y)
        {
            int mapHeight = Game.MapHeight;
            if (y + 1 < mapHeight)                                      //Пока мешок не достигнет нижней границы
            {
                var whatIsBelow = Game.Map[x, y + 1];
                if (whatIsBelow == null || (count >= 1 && (whatIsBelow is Player) || (whatIsBelow is Monster)))
                {
                    ++count;
                    return new CreatureCommand() { DeltaX = 0, DeltaY = +1 };
                }
                else if (count <= 1 && whatIsBelow != null) count = 0;
                else if (count > 1 && whatIsBelow != null) return new CreatureCommand() { DeltaX = 0, DeltaY = 0, TransformTo = new Gold() };
            }
            else if (count > 1 && y + 1 == mapHeight) return new CreatureCommand() { DeltaX = 0, DeltaY = 0, TransformTo = new Gold() };
            return new CreatureCommand();
        }
        public bool DeadInConflict(ICreature conflictedObject) => false;
        public int GetDrawingPriority() => -1;
        public string GetImageFileName() => "Sack.png";
    }
    internal class Gold : ICreature
    {
        public CreatureCommand Act(int x, int y) => new CreatureCommand();
        public bool DeadInConflict(ICreature conflictedObject)
        {
            if (conflictedObject is Player) Game.Scores += 10;
            return true;
        }
        public int GetDrawingPriority() => 2;
        public string GetImageFileName() => "Gold.png";
    }
}
