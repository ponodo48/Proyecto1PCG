using UnityEngine;

namespace Assets
{
    public class Nodo
    {
        public Nodo a;
        public Nodo b;
        public int x;
        public int y;
        public int height;
        public int width;

        public Nodo(int x, int y, int h, int w)
        {
            a = null;
            b = null;
            this.x = x;
            this.y = y;
            this.height = h;
            this.width = w;
        }

    }
}
