using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Futzim
{
    public struct PlayerAttrib
    {
        public float Finishing { get; set; } = 7f;
        public float Passing { get; set; } = 7f;
        public float Dribbling { get; set; } = 7f;

        public float Aerial { get; set; } = 7f;
        public float Marking { get; set; } = 7f;
        public float Goalkeeping { get; set; } = 7f;

        public float Pace { get; set; } = 7f;

        public float Resistance { get; set; } = 7f;
        public float WorkRate { get; set; } = 7f;
        public float Mentality { get; set; } = 7f;

        public PlayerAttrib()
        {
            Dribbling = 7f;
        }
        public PlayerAttrib(float dri, float pac, float pas, float wkR)
        {
            Dribbling = dri;
            Passing = pas;
            Pace = pac;
            WorkRate = wkR;
        }
        public PlayerAttrib(float fin, float pas, float dri, float aer, float mar, float pac, float res, float gkp, float wkR)
        {
            Finishing = fin;
            Passing = pas;
            Aerial = aer;
            Dribbling = dri;
            Passing = pas;
            Marking = mar;
            Resistance = res;
            Goalkeeping = gkp;
            Pace = pac;
            WorkRate = wkR;
        }


        public float GetAvg()
        {
            var sum = (Finishing + Passing + Pace + Marking + Goalkeeping + Dribbling + Aerial);

            return sum / 7f;
        }
    }
}
