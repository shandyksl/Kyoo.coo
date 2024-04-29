using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoo.Core.Model.DbModels
{
    public class GameConfig
    {
        public int Id { get; set; }
        public string AgentCode { get; set; }
        public string GameType { get; set; }
        public string GameName { get; set; }
        public string GameSettings { get; set; }
        public int GameBadge { get; set; }
        public int Rank { get; set; }
        public int IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}
