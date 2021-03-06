﻿using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Player
{
    //Player Texture
    private readonly Texture Standing = Engine.LoadTexture("PlayerForward.png");
    //private readonly Texture Jumping = Engine.LoadTexture("PlayerJumping.png");
    //private readonly Texture JumpingLeft = Engine.LoadTexture("PlayerJumping.png");
    //private readonly Texture JumpingRight = Engine.LoadTexture("PlayerJumping.png");
    private readonly Texture Right = Engine.LoadTexture("PlayerRight.png");
    private readonly Texture Left = Engine.LoadTexture("PlayerLeft.png");
    private readonly float AnimationRate = 2.5f;
    private float StandingIndex = 0f;
    private float RightIndex = 0f;
    private float LeftIndex = 0f;

    private Texture Current;

    private readonly float ScreenX;
    private float ScreenY;
    private float Offset;
    private readonly float Width = 50;
    private readonly float Height = 62;

    //Movement variables
    private const float ACCELX = 0.5f;
    private const float MAXVELX = 3.75f;
    private float VelocityX;
    private float VelocityY;

    private const float JUMP = 1f;
    private const float GRAV = 1f;
    private const float MAXJUMPVEL = 7.5f;
    private const float MAXGRAVVEL = 7.5f;



    //Movement states
    //Walk state
    public enum WSTATE
    {
        STANDING,
        LEFT,
        RIGHT
    };

    private WSTATE WalkState;

    public enum JSTATE
    {
        STANDING,
        JUMPING,
        FALLING
    }

    private JSTATE JumpState;

    public Player()
    {
        ScreenX = Game.Resolution.X / 2 - Width / 2;
        ScreenY = 0;
        Offset = 64;

        WalkState = WSTATE.STANDING;
        JumpState = JSTATE.STANDING;

        Current = Standing;
    }

    public void Input(List<List<int>> tiles)
    {
        if(Engine.GetKeyHeld(Key.A))
        {
            WalkState = WSTATE.LEFT;
        }
        else if(Engine.GetKeyHeld(Key.D))
        {
            WalkState = WSTATE.RIGHT;
        }
        else
        {
            WalkState = WSTATE.STANDING;
        }
        if(Engine.GetKeyHeld(Key.W) && JumpState == JSTATE.STANDING)
        {
            JumpState = JSTATE.JUMPING;
        }

        Velocities();

        bool cx = false;
        bool cy = false;

        //Collision
        for (int y = 0; y < tiles.Count; y++)
        {
            for(int x = 0; x < tiles[y].Count; x++)
            {
                Bounds2 tBounds;
                if(tiles[y][x] != GameScreen.AIR)
                {
                    tBounds = new Bounds2(((x - GrassBiome.LEVELONEWIDTH / 2) * GrassBiome.TILESIZE) - (Offset + VelocityX), y * GrassBiome.TILESIZE, GrassBiome.TILESIZE, GrassBiome.TILESIZE);
                    if (GetBounds().Overlaps(tBounds))
                    {
                        cx = true;
                    }

                    tBounds = new Bounds2(((x - GrassBiome.LEVELONEWIDTH / 2) * GrassBiome.TILESIZE) - (Offset), (y * GrassBiome.TILESIZE), GrassBiome.TILESIZE, GrassBiome.TILESIZE);
                    Bounds2 pBounds = new Bounds2(ScreenX + 7, ScreenY + VelocityY, Width, Height);
                    if (pBounds.Overlaps(tBounds))
                    {
                        cy = true;
                    }
                    tBounds = new Bounds2(((x - GrassBiome.LEVELONEWIDTH / 2) * GrassBiome.TILESIZE) - (Offset), y * GrassBiome.TILESIZE, GrassBiome.TILESIZE, GrassBiome.TILESIZE);
                    float dx = VelocityX / Math.Abs(VelocityX);
                    
                    float dy = VelocityY / Math.Abs(VelocityY);
                    if (GetBounds().Overlaps(tBounds))
                    {
                        tBounds = new Bounds2(((x - GrassBiome.LEVELONEWIDTH / 2) * GrassBiome.TILESIZE) - (Offset), y * GrassBiome.TILESIZE, GrassBiome.TILESIZE, GrassBiome.TILESIZE);
                        while (GetBounds().Overlaps(tBounds))
                        {
                            ScreenY -= 1;
                        }
                    }
                }
            }
        }
        
        if (cx)
        {
            VelocityX = 0;
            WalkState = WSTATE.STANDING;
            Current = Standing;
            
        }
        if (cy)
        {
            Current = Standing;
            VelocityY = 0;
            JumpState = JSTATE.STANDING;
        }
        else if(JumpState == JSTATE.STANDING)
        {
            JumpState = JSTATE.FALLING;
        }
    }

    public void Update()
    {
        Offset += VelocityX;
        ScreenY += VelocityY;
    }

    public void Render()
    {
        //Animation
        StandingIndex = (StandingIndex + Engine.TimeDelta * AnimationRate) % 2.0f;
        RightIndex = (RightIndex + Engine.TimeDelta * AnimationRate) % 2.0f;
        LeftIndex = (LeftIndex + Engine.TimeDelta * AnimationRate) % 2.0f;

        Bounds2 AnimationBounds = new Bounds2(0,0,64,64);

        if(WalkState == WSTATE.STANDING)
        {
            AnimationBounds = new Bounds2(((int)StandingIndex) * 64, 0, 64, 64);
            Current = Standing;
        }
        else if(WalkState == WSTATE.RIGHT)
        {
            AnimationBounds = new Bounds2(((int)RightIndex) * 48, 0, 48, 64);
            Current = Right;
        }
        else if(WalkState == WSTATE.LEFT)
        {
            AnimationBounds = new Bounds2(((int)LeftIndex) * 48, 0, 48, 64);
            Current = Left;
        }


        Engine.DrawTexture(Current, new Vector2(ScreenX, ScreenY), source: AnimationBounds);
    }

    private void Velocities()
    {
        switch (WalkState)
        {
            case WSTATE.LEFT:
                if (VelocityX - ACCELX >= -MAXVELX)
                {
                    VelocityX -= ACCELX;
                }
                else
                {
                    VelocityX = -MAXVELX;
                }
                break;
            case WSTATE.RIGHT:
                
            
                if (VelocityX + ACCELX <= MAXVELX)
                {
                    VelocityX += ACCELX;
                }
                else
                {
                    VelocityX = MAXVELX;
                }
                break;
            case WSTATE.STANDING:
                VelocityX = 0;
                Current = Standing;
                break;
        }

        switch(JumpState)
        {
            case JSTATE.STANDING:
                VelocityY = 0;
                break;
            case JSTATE.JUMPING:
                if(VelocityY - JUMP >= - MAXJUMPVEL)
                {
                    VelocityY -= JUMP;
                }
                else
                {
                    JumpState = JSTATE.FALLING;
                }
                break;
            case JSTATE.FALLING:
                if(VelocityY + GRAV <= MAXGRAVVEL)
                {
                    VelocityY += GRAV;
                }
                else
                {
                    VelocityY = MAXGRAVVEL;
                }
                break;
        }


    }

    public float GetScreenX()
    {
        return ScreenX + 7;
    }

    public void SetOffset(float x)
    {
        Offset = x;
    }

    public void SetScreenY(float y)
    {
        ScreenY = y;
    }

    public float GetScreenY()
    {
        return ScreenY;
    }

    public float GetW()
    {
        return Width;
    }

    public float GetH()
    {
        return Height;
    }

    public bool HitsBounds(Bounds2 other)
    {
        return new Bounds2(ScreenX, ScreenY, Width, Height).Overlaps(other);
    }

    public bool WillHitBoundsAfterVY(Bounds2 other)
    {
        return new Bounds2(ScreenX, ScreenY + VelocityY, Width, Height).Overlaps(other);
    }

    public Bounds2 GetBounds()
    {
        return new Bounds2(ScreenX + 7, ScreenY, Width, Height);
    }

    public float GetOffset()
    {
        return Offset;
    }

    public WSTATE GetWState()
    {
        return WalkState;
    }

    public void SetWState(WSTATE state)
    {
        WalkState = state;
    }

    public JSTATE GetJState()
    {
        return JumpState;
    }

    public void SetJState(JSTATE state)
    {
        JumpState = state;
    }

    public float GetVelocityX()
    {
        return VelocityX;
    }

    public void SetVelocityX(float vel)
    {
        VelocityX = vel;
    }

    public float GetVelocityY()
    {
        return VelocityY;
    }

    public void SetVelocityY(float vel)
    {
        VelocityY = vel;
    }

    public bool OverlapsX(float min, float max)
    {
        return ScreenX + VelocityX > min && ScreenX + VelocityX < max || min > ScreenX + VelocityX && min < ScreenX + VelocityX;
    }

    public bool OverlapsY(float min, float max)
    {
        return ScreenY + VelocityY > min && ScreenY + VelocityY < max || min > ScreenY + VelocityY && min < ScreenY + VelocityY;
    }
}
