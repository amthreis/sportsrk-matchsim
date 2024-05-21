using System.Numerics;
namespace SRkMatchSimAPI.Framework;


    public class Squad
    {
        public Squad(Club club)
        {
            Club = club;
        }



        public static Dictionary<string, Vector2[]> Formations { get; } = new Dictionary<string, Vector2[]>();

        static Squad()
        {
            Formations["4-4-2"] = new Vector2[10]
            {
                //def
                new Vector2(0.8f, 0.2f),
                new Vector2(0.6f, 0.2f),
                new Vector2(0.4f, 0.2f),
                new Vector2(0.2f, 0.2f),

                //mid
                new Vector2(0.33f, 0.4f),
                new Vector2(0.67f, 0.4f),
                new Vector2(0.7f, 0.6f),
                new Vector2(0.3f, 0.6f),

                //atk
                new Vector2(0.4f, 0.8f),
                new Vector2(0.6f, 0.8f)
            };

            Formations["4-3-3"] = new Vector2[10]
            {
                //def
                new Vector2(0.875f, 0.2727273f),
                new Vector2(0.375f, 0.2272728f),
                new Vector2(0.625f, 0.2272728f),
                new Vector2(0.125f, 0.2727273f),
                
                //mid
                new Vector2(0.375f, 0.4090909f),
                new Vector2(0.625f, 0.4090909f),
                new Vector2(0.5f, 0.5454545f),
                
                //atk
                new Vector2(0.8f, 0.6818184f),
                new Vector2(0.2f, 0.6818182f),
                new Vector2(0.5f, 0.772728f)
            };

            Formations["3-5-2"] = new Vector2[10]
            {
                //def
                new Vector2(0.4999999f, 0.2272727f),
                new Vector2(0.2499998f, 0.2727273f),
                new Vector2(0.75f, 0.2727273f),
                
                //mid
                new Vector2(0.375f, 0.4090909f),
                new Vector2(0.625f, 0.4090909f),
                new Vector2(0.875f, 0.5454552f),
                new Vector2(0.125f, 0.5454546f),
                new Vector2(0.5f, 0.590909f),
                
                //atk
                new Vector2(0.375f, 0.7727274f),
                new Vector2(0.6250002f, 0.7727273f)
            };
        }


        public Club Club { get; }

        public List<SquadSlot> Slots = new List<SquadSlot>(45);

        public void AddPlayer(Player player)
        {
            Slots.Add(new SquadSlot(Slots.Count, player, Slots.Count + 1, new Vector2(0.5f, 0.5f)));
        }


        public void AddPlayers(params Player[] players)
        {
            foreach(var p in players)
            {
                Slots.Add(new SquadSlot(Slots.Count, p, Slots.Count + 1, new Vector2(0.5f, 0.5f)));
            }
        }
    }
