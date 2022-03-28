using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Windows.Threading;

namespace SpaceInvader
{

    public partial class MainWindow : Window
    {

        //Player movement
        bool goLeft, goRight;

        //Remove items from the canvas
        List<Rectangle> itemsToRemove = new List<Rectangle>();

        int enemyImages = 0;
        int bulletTimer = 0;
        int bulletTimerLimit = 90;
        int totalEnemies = 0;
        int enemySpeed = 6;
        bool gameOver = false;

        DispatcherTimer gameTimer = new DispatcherTimer();
        ImageBrush playerSkin = new ImageBrush();

        public MainWindow()
        {
            InitializeComponent();

            gameTimer.Tick += GameLoop;
            gameTimer.Interval = TimeSpan.FromMilliseconds(20);
            gameTimer.Start();

            //Player model
            playerSkin.ImageSource = new BitmapImage(new Uri(@"C:\Users\Admin\source\repos\SpaceInvader\SpaceInvader\images\Swordfish.png"));
            player.Fill = playerSkin;

            myCanvas.Focus();

            //Number of enemies
            makeEnimies(30);
        }

        private void GameLoop(object? sender, EventArgs e)
        {

            //Player hitbox

            Rect playerHitBox = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width, player.Height);
            enemiesLeft.Content = "Enimies Left: " + totalEnemies;

           //Left boundary
            if(goLeft == true && Canvas.GetLeft(player) > 0)
            {
                Canvas.SetLeft(player, Canvas.GetLeft(player) - 10);
            }    
            //Right boundary
            if(goRight == true && Canvas.GetLeft(player) + 80 < Application.Current.MainWindow.Width)
            {
                Canvas.SetLeft(player, Canvas.GetLeft(player) + 10);
            }

            //Loop creating bullets based on enemy location
            bulletTimer -= 3;

            if (bulletTimer < 0)
            {
                enemyBulletMaker(Canvas.GetLeft(player) + 20, 10 );

                bulletTimer = bulletTimerLimit;
            }


            //Foreach loop to identify enemy rectangles and enemy/player bullets

            foreach (var x in myCanvas.Children.OfType<Rectangle>())
            {
                //player bullet and speed
                if (x is Rectangle && (string)x.Tag == "bullet")
                {
                    Canvas.SetTop(x, Canvas.GetTop(x) - 20);

                    //destroy bullet
                    if (Canvas.GetTop(x) < 10)
                    {
                        itemsToRemove.Add(x);
                    }

                    //Functionality for enemy damage from player
                    Rect bullet = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);

                    //Another Foreach to idenfiy enemy seperately to interact with bullet

                    foreach (var y in myCanvas.Children.OfType<Rectangle>())
                    {
                        if (y is Rectangle && (string)y.Tag == "enemy")
                        {
                            Rect enemyHit = new Rect(Canvas.GetLeft(y), Canvas.GetTop(y), y.Width, y.Height);

                            if (bullet.IntersectsWith(enemyHit))
                            {
                                itemsToRemove.Add(x);
                                itemsToRemove.Add(y);
                                totalEnemies -= 1;

                            }
                        }
                    }

                }

                if(x is Rectangle && (string)x.Tag == "enemy")
                {
                    //Enemy movement right
                    Canvas.SetLeft(x, Canvas.GetLeft(x) + enemySpeed);

                    if (Canvas.GetLeft(x) > 820)
                    {
                        Canvas.SetLeft(x, -80);
                        Canvas.SetTop(x, Canvas.GetTop(x) + (x.Height + 10));
                    }

                    //Enemy hitbox
                    Rect enemyHitBox = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);

                    if (playerHitBox.IntersectsWith(enemyHitBox))
                    {
                        showGameOver("You were killed by the invaders!!");
                    }
                }

                //Enemy bullet following player
                if (x is Rectangle && (string)x.Tag == "enemyBullet")
                {
                    Canvas.SetTop(x, Canvas.GetTop(x) + 10);

                    //destroy bullet
                    if (Canvas.GetTop(x) > 480)
                    {
                        itemsToRemove.Add(x);
                    }
                    Rect enemyBulletHitBox = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);

                    if (playerHitBox.IntersectsWith(enemyBulletHitBox))
                    {
                        showGameOver("You were killed by the invader missle!!");
                    }
                }
            }

            foreach (Rectangle i in itemsToRemove)
            {
                myCanvas.Children.Remove(i);
            }

            //Enemy speed
            if(totalEnemies < 10)
            {
                enemySpeed = 12;
            }

            //Win
            if (totalEnemies < 1)
            {
                showGameOver("You Win, you saved the world!");
            }
        }

        
        //Key controls
        private void KeyIsDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                goLeft = true;
            }
            if (e.Key == Key.Right)
            {
                goRight = true;
            }
        }

        private void KeyIsUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                goLeft = false;
            }
            if (e.Key == Key.Right)
            {
                goRight = false;
            }

            if(e.Key == Key.Space)
            {
                //Bullets created and placed

                Rectangle newBullet = new Rectangle
                {
                    Tag = "bullet",
                    Height = 20,
                    Width = 5,
                    Fill = Brushes.White,
                    Stroke = Brushes.Red
                };

                

                Canvas.SetTop(newBullet, Canvas.GetTop(player) - newBullet.Height);
                Canvas.SetLeft(newBullet, Canvas.GetLeft(player) + player.Width / 2);

                myCanvas.Children.Add(newBullet);
            }

            //Play again logic
            if (e.Key == Key.Enter && gameOver == true)
            {
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
        }



        private void enemyBulletMaker(double x, double y)
        {
            //Creating enemy bullets  using GameLoop
            Rectangle enemyBullet = new Rectangle
            {

                Tag = "enemyBullet",
                Height = 40,
                Width = 15,
                Fill = Brushes.Green,
                Stroke = Brushes.Black,
                StrokeThickness = 5
            };

            Canvas.SetTop(enemyBullet, y);
            Canvas.SetLeft(enemyBullet, x);

            myCanvas.Children.Add(enemyBullet);
        }

        private void makeEnimies(int limit)
        {
            int left = 0;

            totalEnemies = limit;

            //Loop creating enemies
            for (int i = 0; i < limit; i++)
            {
                ImageBrush enemySkin = new ImageBrush();

                Rectangle newEnemy = new Rectangle
                {
                    Tag = "enemy",
                    Height = 45,
                    Width = 45,
                    Fill = enemySkin
                };

                //Setting the eneimies position
                Canvas.SetTop(newEnemy, 30);
                Canvas.SetLeft(newEnemy, left);
                myCanvas.Children.Add(newEnemy);
                left -= 60;

                enemyImages++;

                if (enemyImages > 8)
                {
                    enemyImages = 1;
                }

                //Alternating between enemy images
                switch (enemyImages)
                {
                    case 1:
                        enemySkin.ImageSource = new BitmapImage(new Uri(@"C:\Users\Admin\source\repos\SpaceInvader\SpaceInvader\images\invader1.gif"));
                        break;
                    case 2:
                        enemySkin.ImageSource = new BitmapImage(new Uri(@"C:\Users\Admin\source\repos\SpaceInvader\SpaceInvader\images\invader2.gif"));
                        break;
                    case 3:
                        enemySkin.ImageSource = new BitmapImage(new Uri(@"C:\Users\Admin\source\repos\SpaceInvader\SpaceInvader\images\invader3.gif"));
                        break;
                    case 4:
                        enemySkin.ImageSource = new BitmapImage(new Uri(@"C:\Users\Admin\source\repos\SpaceInvader\SpaceInvader\images\invader4.gif"));
                        break;
                    case 5:
                        enemySkin.ImageSource = new BitmapImage(new Uri(@"C:\Users\Admin\source\repos\SpaceInvader\SpaceInvader\images\invader5.gif"));
                        break;
                    case 6:
                        enemySkin.ImageSource = new BitmapImage(new Uri(@"C:\Users\Admin\source\repos\SpaceInvader\SpaceInvader\images\invader6.gif"));
                        break;
                    case 7:
                        enemySkin.ImageSource = new BitmapImage(new Uri(@"C:\Users\Admin\source\repos\SpaceInvader\SpaceInvader\images\invader7.gif"));
                        break;
                    case 8:
                        enemySkin.ImageSource = new BitmapImage(new Uri(@"C:\Users\Admin\source\repos\SpaceInvader\SpaceInvader\images\invader8.gif"));
                        break;


                }
            }
        }

        private void showGameOver(string msg)
        {
            gameOver = true;
            gameTimer.Stop();
            enemiesLeft.Content += " " + msg + " Press Enter to play again";
        }
    }
}
