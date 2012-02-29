using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace WindowsGame4
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    /// 




    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;


        Texture2D ball;
        Texture2D bar;
        Texture2D block;

        Vector2 ballPos;
        Vector2 barPos;
        Vector2 [,] blocksPos;


        Vector2 ballVel;
        Vector2 barVel;

        Random rdm;
        int height, width;
        int numBrickRows;
        int score;

        SoundEffect sound;
        SoundEffectInstance soundEngineInstance;
        SoundEffect soundBrickHit;
        SoundEffectInstance soundBrickHitEngineInstance;

        // Display the score and the level on the screen.
        scoreboard scoreBoard;

        
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            rdm = new Random();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            // Create the scoreboard and set the initial level and score values.
            scoreBoard = new scoreboard(Content.Load<SpriteFont>("SpriteFont1"));
            scoreBoard.Score = score;
            width = graphics.GraphicsDevice.Viewport.Width;
            height = graphics.GraphicsDevice.Viewport.Height;


            ball = Content.Load<Texture2D>("ball");
            bar = Content.Load<Texture2D>("bar");

            ballPos = new Vector2(width/2 + rdm.Next()%100 - 50, rdm.Next() % (height/2));
            barPos = new Vector2(width/2 - bar.Width, height - bar.Height );

            ballVel = new Vector2(250.0f,250.0f);
            barVel = new Vector2(300.0f,300.0f);

            block = Content.Load<Texture2D>("brick");
            blocksPos = new Vector2[15,3];

            numBrickRows = 3;
            score = 0;


            int padding = 7;
            int bricksForScreen = width / (block.Width + padding);

            int topLeft = graphics.GraphicsDevice.Viewport.X;
            int topRight = graphics.GraphicsDevice.Viewport.Y;
            

            int currentRow = 0;
            
            for (int j=0; j<numBrickRows;j++){
                int currentPos = 0 ;
                for (int i = 0; i <= bricksForScreen; i++)
                {   
                    blocksPos[i,j] = new Vector2((float) currentPos,(float) currentRow );
                    currentPos += block.Width+padding;
                }
                currentRow += block.Height + 5;
            }


            sound = Content.Load<SoundEffect>("53576_newgrounds_guitar");
            soundEngineInstance = sound.CreateInstance();
            soundEngineInstance.IsLooped = true;
            soundEngineInstance.Play();
          //  soundBrickHit = Content.Load<SoundEffect>();
            soundBrickHitEngineInstance = sound.CreateInstance();
            
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            ballPos += ballVel * (float)gameTime.ElapsedGameTime.TotalSeconds;

            detectBallWallCollision();
            detectBallBarCollision();
            detectBallBrickCollision();
            processKeyboard(gameTime);

            base.Update(gameTime);
        }

        void detectBallWallCollision()
        {
            //ball hit the left-side of the wall
            if (ballPos.X < 0)
            {
                ballVel.X *= -1;//bounce the ball
                ballPos.X = 0;//set the ball back within the bounds of the game
            }
            //ball hit the ceiling
            if (ballPos.Y < 0)
            {
                ballVel.Y *= -1;//bounce the ball
                ballPos.Y = 0;//set the ball back within the bounds of the game
            }
            //ball hits right-side of the wall
            if (ballPos.X > width - ball.Width)
            {
                ballVel.X *= -1;//bounce the ball
                ballPos.X = width - ball.Width;//set the ball back within the bounds of the game
            }
            //ball hits the floor
            if (ballPos.Y > height - ball.Height)
            {
                ballVel.Y *= -1;//bounce the ball
                ballPos.Y = height - ball.Height;//set the ball back within the bounds of the game
            }
        }

        void detectBallBarCollision()
        {
            Vector3 ballMin = new Vector3(ballPos.X, ballPos.Y, 0);
            Vector3 ballMax = new Vector3(ballPos.X + ball.Width, ballPos.Y + ball.Height, 0);
 
            Vector3 barMin = new Vector3(barPos.X, barPos.Y, 0);
            Vector3 barMax = new Vector3(barPos.X + bar.Width, barPos.Y + bar.Height, 0);

            Vector3 ballCenter = new Vector3(ballPos.X + (ball.Width/2), ballPos.Y + (ball.Height/2),0);
            Vector3 barCenter = new Vector3(barPos.X + (bar.Width / 2), barPos.Y + (bar.Height / 2), 0);

            BoundingBox ballBoundingBox1, barBoundingBox2;

            ballBoundingBox1.Min = ballMin;
            ballBoundingBox1.Max = ballMax; 

            barBoundingBox2.Min = barMin;
            barBoundingBox2.Max = barMax;

             
            //bounce the ball off the bar
            if (ballBoundingBox1.Intersects(barBoundingBox2))
            { 
                float maxBounceAngle = (float)Math.PI / 4;//45 degree angle, in radians

                float contactDistanceFromCenterofBar = (barPos.X + bar.Width / 2) - (ballPos.X + ball.Width / 2);//where the ball hit the bar
                float normalizedContactPoint = contactDistanceFromCenterofBar / (bar.Width / 2); // making the value relative to how far from center of the bar the ball hits
                float bounceAngle = normalizedContactPoint * maxBounceAngle;

                //calculate angle to bounce based on how far it hit from center of box
             //   if (ballPos.Y <= (height - block.Height))//ball hit the top of the bar
             //   {
                   float x = ballVel.X;
                   float y = ballVel.Y;
                      
                   float vectorMagnitude = (x*x) + (y*y);
                   float newX = x*(float)Math.Cos(bounceAngle);

                  //calculate what Y needs to become to make up for change in x
                   float newY = (float)Math.Sqrt(vectorMagnitude - Math.Pow(newX, 2));
                   ballVel.X = newX;
                   ballVel.Y = newY*-1;
                   ballPos.Y = barPos.Y - ball.Height;
            }
        }

        void detectBallBrickCollision()
        {
            Vector3 ballMin = new Vector3(ballPos.X, ballPos.Y, 0);
            Vector3 ballMax = new Vector3(ballPos.X + ball.Width, ballPos.Y + ball.Height, 0);
            BoundingBox ballBoundingBox = new BoundingBox(ballMin, ballMax);

            Vector2 nearestPnt = ballPos;
            Vector3 brickMin, brickMax;
            for (int j = 0; j < numBrickRows; j++)
            {
                for (int i = 0; i < blocksPos.GetUpperBound(0); i++)
                {
                    if (blocksPos[i,j] != Vector2.Zero)
                    {
                        brickMin = new Vector3(blocksPos[i,j].X, blocksPos[i,j].Y, 0);
                        brickMax = new Vector3(blocksPos[i,j].X + block.Width, blocksPos[i,j].Y + block.Height, 0);

                        if (ballBoundingBox.Intersects(new BoundingBox(brickMin, brickMax)))
                        {
                            //what should we do? change x or y velocity?


                            //checking to see if it's to left of brick
                            if (ballPos.X < blocksPos[i,j].X && Math.Abs(blocksPos[i,j].Y - ballPos.Y) <= ball.Width / 2)
                            {
                                ballVel.X *= -1;
                                ballPos.X = blocksPos[i,j].X - ball.Width;
                            }

                            //check to see if it's too the right
                            else if (ballPos.X + ball.Width > blocksPos[i,j].X + block.Width && Math.Abs(blocksPos[i,j].Y - ballPos.Y) <= ball.Width / 2)
                            {
                                ballVel.X *= -1;
                                ballPos.X = blocksPos[i,j].X - ball.Width;
                            }
                            else
                            {
                                ballVel.Y *= -1;
                                if (ballPos.Y < blocksPos[i,j].Y)
                                {
                                    ballPos.Y = blocksPos[i,j].Y - ball.Height + 1;
                                }
                                else
                                {
                                    ballPos.Y = blocksPos[i,j].Y + block.Height - 1;
                                }
                            }
                            blocksPos[i,j] = Vector2.Zero;
                            score++;
                            soundBrickHitEngineInstance.Play();
                        }
                    }
                }
            }
        }
        void processKeyboard(GameTime gameTime) {
            KeyboardState s = Keyboard.GetState();
            
            if (s.IsKeyDown(Keys.Left) && barPos.X > 0 )
            {
                barPos.X -= barVel.X * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            if (s.IsKeyDown(Keys.Right) && barPos.X + bar.Width < width)
            {
                barPos.X += barVel.X * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            // TODO: Add your drawing code here

            spriteBatch.Begin();
            spriteBatch.Draw(ball, ballPos, Color.White);
            scoreBoard.Draw(spriteBatch);
            spriteBatch.Draw(bar, barPos, Color.White);
            for (int j = 0; j < numBrickRows; j++)
            {
                for (int i = 0; i < blocksPos.GetUpperBound(0); i++)
                {
                    if (blocksPos[i, j] != Vector2.Zero)
                    {
                        spriteBatch.Draw(block, blocksPos[i, j], Color.White);
                    }
                }
            }
            
            spriteBatch.End();
            base.Draw(gameTime);
            
        }


    }//end Game1
    class scoreboard
    {
        private Vector2 scorePos = new Vector2(20, 420);

        private SpriteFont _font;

        public int Score { get; set; }
        

        public scoreboard(SpriteFont font)
        {
            _font = font;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
         //   spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            spriteBatch.DrawString(_font, "Score:" + Score.ToString(), scorePos, Color.Black);
         //   spriteBatch.End();
        }
    }//end class scoreboard

}
