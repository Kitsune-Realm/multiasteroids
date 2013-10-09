using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework;

namespace MultiAsteroids
{
    class SelectCursor
    {
        public Texture2D Texture;
        public SoundEffect sfxMove { get; set; }
        public SoundEffect sfxSelect { get; set; }
        public string[] menuContent { get; set; }
        public int menuIndex { get; set; }
        public Vector2 position { get; set; }

        public SelectCursor(ContentManager content)
        {
            this.Texture = content.Load<Texture2D>("green_projectile_sheet");
            this.sfxMove = content.Load<SoundEffect>("sounds/BigButtonClick");
            this.sfxSelect = content.Load<SoundEffect>("sounds/SelectSomething");
            this.position = new Vector2(0, 0);
            menuContent = new string[0];
        }

        public void updateMenu(int x)
        {
            if(menuIndex + x >= 0 && menuIndex + x <= (menuContent.Length-1))
                menuIndex += x;
            sfxMove.Play();
        }
    }
}
